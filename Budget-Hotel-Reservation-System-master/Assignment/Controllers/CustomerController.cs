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
    /// <summary>
    /// Controller for customer profile management and personal features.
    /// Handles profile viewing/editing, password changes, booking history,
    /// and profile picture uploads. Requires authentication (Customer, Admin, Manager, or Staff roles).
    /// </summary>
    [AuthorizeRole(UserRole.Customer, UserRole.Admin, UserRole.Manager, UserRole.Staff)]
    public class CustomerController : Controller
    {
        /// <summary>
        /// Database context for accessing user and booking data.
        /// </summary>
        private readonly HotelDbContext _context;

        /// <summary>
        /// Logger for recording customer operations and errors.
        /// </summary>
        private readonly ILogger<CustomerController> _logger;

        /// <summary>
        /// Web host environment for file operations (profile picture uploads).
        /// </summary>
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Initializes a new instance of the CustomerController.
        /// </summary>
        /// <param name="context">Database context for data access.</param>
        /// <param name="logger">Logger instance for logging.</param>
        /// <param name="environment">Web host environment for file operations.</param>
        public CustomerController(HotelDbContext context, ILogger<CustomerController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Displays the user's profile page with personal information and booking history.
        /// </summary>
        /// <returns>The profile view for the authenticated user.</returns>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Security");
            }

            var user = await _context.Users
                .AsNoTracking()
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
                .AsNoTracking()
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

            // Log for debugging
            _logger.LogInformation("EditProfile GET - User {UserId}, ProfilePictureUrl: {Url}", userId, user.ProfilePictureUrl ?? "NULL");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User user, string? newPassword, IFormFile? profilePicture, string? deleteImage)
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

            // Validate profile picture if provided
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
                
                // Validate file extension only (no size or dimension restrictions)
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ProfilePicture", "Invalid file type. Please upload JPG, PNG, GIF, or WebP images only.");
                }
            }

            if (ModelState.IsValid)
            {
                // Handle profile picture FIRST before other updates
                if (deleteImage == "true")
                {
                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(existingUser.ProfilePictureUrl) && existingUser.ProfilePictureUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, existingUser.ProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    existingUser.ProfilePictureUrl = null;
                }
                else if (profilePicture != null && profilePicture.Length > 0)
                {
                    // File validation already done above, so we can proceed with upload
                    var fileExtension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
                    
                    // Ensure directory exists
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(existingUser.ProfilePictureUrl) && existingUser.ProfilePictureUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, existingUser.ProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Generate unique filename and save file
                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePicture.CopyToAsync(fileStream);
                    }

                    // Update profile picture URL - simple assignment like hotels do
                    existingUser.ProfilePictureUrl = "/uploads/profiles/" + uniqueFileName;
                }
                
                // Update other properties
                existingUser.FullName = user.FullName;
                existingUser.PhoneNumber = EncryptionService.Encrypt(user.PhoneNumber ?? "");
                existingUser.Bio = user.Bio;
                existingUser.PreferredLanguage = user.PreferredLanguage;
                existingUser.Theme = user.Theme;

                if (!string.IsNullOrEmpty(newPassword) && newPassword.Length >= 8)
                {
                    existingUser.PasswordHash = PasswordService.HashPassword(newPassword);
                }

                // Save all changes
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
            try
            {
                if (profilePicture == null || profilePicture.Length == 0)
                {
                    TempData["Error"] = "Please select a valid image file.";
                    return RedirectToAction("Profile");
                }

                // Validate file type only (no size or dimension restrictions)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["Error"] = "Invalid file type. Please upload JPG, PNG, GIF, or WebP images only.";
                    return RedirectToAction("Profile");
                }

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

                // Store old URL before updating
                var oldProfilePictureUrl = user.ProfilePictureUrl;

                // Ensure directory exists
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Delete old profile picture if it exists and is local
                if (!string.IsNullOrEmpty(oldProfilePictureUrl) && oldProfilePictureUrl.StartsWith("/uploads/"))
                {
                    try
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, oldProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                            _logger.LogInformation("Deleted old profile picture file: {Path}", oldFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old profile picture: {Path}", oldProfilePictureUrl);
                        // Continue with upload even if old file deletion fails
                    }
                }

                // Generate unique filename
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(fileStream);
                }

                // Verify file was created
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogError("Profile picture file was not created at: {Path}", filePath);
                    TempData["Error"] = "Failed to save profile picture file. Please try again.";
                    return RedirectToAction("EditProfile");
                }

                // Update profile picture URL - explicitly mark property as modified
                var newProfilePictureUrl = "/uploads/profiles/" + uniqueFileName;
                user.ProfilePictureUrl = newProfilePictureUrl;
                
                // Explicitly mark the property as modified
                _context.Entry(user).Property(u => u.ProfilePictureUrl).IsModified = true;
                
                var result = await _context.SaveChangesAsync();
                
                if (result > 0)
                {
                    _logger.LogInformation("Profile picture updated successfully for user {UserId}. URL: {Url}", userId, newProfilePictureUrl);
                    TempData["Success"] = "Profile picture updated successfully!";
                }
                else
                {
                    _logger.LogWarning("No changes saved for user {UserId} profile picture. Attempted URL: {Url}", userId, newProfilePictureUrl);
                    TempData["Error"] = "Failed to update profile picture in database. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture");
                TempData["Error"] = "An error occurred while uploading your profile picture. Please try again.";
            }

            return RedirectToAction("EditProfile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            try
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
                    try
                    {
                        var filePath = Path.Combine(_environment.WebRootPath, user.ProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                            _logger.LogInformation("Deleted profile picture file: {Path}", filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete profile picture file: {Path}", user.ProfilePictureUrl);
                        // Continue with database update even if file deletion fails
                    }
                }

                // Remove from database
                user.ProfilePictureUrl = null;
                _context.Users.Update(user);
                var result = await _context.SaveChangesAsync();
                
                if (result > 0)
                {
                    _logger.LogInformation("Profile picture removed from database for user {UserId}", userId);
                    TempData["Success"] = "Profile picture deleted successfully.";
                }
                else
                {
                    _logger.LogWarning("No changes saved when deleting profile picture for user {UserId}", userId);
                    TempData["Error"] = "Failed to delete profile picture. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile picture");
                TempData["Error"] = "An error occurred while deleting your profile picture. Please try again.";
            }

            return RedirectToAction("EditProfile");
        }

        // Note: Favorites/Wishlist feature has been removed
    }
}