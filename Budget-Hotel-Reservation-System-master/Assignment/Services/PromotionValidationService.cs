using Assignment.Models;
using Assignment.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Services
{
    /// <summary>
    /// Service for validating and managing promotion code usage.
    /// Implements comprehensive validation rules including date ranges, minimum requirements,
    /// usage limits, and abuse prevention mechanisms (per phone number, payment card, device, or user account).
    /// Automatically deactivates expired or fully-used promotions.
    /// </summary>
    public class PromotionValidationService
    {
        /// <summary>
        /// Database context for accessing Promotions and Bookings tables.
        /// Promotion usage tracking is now stored in the Booking table.
        /// </summary>
        private readonly HotelDbContext _context;

        /// <summary>
        /// Initializes a new instance of the PromotionValidationService with the provided database context.
        /// </summary>
        /// <param name="context">The database context for accessing promotions data.</param>
        public PromotionValidationService(HotelDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Validates whether a promotion can be used for a specific booking.
        /// Checks all validation rules including dates, minimum requirements, usage limits, and abuse prevention.
        /// </summary>
        /// <param name="promotionId">The ID of the promotion to validate.</param>
        /// <param name="userId">The ID of the user attempting to use the promotion.</param>
        /// <param name="phoneNumber">Optional phone number used in the booking (for abuse prevention).</param>
        /// <param name="cardNumber">Optional payment card number (for abuse prevention).</param>
        /// <param name="deviceFingerprint">Optional device fingerprint (for abuse prevention).</param>
        /// <param name="ipAddress">Optional IP address (for abuse prevention).</param>
        /// <param name="totalAmount">Total booking amount (for minimum amount validation).</param>
        /// <param name="nights">Number of nights in the booking (for minimum nights validation).</param>
        /// <returns>
        /// A tuple containing:
        /// - IsValid: True if the promotion can be used, false otherwise.
        /// - ErrorMessage: Error message explaining why validation failed (empty if valid).
        /// </returns>
        public async Task<(bool IsValid, string ErrorMessage)> ValidatePromotionUsageAsync(
            int promotionId,
            int userId,
            string? phoneNumber,
            string? cardNumber,
            string? deviceFingerprint,
            string? ipAddress,
            decimal totalAmount,
            int nights)
        {
            // ========== STEP 1: CLEANUP INVALID PROMOTIONS ==========
            // Before validating, clean up any promotions that are expired or have reached max uses
            // This ensures we're working with current, valid promotions
            await DeactivateInvalidPromotionsAsync();

            // ========== STEP 2: LOAD PROMOTION FROM DATABASE ==========
            // Retrieve the promotion record from database using the promotion ID
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.PromotionId == promotionId);

            // ========== STEP 3: CHECK PROMOTION EXISTS AND IS ACTIVE ==========
            // Verify that the promotion exists in database and is currently active
            // Inactive promotions cannot be used even if they exist
            if (promotion == null || !promotion.IsActive)
            {
                return (false, "Invalid or inactive promotion code.");
            }

            // ========== STEP 4: CHECK DATE VALIDITY ==========
            // Verify that current date is within promotion's valid date range
            // Promotion must have started (StartDate <= Now) and not expired (EndDate >= Now)
            if (promotion.StartDate > DateTime.Now || promotion.EndDate < DateTime.Now)
            {
                // If promotion has expired, automatically deactivate it
                // This prevents future validation attempts on expired promotions
                if (promotion.EndDate < DateTime.Now)
                {
                    promotion.IsActive = false;
                    await _context.SaveChangesAsync();
                }
                return (false, "The selected promotion is not valid for the current date.");
            }

            // ========== STEP 5: CHECK MINIMUM REQUIREMENTS ==========
            // Some promotions require minimum booking amount or minimum number of nights
            // This ensures promotions are only used for qualifying bookings
            
            // Check minimum nights requirement (e.g., "Stay 3 nights or more")
            if (promotion.MinimumNights.HasValue && nights < promotion.MinimumNights.Value)
            {
                return (false, $"This promotion requires a minimum stay of {promotion.MinimumNights.Value} night(s).");
            }

            // Check minimum amount requirement (e.g., "Bookings over RM500")
            if (promotion.MinimumAmount.HasValue && totalAmount < promotion.MinimumAmount.Value)
            {
                return (false, $"This promotion requires a minimum amount of RM {promotion.MinimumAmount.Value:F2}.");
            }

            // ========== STEP 6: CHECK MAXIMUM TOTAL USES ==========
            // Some promotions have a global limit (e.g., "First 100 customers only")
            // Count how many times this promotion has been used across ALL users
            if (promotion.MaxTotalUses.HasValue)
            {
                // Count bookings where this promotion was used (PromotionUsedAt is not null)
                var totalUses = await _context.Bookings
                    .CountAsync(b => b.PromotionId == promotionId && b.PromotionUsedAt != null);
                
                // If maximum uses reached, promotion is no longer valid
                if (totalUses >= promotion.MaxTotalUses.Value)
                {
                    // Automatically deactivate promotion when max uses reached
                    // This prevents further attempts to use an exhausted promotion
                    promotion.IsActive = false;
                    await _context.SaveChangesAsync();
                    return (false, "This promotion has reached its maximum usage limit.");
                }
            }

            // ========== STEP 7: CHECK PER-PHONE-NUMBER LIMIT (ABUSE PREVENTION) ==========
            // Prevent same person from using promotion multiple times with different accounts
            // Phone number is encrypted before comparison (privacy protection)
            if (promotion.LimitPerPhoneNumber && !string.IsNullOrEmpty(phoneNumber))
            {
                // Encrypt phone number to match format stored in database
                var phoneHash = EncryptionService.Encrypt(phoneNumber);
                
                // Count how many times this promotion was used with this phone number
                var phoneUses = await _context.Bookings
                    .CountAsync(b => b.PromotionId == promotionId && 
                                   b.PromotionPhoneNumberHash == phoneHash && 
                                   b.PromotionUsedAt != null);
                
                // If limit reached, prevent further use
                if (phoneUses >= promotion.MaxUsesPerLimit)
                {
                    return (false, "This promotion has already been used with this phone number.");
                }
            }

            // ========== STEP 8: CHECK PER-PAYMENT-CARD LIMIT (ABUSE PREVENTION) ==========
            // Prevent same payment card from being used multiple times with same promotion
            // Card number is hashed (not stored in full) for security
            if (promotion.LimitPerPaymentCard && !string.IsNullOrEmpty(cardNumber))
            {
                // Create card identifier (last 4 digits + hash of full number)
                var cardIdentifier = GetCardIdentifier(cardNumber);
                
                // Count how many times this promotion was used with this payment card
                var cardUses = await _context.Bookings
                    .CountAsync(b => b.PromotionId == promotionId && 
                                   b.PromotionCardIdentifier == cardIdentifier && 
                                   b.PromotionUsedAt != null);
                
                // If limit reached, prevent further use
                if (cardUses >= promotion.MaxUsesPerLimit)
                {
                    return (false, "This promotion has already been used with this payment card.");
                }
            }

            // ========== STEP 9: CHECK PER-USER-ACCOUNT LIMIT (ABUSE PREVENTION) ==========
            // Prevent same user account from using promotion more than allowed times
            // This is the simplest abuse prevention mechanism
            if (promotion.LimitPerUserAccount)
            {
                // Count how many times this user has used this promotion
                var userUses = await _context.Bookings
                    .CountAsync(b => b.PromotionId == promotionId && 
                                   b.UserId == userId && 
                                   b.PromotionUsedAt != null);
                
                // If limit reached, prevent further use
                if (userUses >= promotion.MaxUsesPerLimit)
                {
                    return (false, "This promotion has already been used with your account.");
                }
            }

            // ========== STEP 10: CHECK PER-DEVICE/IP LIMIT (ABUSE PREVENTION) ==========
            // Prevent same device or IP address from using promotion multiple times
            // This catches users trying to bypass limits by creating multiple accounts
            if (promotion.LimitPerDevice)
            {
                // Count bookings where same device fingerprint OR same IP address was used
                // This catches both device-based and location-based abuse attempts
                var deviceUses = await _context.Bookings
                    .Where(b => b.PromotionId == promotionId && b.PromotionUsedAt != null)
                    .Where(b => (deviceFingerprint != null && b.PromotionDeviceFingerprint == deviceFingerprint) ||
                                (ipAddress != null && b.PromotionIpAddress == ipAddress))
                    .CountAsync();
                
                // If limit reached, prevent further use
                if (deviceUses >= promotion.MaxUsesPerLimit)
                {
                    return (false, "This promotion has already been used from this device or location.");
                }
            }

            // ========== ALL VALIDATION CHECKS PASSED ==========
            // Promotion is valid and can be used for this booking
            // Return success with empty error message
            return (true, string.Empty);
        }

        /// <summary>
        /// Records that a promotion was used in a booking.
        /// Stores tracking information (phone number, card, device, IP) in the Booking table for abuse prevention.
        /// Automatically deactivates the promotion if maximum total uses is reached.
        /// </summary>
        /// <param name="promotionId">The ID of the promotion that was used.</param>
        /// <param name="bookingId">The ID of the booking where the promotion was applied.</param>
        /// <param name="userId">The ID of the user who used the promotion.</param>
        /// <param name="phoneNumber">Optional phone number used in the booking (encrypted before storage).</param>
        /// <param name="cardNumber">Optional payment card number (hashed before storage).</param>
        /// <param name="deviceFingerprint">Optional device fingerprint.</param>
        /// <param name="ipAddress">Optional IP address.</param>
        public async Task RecordPromotionUsageAsync(
            int promotionId,
            int bookingId,
            int userId,
            string? phoneNumber,
            string? cardNumber,
            string? deviceFingerprint,
            string? ipAddress)
        {
            // ========== RECORD PROMOTION USAGE TRACKING ==========
            // Store tracking information in the booking record for abuse prevention
            // This information is used to enforce usage limits in future validations
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking != null)
            {
                // Link promotion to booking
                booking.PromotionId = promotionId;
                
                // Store encrypted phone number (if provided) for per-phone-number limit checking
                booking.PromotionPhoneNumberHash = !string.IsNullOrEmpty(phoneNumber) ? EncryptionService.Encrypt(phoneNumber) : null;
                
                // Store hashed card identifier (if provided) for per-card limit checking
                // Card identifier includes last 4 digits + hash of full number (for security)
                booking.PromotionCardIdentifier = !string.IsNullOrEmpty(cardNumber) ? GetCardIdentifier(cardNumber) : null;
                
                // Store device fingerprint for per-device limit checking
                booking.PromotionDeviceFingerprint = deviceFingerprint;
                
                // Store IP address for per-location limit checking
                booking.PromotionIpAddress = ipAddress;
                
                // Record timestamp when promotion was used
                booking.PromotionUsedAt = DateTime.Now;
            }

            // Save changes to database
            await _context.SaveChangesAsync();

            // After recording usage, check if promotion should be deactivated
            var promotion = await _context.Promotions.FindAsync(promotionId);
            if (promotion != null && promotion.IsActive && promotion.MaxTotalUses.HasValue)
            {
                var totalUses = await _context.Bookings
                    .CountAsync(b => b.PromotionId == promotionId && b.PromotionUsedAt != null);
                
                if (totalUses >= promotion.MaxTotalUses.Value)
                {
                    promotion.IsActive = false;
                    await _context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Creates a secure identifier for a payment card number.
        /// Always encrypts the card identifier to prevent storing any card information in plaintext.
        /// For cards with 4+ digits: stores last 4 digits and encrypted hash.
        /// For cards with less than 4 digits: stores only encrypted hash (no plaintext digits).
        /// </summary>
        /// <param name="cardNumber">The full payment card number.</param>
        /// <returns>
        /// For cards with 4+ digits: "XXXX-HASH" where XXXX is last 4 digits and HASH is encrypted identifier.
        /// For cards with less than 4 digits: "HASH" (fully encrypted, no plaintext digits).
        /// </returns>
        private string GetCardIdentifier(string cardNumber)
        {
            // Remove spaces and dashes from card number
            var cleaned = cardNumber.Replace(" ", "").Replace("-", "");
            
            // Always encrypt the full card number for security
            var hash = EncryptionService.Encrypt(cleaned);
            
            // For cards with 4+ digits, include last 4 digits for display purposes
            // For shorter cards, return only the encrypted hash (no plaintext)
            if (cleaned.Length >= 4)
            {
                // Get last 4 digits (commonly shown on receipts)
                var last4 = cleaned.Substring(cleaned.Length - 4);
                // Return format: "XXXX-HASH" where XXXX is last 4 digits and HASH is encrypted identifier
                return $"{last4}-{hash.Substring(0, 8)}";
            }
            else
            {
                // For short card numbers, return only encrypted hash (no plaintext digits)
                // This ensures no card information is stored in plaintext, regardless of input length
                return hash.Substring(0, Math.Min(16, hash.Length));
            }
        }

        /// <summary>
        /// Automatically deactivates promotions that have expired or reached their maximum usage limits.
        /// This should be called periodically (e.g., on application startup or via a scheduled task)
        /// to keep the promotions table clean and ensure expired promotions are not shown to users.
        /// </summary>
        /// <returns>The number of promotions that were deactivated.</returns>
        public async Task<int> DeactivateInvalidPromotionsAsync()
        {
            var now = DateTime.Now;
            var deactivatedCount = 0;

            // Get all active promotions that might need deactivation
            var activePromotions = await _context.Promotions
                .Where(p => p.IsActive)
                .ToListAsync();

            foreach (var promotion in activePromotions)
            {
                bool shouldDeactivate = false;
                string reason = string.Empty;

                // Check if promotion has expired (end date has passed)
                if (promotion.EndDate < now)
                {
                    shouldDeactivate = true;
                    reason = "Expired";
                }
                // Note: We don't deactivate promotions that haven't started yet
                // (they will become active when StartDate is reached)
                // Check if maximum total uses has been reached
                else if (promotion.MaxTotalUses.HasValue)
                {
                    var totalUses = await _context.Bookings
                        .CountAsync(b => b.PromotionId == promotion.PromotionId && b.PromotionUsedAt != null);
                    
                    if (totalUses >= promotion.MaxTotalUses.Value)
                    {
                        shouldDeactivate = true;
                        reason = "Maximum usage reached";
                    }
                }

                // Deactivate the promotion if any condition is met
                if (shouldDeactivate)
                {
                    promotion.IsActive = false;
                    deactivatedCount++;
                }
            }

            // Save changes if any promotions were deactivated
            if (deactivatedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return deactivatedCount;
        }
    }
}

