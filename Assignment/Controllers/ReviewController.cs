using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class ReviewController : Controller
    {
        private readonly HotelDbContext _context;

        public ReviewController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookingId, int rating, string comment)
        {
            if (!AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login", "Security");
            }

            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            // Verify user owns the booking
            var currentUserId = AuthenticationHelper.GetUserId(HttpContext);
            if (booking.UserId != currentUserId)
            {
                return Forbid();
            }

            // Verify booking is confirmed or completed (allow reviews after payment confirmation)
            if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.CheckedOut && booking.Status != BookingStatus.CheckedIn)
            {
                TempData["Error"] = "You can only review confirmed or completed bookings.";
                return RedirectToAction("MyBookings", "Booking");
            }

            // Check if review already exists for this booking by this user
            if (await _context.Reviews.AnyAsync(r => r.BookingId == bookingId && r.UserId == currentUserId))
            {
                TempData["Error"] = "You have already reviewed this booking.";
                return RedirectToAction("MyBookings", "Booking");
            }

            var review = new Review
            {
                BookingId = bookingId,
                UserId = currentUserId.Value,
                Rating = rating,
                Comment = comment,
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you for your review!";
            return RedirectToAction("MyBookings", "Booking");
        }
    }
}
