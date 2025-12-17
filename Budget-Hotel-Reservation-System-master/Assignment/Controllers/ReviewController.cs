using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    /// <summary>
    /// Controller for handling customer reviews and ratings.
    /// Allows authenticated users to submit reviews for completed bookings.
    /// Public access for viewing reviews, authentication required for creating reviews.
    /// </summary>
    public class ReviewController : Controller
    {
        /// <summary>
        /// Database context for accessing review and booking data.
        /// </summary>
        private readonly HotelDbContext _context;

        /// <summary>
        /// Initializes a new instance of the ReviewController.
        /// </summary>
        /// <param name="context">Database context for data access.</param>
        public ReviewController(HotelDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new review for a completed booking.
        /// Only allows users to review their own bookings.
        /// </summary>
        /// <param name="bookingId">ID of the booking to review.</param>
        /// <param name="rating">Rating value (1-5 stars).</param>
        /// <param name="comment">Optional review comment/feedback.</param>
        /// <returns>Redirects to booking history or returns error if validation fails.</returns>
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

            // Check if review already exists for this booking (user info from Booking.UserId)
            if (await _context.Reviews.AnyAsync(r => r.BookingId == bookingId))
            {
                TempData["Error"] = "You have already reviewed this booking.";
                return RedirectToAction("MyBookings", "Booking");
            }

            var review = new Review
            {
                BookingId = bookingId,
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