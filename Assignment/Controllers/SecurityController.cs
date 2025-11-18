using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Services;
using Assignment.ViewModels.Security;
using Assignment.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Assignment.Controllers
{
    public class SecurityController : Controller
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<SecurityController> _logger;
        private const int MaxLoginAttempts = 3;
        private const int LockoutMinutes = 15;

        public SecurityController(HotelDbContext context, ILogger<SecurityController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check for too many failed login attempts
            var recentFailedAttempts = _context.LoginAttempts
                .Where(la => la.Email == model.Email && 
                            !la.WasSuccessful && 
                            la.Timestamp > DateTime.Now.AddMinutes(-LockoutMinutes))
                .Count();

            if (recentFailedAttempts >= MaxLoginAttempts)
            {
                ModelState.AddModelError("", $"Too many failed login attempts. Please try again after {LockoutMinutes} minutes.");
                return View(model);
            }

            // Find user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !PasswordService.VerifyPassword(model.Password, user.PasswordHash))
            {
                // Record failed attempt
                _context.LoginAttempts.Add(new LoginAttempt
                {
                    Email = model.Email,
                    WasSuccessful = false,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Check if user is active
            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your account has been deactivated. Please contact support.");
                return View(model);
            }

            // Record successful attempt
            _context.LoginAttempts.Add(new LoginAttempt
            {
                Email = model.Email,
                WasSuccessful = true,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();

            // Sign in user
            await AuthenticationHelper.SignInAsync(HttpContext, user);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // Redirect based on role
            return user.Role switch
            {
                UserRole.Admin => RedirectToAction("Index", "Admin"),
                UserRole.Manager => RedirectToAction("Index", "Admin"),
                UserRole.Staff => RedirectToAction("Index", "Admin"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await AuthenticationHelper.SignOutAsync(HttpContext);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            // Create new user
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = PasswordService.HashPassword(model.Password),
                PhoneNumber = model.PhoneNumber,
                Role = UserRole.Customer,
                IsEmailVerified = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate email verification token
            var token = GenerateSecureToken();
            var securityToken = new SecurityToken
            {
                TokenValue = token,
                UserId = user.UserId,
                Type = TokenType.EmailVerification,
                ExpiryDate = DateTime.Now.AddDays(1),
                IsUsed = false
            };

            _context.SecurityTokens.Add(securityToken);
            await _context.SaveChangesAsync();

            // In a real application, send email here
            // For now, we'll redirect to a page that shows the verification link
            TempData["VerificationToken"] = token;
            TempData["UserId"] = user.UserId;

            return RedirectToAction("VerifyEmail", new { userId = user.UserId });
        }

        [HttpGet]
        public IActionResult VerifyEmail(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.IsEmailVerified)
            {
                return RedirectToAction("Login");
            }

            ViewBag.UserId = userId;
            ViewBag.Email = user.Email;
            ViewBag.Token = TempData["VerificationToken"]?.ToString();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(int userId, string token)
        {
            var securityToken = await _context.SecurityTokens
                .Include(st => st.User)
                .FirstOrDefaultAsync(st => st.UserId == userId && 
                                          st.TokenValue == token && 
                                          st.Type == TokenType.EmailVerification &&
                                          !st.IsUsed &&
                                          st.ExpiryDate > DateTime.Now);

            if (securityToken == null)
            {
                ViewBag.Error = "Invalid or expired verification token.";
                return View("VerifyEmail");
            }

            securityToken.User.IsEmailVerified = true;
            securityToken.IsUsed = true;
            await _context.SaveChangesAsync();

            ViewBag.Success = "Email verified successfully! You can now login.";
            return View("VerifyEmail");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                // Don't reveal that the email doesn't exist
                ViewBag.Message = "If the email exists, a password reset link has been sent.";
                return View(model);
            }

            // Generate password reset token
            var token = GenerateSecureToken();
            var securityToken = new SecurityToken
            {
                TokenValue = token,
                UserId = user.UserId,
                Type = TokenType.PasswordReset,
                ExpiryDate = DateTime.Now.AddHours(1),
                IsUsed = false
            };

            _context.SecurityTokens.Add(securityToken);
            await _context.SaveChangesAsync();

            // In a real application, send email here
            // For now, we'll redirect to a page that shows the reset link
            TempData["ResetToken"] = token;
            TempData["ResetEmail"] = user.Email;

            return RedirectToAction("ResetPassword", new { token = token, email = user.Email });
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordViewModel
            {
                Token = token ?? string.Empty,
                Email = email ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email address.");
                return View(model);
            }

            var securityToken = await _context.SecurityTokens
                .Include(st => st.User)
                .FirstOrDefaultAsync(st => st.UserId == user.UserId &&
                                          st.TokenValue == model.Token &&
                                          st.Type == TokenType.PasswordReset &&
                                          !st.IsUsed &&
                                          st.ExpiryDate > DateTime.Now);

            if (securityToken == null)
            {
                ModelState.AddModelError("", "Invalid or expired reset token.");
                return View(model);
            }

            // Update password
            securityToken.User.PasswordHash = PasswordService.HashPassword(model.Password);
            securityToken.IsUsed = true;
            await _context.SaveChangesAsync();

            ViewBag.Success = "Password reset successfully! You can now login with your new password.";
            return View("Login", new LoginViewModel());
        }

        private string GenerateSecureToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
            }
        }
    }
}

