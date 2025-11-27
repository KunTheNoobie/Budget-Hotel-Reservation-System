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
                // Rate limiting: Check for too many contact form submissions from same IP
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var recentAttempts = _context.SecurityLogs
                    .Count(s => s.IPAddress == ipAddress && 
                                s.Action == "ContactForm" && 
                                s.Timestamp > DateTime.Now.AddMinutes(-5));
                
                if (recentAttempts >= 5)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Too many contact form submissions. Please try again after 5 minutes." });
                    }
                    TempData["Error"] = "Too many contact form submissions. Please try again after 5 minutes.";
                    return RedirectToAction("Contact", "Home");
                }

                if (ModelState.IsValid)
                {
                    model.SentAt = DateTime.Now;
                    model.IsRead = false;
                    _context.ContactMessages.Add(model);
                    await _context.SaveChangesAsync();

                    // Log contact form submission
                    _context.SecurityLogs.Add(new SecurityLog
                    {
                        Action = "ContactForm",
                        UserId = null,
                        IPAddress = ipAddress,
                        Details = $"Contact form submitted from {model.Email}",
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
