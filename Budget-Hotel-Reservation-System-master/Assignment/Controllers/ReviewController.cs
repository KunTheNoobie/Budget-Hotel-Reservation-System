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
            // ========== AUTHENTICATION CHECK ==========
            // Only authenticated users can submit reviews
            // This prevents anonymous users from creating fake reviews
            if (!AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login", "Security");
            }

            // ========== LOAD BOOKING ==========
            // Load the booking that is being reviewed
            // Include User navigation property to access user information
            var booking = await _context.Bookings
                .Include(b => b.User)  // Include user info (reviews are linked to booking, user info comes from booking)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            // Check if booking exists
            if (booking == null)
            {
                return NotFound();
            }

            // ========== OWNERSHIP VERIFICATION ==========
            // Verify that the current user owns this booking
            // Users can only review their own bookings (prevents fake reviews)
            var currentUserId = AuthenticationHelper.GetUserId(HttpContext);
            if (booking.UserId != currentUserId)
            {
                // User doesn't own this booking - return 403 Forbidden
                return Forbid();
            }

            // ========== BOOKING STATUS VALIDATION ==========
            // Only allow reviews for bookings that are confirmed or completed
            // This ensures only customers who have actually booked (and paid) can review
            // Pending bookings (not paid) and Cancelled bookings cannot be reviewed
            if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.CheckedOut && booking.Status != BookingStatus.CheckedIn)
            {
                TempData["Error"] = "You can only review confirmed or completed bookings.";
                return RedirectToAction("MyBookings", "Booking");
            }

            // ========== DUPLICATE REVIEW CHECK ==========
            // Check if a review already exists for this booking
            // Each booking can only have one review (prevents spam/duplicate reviews)
            // Note: Reviews are linked to Booking, not directly to User
            // User information is obtained from Booking.UserId
            if (await _context.Reviews.AnyAsync(r => r.BookingId == bookingId))
            {
                TempData["Error"] = "You have already reviewed this booking.";
                return RedirectToAction("MyBookings", "Booking");
            }

            // ========== CREATE REVIEW ==========
            // All validations passed, create the review
            var review = new Review
            {
                BookingId = bookingId,        // Link review to the booking
                Rating = rating,              // Rating (1-5 stars)
                Comment = comment,            // Optional review comment/feedback
                ReviewDate = DateTime.Now     // Timestamp when review was submitted
            };

            // Add review to database
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Show success message and redirect to booking history
            TempData["Success"] = "Thank you for your review!";
            return RedirectToAction("MyBookings", "Booking");
        }
    }
}