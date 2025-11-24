using Assignment.Models;
using Assignment.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Services
{
    public class PromotionValidationService
    {
        private readonly HotelDbContext _context;

        public PromotionValidationService(HotelDbContext context)
        {
            _context = context;
        }

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
            // First, clean up any invalid promotions
            await DeactivateInvalidPromotionsAsync();

            var promotion = await _context.Promotions
                .Include(p => p.PromotionUsages)
                .FirstOrDefaultAsync(p => p.PromotionId == promotionId);

            if (promotion == null || !promotion.IsActive)
            {
                return (false, "Invalid or inactive promotion code.");
            }

            // Check date validity
            if (promotion.StartDate > DateTime.Now || promotion.EndDate < DateTime.Now)
            {
                // Auto-deactivate if expired
                if (promotion.EndDate < DateTime.Now)
                {
                    promotion.IsActive = false;
                    await _context.SaveChangesAsync();
                }
                return (false, "The selected promotion is not valid for the current date.");
            }

            // Check minimum criteria
            if (promotion.MinimumNights.HasValue && nights < promotion.MinimumNights.Value)
            {
                return (false, $"This promotion requires a minimum stay of {promotion.MinimumNights.Value} night(s).");
            }

            if (promotion.MinimumAmount.HasValue && totalAmount < promotion.MinimumAmount.Value)
            {
                return (false, $"This promotion requires a minimum amount of RM {promotion.MinimumAmount.Value:F2}.");
            }

            // Check maximum total uses
            if (promotion.MaxTotalUses.HasValue)
            {
                var totalUses = await _context.PromotionUsages
                    .CountAsync(pu => pu.PromotionId == promotionId);
                
                if (totalUses >= promotion.MaxTotalUses.Value)
                {
                    // Auto-deactivate if max usage reached
                    promotion.IsActive = false;
                    await _context.SaveChangesAsync();
                    return (false, "This promotion has reached its maximum usage limit.");
                }
            }

            // Check per-phone-number limit
            if (promotion.LimitPerPhoneNumber && !string.IsNullOrEmpty(phoneNumber))
            {
                var phoneHash = EncryptionService.Encrypt(phoneNumber);
                var phoneUses = await _context.PromotionUsages
                    .CountAsync(pu => pu.PromotionId == promotionId && pu.PhoneNumberHash == phoneHash);
                
                if (phoneUses >= promotion.MaxUsesPerLimit)
                {
                    return (false, "This promotion has already been used with this phone number.");
                }
            }

            // Check per-payment-card limit
            if (promotion.LimitPerPaymentCard && !string.IsNullOrEmpty(cardNumber))
            {
                var cardIdentifier = GetCardIdentifier(cardNumber);
                var cardUses = await _context.PromotionUsages
                    .CountAsync(pu => pu.PromotionId == promotionId && pu.CardIdentifier == cardIdentifier);
                
                if (cardUses >= promotion.MaxUsesPerLimit)
                {
                    return (false, "This promotion has already been used with this payment card.");
                }
            }

            // Check per-user-account limit
            if (promotion.LimitPerUserAccount)
            {
                var userUses = await _context.PromotionUsages
                    .CountAsync(pu => pu.PromotionId == promotionId && pu.UserId == userId);
                
                if (userUses >= promotion.MaxUsesPerLimit)
                {
                    return (false, "This promotion has already been used with your account.");
                }
            }

            // Check per-device/IP limit
            if (promotion.LimitPerDevice)
            {
                var deviceUses = await _context.PromotionUsages
                    .Where(pu => pu.PromotionId == promotionId)
                    .Where(pu => (deviceFingerprint != null && pu.DeviceFingerprint == deviceFingerprint) ||
                                 (ipAddress != null && pu.IpAddress == ipAddress))
                    .CountAsync();
                
                if (deviceUses >= promotion.MaxUsesPerLimit)
                {
                    return (false, "This promotion has already been used from this device or location.");
                }
            }

            return (true, string.Empty);
        }

        public async Task RecordPromotionUsageAsync(
            int promotionId,
            int bookingId,
            int userId,
            string? phoneNumber,
            string? cardNumber,
            string? deviceFingerprint,
            string? ipAddress)
        {
            var usage = new PromotionUsage
            {
                PromotionId = promotionId,
                BookingId = bookingId,
                UserId = userId,
                PhoneNumberHash = !string.IsNullOrEmpty(phoneNumber) ? EncryptionService.Encrypt(phoneNumber) : null,
                CardIdentifier = !string.IsNullOrEmpty(cardNumber) ? GetCardIdentifier(cardNumber) : null,
                DeviceFingerprint = deviceFingerprint,
                IpAddress = ipAddress,
                UsedAt = DateTime.Now
            };

            _context.PromotionUsages.Add(usage);
            await _context.SaveChangesAsync();

            // After recording usage, check if promotion should be deactivated
            var promotion = await _context.Promotions.FindAsync(promotionId);
            if (promotion != null && promotion.IsActive && promotion.MaxTotalUses.HasValue)
            {
                var totalUses = await _context.PromotionUsages
                    .CountAsync(pu => pu.PromotionId == promotionId);
                
                if (totalUses >= promotion.MaxTotalUses.Value)
                {
                    promotion.IsActive = false;
                    await _context.SaveChangesAsync();
                }
            }
        }

        private string GetCardIdentifier(string cardNumber)
        {
            // Remove spaces and get last 4 digits
            var cleaned = cardNumber.Replace(" ", "").Replace("-", "");
            if (cleaned.Length < 4) return cleaned;
            
            var last4 = cleaned.Substring(cleaned.Length - 4);
            // Create a hash of the full card number (for security, we don't store full number)
            var hash = EncryptionService.Encrypt(cleaned);
            return $"{last4}-{hash.Substring(0, 8)}";
        }

        /// <summary>
        /// Automatically deactivates promotions that have expired or reached their maximum usage limits.
        /// This should be called periodically or before loading promotions.
        /// </summary>
        public async Task<int> DeactivateInvalidPromotionsAsync()
        {
            var now = DateTime.Now;
            var deactivatedCount = 0;

            // Get all active promotions
            var activePromotions = await _context.Promotions
                .Where(p => p.IsActive)
                .ToListAsync();

            foreach (var promotion in activePromotions)
            {
                bool shouldDeactivate = false;
                string reason = string.Empty;

                // Check if expired
                if (promotion.EndDate < now)
                {
                    shouldDeactivate = true;
                    reason = "Expired";
                }
                // Check if not started yet (optional - you might want to keep these active)
                // else if (promotion.StartDate > now)
                // {
                //     shouldDeactivate = true;
                //     reason = "Not started yet";
                // }
                // Check if max total uses reached
                else if (promotion.MaxTotalUses.HasValue)
                {
                    var totalUses = await _context.PromotionUsages
                        .CountAsync(pu => pu.PromotionId == promotion.PromotionId);
                    
                    if (totalUses >= promotion.MaxTotalUses.Value)
                    {
                        shouldDeactivate = true;
                        reason = "Maximum usage reached";
                    }
                }

                if (shouldDeactivate)
                {
                    promotion.IsActive = false;
                    deactivatedCount++;
                }
            }

            if (deactivatedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return deactivatedCount;
        }
    }
}

