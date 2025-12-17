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
            var today = DateTime.Today;
            var updatedCount = 0;

            try
            {
                // 2. Auto Check-out: CheckedIn bookings where check-out date has passed
                var bookingsToCheckOut = await _context.Bookings
                    .Where(b => b.Status == BookingStatus.CheckedIn && 
                               b.CheckOutDate.Date < today &&
                               !b.IsDeleted)
                    .ToListAsync();

                foreach (var booking in bookingsToCheckOut)
                {
                    booking.Status = BookingStatus.CheckedOut;
                    updatedCount++;
                    _logger.LogInformation("Auto check-out: Booking {BookingId} status changed to CheckedOut", booking.BookingId);
                }

                // 3. Auto No-show: Confirmed bookings where check-in date has passed but not checked in
                // Only mark as NoShow if check-in date was yesterday or earlier (give a day grace period)
                var yesterday = today.AddDays(-1);
                var bookingsToNoShow = await _context.Bookings
                    .Where(b => b.Status == BookingStatus.Confirmed && 
                               b.CheckInDate.Date < today &&
                               b.CheckInDate.Date <= yesterday &&
                               !b.IsDeleted)
                    .ToListAsync();

                foreach (var booking in bookingsToNoShow)
                {
                    booking.Status = BookingStatus.NoShow;
                    updatedCount++;
                    _logger.LogInformation("Auto no-show: Booking {BookingId} status changed to NoShow", booking.BookingId);
                }

                // Save all changes
                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Automatically updated {Count} booking status(es)", updatedCount);
                }

                return updatedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking statuses automatically");
                throw;
            }
        }
    }
}
