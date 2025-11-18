using Assignment.Attributes;
using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Services;
using Assignment.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    [AuthorizeRole(UserRole.Customer, UserRole.Admin, UserRole.Manager, UserRole.Staff)]
    public class CustomerController : Controller
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(HotelDbContext context, ILogger<CustomerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Security");
            }

            var user = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Security");
            }

            var user = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User user, string? newPassword)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Security");
            }

            var existingUser = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (existingUser == null)
            {
                return NotFound();
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.UserId != userId.Value))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;

                if (!string.IsNullOrEmpty(newPassword) && newPassword.Length >= 8)
                {
                    existingUser.PasswordHash = PasswordService.HashPassword(newPassword);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }

            return View(user);
        }
    }
}

