using Assignment.Models;
using Assignment.Models.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Assignment.Controllers
{
    /// <summary>
    /// Controller for handling contact form submissions and newsletter subscriptions.
    /// Implements rate limiting to prevent spam and abuse. Public access (no authentication required).
    /// </summary>
    public class ContactController : Controller
    {
        /// <summary>
        /// Database context for accessing contact messages and newsletter data.
        /// </summary>
        private readonly HotelDbContext _context;

        /// <summary>
        /// Logger for recording contact form submissions and errors.
        /// </summary>
        private readonly ILogger<ContactController> _logger;

        /// <summary>
        /// Initializes a new instance of the ContactController.
        /// </summary>
        /// <param name="context">Database context for data access.</param>
        /// <param name="logger">Logger instance for logging.</param>
        public ContactController(HotelDbContext context, ILogger<ContactController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Displays the contact form page.
        /// </summary>
        /// <returns>The contact form view.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Processes contact form submissions with rate limiting protection.
        /// Prevents spam by limiting submissions from the same IP address.
        /// </summary>
        /// <param name="model">The contact message data submitted by the user.</param>
        /// <returns>Redirects to contact page with success message or returns view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(ContactMessage model)
        {
            try
            {
                // ========== RATE LIMITING PROTECTION ==========
                // Prevent spam by limiting contact form submissions from same IP address
                // This protects against automated bots flooding the contact form
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                
                // Count how many contact form submissions from this IP in the last 5 minutes
                var recentAttempts = _context.SecurityLogs
                    .Count(s => s.IPAddress == ipAddress &&           // Same IP address
                                s.Action == "ContactForm" &&          // Contact form action
                                s.Timestamp > DateTime.Now.AddMinutes(-5)); // Within last 5 minutes
                
                // If too many attempts (5 or more), block the submission
                if (recentAttempts >= 5)
                {
                    // Check if this is an AJAX request (for AJAX form submissions)
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        // Return JSON response for AJAX requests
                        return Json(new { success = false, message = "Too many contact form submissions. Please try again after 5 minutes." });
                    }
                    
                    // Return error message for regular form submissions
                    TempData["Error"] = "Too many contact form submissions. Please try again after 5 minutes.";
                    return RedirectToAction("Contact", "Home");
                }

                // ========== VALIDATE AND SAVE CONTACT MESSAGE ==========
                // Check if form data is valid (required fields, email format, etc.)
                if (ModelState.IsValid)
                {
                    // Set timestamp when message was sent
                    model.SentAt = DateTime.Now;
                    
                    // Mark message as unread (admin hasn't read it yet)
                    model.IsRead = false;
                    
                    // Add message to database
                    _context.ContactMessages.Add(model);
                    await _context.SaveChangesAsync();

                    // ========== LOG CONTACT FORM SUBMISSION ==========
                    // Record the submission in security logs for rate limiting and audit trail
                    _context.SecurityLogs.Add(new SecurityLog
                    {
                        Action = "ContactForm",                    // Action type
                        UserId = null,                            // No user ID (public form)
                        IPAddress = ipAddress,                    // Store IP for rate limiting
                        Details = $"Contact form submitted from {model.Email}", // Additional details
                        Timestamp = DateTime.Now
                    });
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Your message has been sent successfully! We'll get back to you soon." });
                    }
                    
                    TempData["Success"] = "Your message has been sent successfully! We'll get back to you soon.";
                    return RedirectToAction("Contact", "Home");
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, message = string.Join(" ", errors) });
                }

                return RedirectToAction("Contact", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contact message from {Email}", model?.Email ?? "unknown");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while sending your message. Please try again later." });
                }
                
                TempData["Error"] = "An error occurred while sending your message. Please try again later.";
                return RedirectToAction("Contact", "Home");
            }
        }
    }
}
