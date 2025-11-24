using Assignment.Attributes;
using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Services;
using Assignment.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace Assignment.Controllers
{
    [AuthorizeRole(UserRole.Customer, UserRole.Admin, UserRole.Manager, UserRole.Staff)]
    public class CustomerController : Controller
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<CustomerController> _logger;
        private readonly IWebHostEnvironment _environment;

        public CustomerController(HotelDbContext context, ILogger<CustomerController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
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
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            // Decrypt for display
            user.PhoneNumber = EncryptionService.Decrypt(user.PhoneNumber ?? "");

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
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            // Decrypt for display
            user.PhoneNumber = EncryptionService.Decrypt(user.PhoneNumber ?? "");

            // Set defaults if not set
            if (string.IsNullOrWhiteSpace(user.PreferredLanguage))
            {
                user.PreferredLanguage = "en-US";
            }
            if (string.IsNullOrWhiteSpace(user.Theme))
            {
                user.Theme = "Default";
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
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (existingUser == null)
            {
                return NotFound();
            }

            // Remove PasswordHash from validation since we handle it separately
            ModelState.Remove("PasswordHash");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                ModelState.AddModelError("FullName", "Full name is required.");
            }

            // Validate email format (even though it's read-only, we should validate it's valid)
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address.");
            }

            // Email cannot be changed - but we should verify it matches the existing email
            if (existingUser.Email != user.Email)
            {
                ModelState.AddModelError("Email", "Email cannot be changed.");
                // Restore original email
                user.Email = existingUser.Email;
            }

            // Validate phone number if provided
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                var phonePattern = @"^(\+?[0-9]{1,4}[-]?[0-9]{2,4}[-]?[0-9]{3,4}[-]?[0-9]{3,4}|[0-9]{7,15})$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(user.PhoneNumber, phonePattern))
                {
                    ModelState.AddModelError("PhoneNumber", "Please enter a valid phone number (e.g., +60-12-345-6789, 012-345-6789, or 0123456789).");
                }
            }

            // Validate Bio length
            if (!string.IsNullOrWhiteSpace(user.Bio) && user.Bio.Length > 500)
            {
                ModelState.AddModelError("Bio", "Bio must not exceed 500 characters.");
            }

            // Validate PreferredLanguage
            var validLanguages = new[] { "en-US", "en-GB", "ms-MY", "zh-CN", "zh-TW", "ja-JP", "ko-KR", "th-TH", "vi-VN", "id-ID" };
            if (string.IsNullOrWhiteSpace(user.PreferredLanguage))
            {
                user.PreferredLanguage = "en-US"; // Default
            }
            else if (!validLanguages.Contains(user.PreferredLanguage))
            {
                ModelState.AddModelError("PreferredLanguage", "Please select a valid language.");
            }

            // Validate Theme
            var validThemes = new[] { "Default", "Dark", "Light", "Auto" };
            if (string.IsNullOrWhiteSpace(user.Theme))
            {
                user.Theme = "Default"; // Default
            }
            else if (!validThemes.Contains(user.Theme))
            {
                ModelState.AddModelError("Theme", "Please select a valid theme.");
            }

            // Validate new password if provided
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword.Length < 8)
                {
                    ModelState.AddModelError("NewPassword", "Password must be at least 8 characters.");
                    ViewData["NewPasswordError"] = "Password must be at least 8 characters.";
                }
                else
                {
                    var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
                    if (!System.Text.RegularExpressions.Regex.IsMatch(newPassword, passwordPattern))
                    {
                        ModelState.AddModelError("NewPassword", "Password must contain uppercase, lowercase, number, and special character (@$!%*?&).");
                        ViewData["NewPasswordError"] = "Password must contain uppercase, lowercase, number, and special character (@$!%*?&).";
                    }
                }
            }

            if (ModelState.IsValid)
            {
                existingUser.FullName = user.FullName;
                // Don't update email - it cannot be changed
                // existingUser.Email = user.Email; // Removed - email is read-only
                // Encrypt phone number
                existingUser.PhoneNumber = EncryptionService.Encrypt(user.PhoneNumber ?? "");
                
                // Update new properties merged from UserProfile
                // Don't update ProfilePictureUrl here - it's managed separately via UploadProfilePicture action
                // existingUser.ProfilePictureUrl = user.ProfilePictureUrl; // Removed to prevent overwriting
                existingUser.Bio = user.Bio;
                existingUser.PreferredLanguage = user.PreferredLanguage;
                existingUser.Theme = user.Theme;

                if (!string.IsNullOrEmpty(newPassword) && newPassword.Length >= 8)
                {
                    existingUser.PasswordHash = PasswordService.HashPassword(newPassword);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }

            // Preserve existing user data for display if validation fails
            // Preserve profile picture URL and other fields that aren't in the form
            user.ProfilePictureUrl = existingUser.ProfilePictureUrl;
            user.UserId = existingUser.UserId;
            user.Role = existingUser.Role;
            user.IsEmailVerified = existingUser.IsEmailVerified;
            user.IsActive = existingUser.IsActive;
            user.CreatedAt = existingUser.CreatedAt;
            
            // Decrypt phone number for display if validation fails
            user.PhoneNumber = EncryptionService.Decrypt(user.PhoneNumber ?? "");
            
            // Set defaults if not set
            if (string.IsNullOrWhiteSpace(user.PreferredLanguage))
            {
                user.PreferredLanguage = "en-US";
            }
            if (string.IsNullOrWhiteSpace(user.Theme))
            {
                user.Theme = "Default";
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var userId = AuthenticationHelper.GetUserId(HttpContext);
                var user = await _context.Users.FindAsync(userId);

                if (user == null) return NotFound();

                // Ensure directory exists
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + profilePicture.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(fileStream);
                }

                user.ProfilePictureUrl = "/uploads/profiles/" + uniqueFileName;
                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Profile picture updated successfully!";
            }
            else
            {
                TempData["Error"] = "Please select a valid image file.";
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Security");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Delete physical file if it exists and is local
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && user.ProfilePictureUrl.StartsWith("/uploads/"))
            {
                var filePath = Path.Combine(_environment.WebRootPath, user.ProfilePictureUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            user.ProfilePictureUrl = null;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Profile picture deleted successfully.";
            return RedirectToAction("Profile");
        }
    }
}
