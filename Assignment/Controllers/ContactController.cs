using Assignment.Models;
using Assignment.Models.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Assignment.Controllers
{
    public class ContactController : Controller
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<ContactController> _logger;

        public ContactController(HotelDbContext context, ILogger<ContactController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(ContactMessage model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.SentAt = DateTime.Now;
                    model.IsRead = false;
                    _context.ContactMessages.Add(model);
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
