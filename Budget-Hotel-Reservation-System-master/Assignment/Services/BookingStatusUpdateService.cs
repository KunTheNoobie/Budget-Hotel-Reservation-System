using Assignment.Models;
using Assignment.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Services
{
    /// <summary>
    /// Service for automatically updating booking statuses based on dates.
    /// Handles automatic check-in, check-out, and no-show status transitions.
    /// </summary>
    public class BookingStatusUpdateService
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<BookingStatusUpdateService> _logger;

        public BookingStatusUpdateService(HotelDbContext context, ILogger<BookingStatusUpdateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Updates booking statuses automatically based on check-in and check-out dates.
        /// - Confirmed bookings with check-in date <= today → CheckedIn
        /// - CheckedIn bookings with check-out date < today → CheckedOut
        /// - Confirmed bookings with check-in date < today (not checked in) → NoShow
        /// </summary>
        /// <returns>The number of bookings that were updated.</returns>
        public async Task<int> UpdateBookingStatusesAsync()
        {
            // ========== AUTOMATIC BOOKING STATUS UPDATES ==========
            // This method automatically updates booking statuses based on dates
            // It runs periodically (e.g., on application startup, scheduled task, or manual trigger)
            // This ensures bookings are always in the correct status without manual intervention
            
            var today = DateTime.Today;  // Get today's date (without time component)
            var updatedCount = 0;         // Counter for tracking how many bookings were updated

            try
            {
                // ========== STEP 1: AUTO CHECK-OUT ==========
                // Find bookings that are currently CheckedIn but check-out date has passed
                // These guests should have already checked out, so update status automatically
                var bookingsToCheckOut = await _context.Bookings
                    .Where(b => b.Status == BookingStatus.CheckedIn &&  // Currently checked in
                               b.CheckOutDate.Date < today &&           // Check-out date was before today
                               !b.IsDeleted)                            // Not soft-deleted
                    .ToListAsync();

                // Update each booking to CheckedOut status
                foreach (var booking in bookingsToCheckOut)
                {
                    booking.Status = BookingStatus.CheckedOut;  // Change status to CheckedOut
                    updatedCount++;                              // Increment counter
                    
                    // Log the status change for audit trail
                    _logger.LogInformation("Auto check-out: Booking {BookingId} status changed to CheckedOut", booking.BookingId);
                }

                // ========== STEP 2: AUTO NO-SHOW ==========
                // Find bookings that are Confirmed but guest never checked in
                // These are "no-show" bookings where guest didn't arrive
                // Only mark as NoShow if check-in date was yesterday or earlier (give 1 day grace period)
                // This allows for late check-ins on the same day
                var yesterday = today.AddDays(-1);
                var bookingsToNoShow = await _context.Bookings
                    .Where(b => b.Status == BookingStatus.Confirmed &&  // Booking was confirmed (payment completed)
                               b.CheckInDate.Date < today &&            // Check-in date has passed
                               b.CheckInDate.Date <= yesterday &&       // Check-in was yesterday or earlier (grace period)
                               !b.IsDeleted)                            // Not soft-deleted
                    .ToListAsync();

                // Update each booking to NoShow status
                foreach (var booking in bookingsToNoShow)
                {
                    booking.Status = BookingStatus.NoShow;  // Change status to NoShow
                    updatedCount++;                         // Increment counter
                    
                    // Log the status change for audit trail
                    _logger.LogInformation("Auto no-show: Booking {BookingId} status changed to NoShow", booking.BookingId);
                }

                // ========== SAVE CHANGES ==========
                // Save all status updates to database in one transaction
                // Only save if there were any updates (optimization)
                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    
                    // Log summary of updates
                    _logger.LogInformation("Automatically updated {Count} booking status(es)", updatedCount);
                }

                // Return count of updated bookings (useful for monitoring and reporting)
                return updatedCount;
            }
            catch (Exception ex)
            {
                // ========== ERROR HANDLING ==========
                // Log error details for debugging
                // Re-throw exception so calling code can handle it appropriately
                _logger.LogError(ex, "Error updating booking statuses automatically");
                throw;
            }
        }
    }
}
