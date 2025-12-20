using Assignment.Helpers;
using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Services;
using Assignment.ViewModels.Security;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Security.Cryptography;
using System.Text;
using static QRCoder.PayloadGenerator;
using static System.Net.WebRequestMethods;

namespace Assignment.Controllers
{
    /// <summary>
    /// Controller for handling user authentication and security-related operations.
    /// Manages user registration, login, logout, password reset, email verification,
    /// and implements security features like login attempt tracking and account lockout.
    /// </summary>
    public class SecurityController : Controller
    {
        /// <summary>
        /// Database context for accessing user and security data.
        /// </summary>
        private readonly HotelDbContext _context;

        /// <summary>
        /// Logger for recording security events and errors.
        /// </summary>
        private readonly ILogger<SecurityController> _logger;

        /// <summary>
        /// Service for logging security events to the database.
        /// </summary>
        private readonly SecurityLogger _securityLogger;

        /// <summary>
        /// Service for sending emails (verification links and OTP codes).
        /// </summary>
        private readonly Services.EmailService _emailService;

        /// <summary>
        /// Configuration for accessing application settings.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Maximum number of failed login attempts before account lockout.
        /// Defaults to 3 if not configured.
        /// </summary>
        private int MaxLoginAttempts => _configuration.GetValue<int>("SecuritySettings:MaxLoginAttempts", 3);

        /// <summary>
        /// Number of minutes an account remains locked after exceeding max login attempts.
        /// Defaults to 15 minutes if not configured.
        /// </summary>
        private int LockoutMinutes => _configuration.GetValue<int>("SecuritySettings:LockoutMinutes", 15);

        /// <summary>
        /// Initializes a new instance of the SecurityController.
        /// </summary>
        /// <param name="context">Database context for data access.</param>
        /// <param name="logger">Logger instance for logging.</param>
        /// <param name="configuration">Configuration for accessing settings.</param>
        /// <param name="emailService">Service for sending emails.</param>
        public SecurityController(HotelDbContext context, ILogger<SecurityController> logger, IConfiguration configuration, Services.EmailService emailService)
        {
            _context = context;
            _logger = logger;
            _securityLogger = new SecurityLogger(context);
            _configuration = configuration;
            _emailService = emailService;
        }

        /// <summary>
        /// Displays the login page.
        /// Redirects to home if user is already authenticated.
        /// </summary>
        /// <param name="returnUrl">Optional URL to redirect to after successful login.</param>
        /// <returns>The login page view.</returns>
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

            // ========== ACCOUNT LOCKOUT PROTECTION ==========
            // Check for too many failed login attempts from this email address
            // This prevents brute-force attacks where attackers try many passwords
            var recentFailedAttempts = _context.LoginAttempts
                .Where(la => la.Email == model.Email &&           // Same email address
                            !la.WasSuccessful &&                  // Only count failed attempts
                            la.Timestamp > DateTime.Now.AddMinutes(-LockoutMinutes)) // Within lockout time window
                .Count();

            // If user has exceeded maximum allowed failed attempts, block login
            // This is a security feature to prevent password guessing attacks
            if (recentFailedAttempts >= MaxLoginAttempts)
            {
                // Log the lockout event for security monitoring
                await _securityLogger.LogAsync("LoginLockedOut", null, HttpContext.Connection.RemoteIpAddress?.ToString(), $"Email: {model.Email}");
                
                // Show error message to user explaining they're locked out
                ModelState.AddModelError("", $"Too many failed login attempts. Please try again after {LockoutMinutes} minutes.");
                return View(model);
            }

            // ========== USER AUTHENTICATION ==========
            // Check credentials for admin, manager, and staff accounts first
            // These accounts use hard-coded passwords for easy testing/demo purposes
            // In production, all accounts should use hashed passwords
            bool isHardCodedLogin = false;
            User? user = null;
            
            // ========== CHECK ADMIN LOGIN ==========
            // Admin account uses hard-coded credentials for demonstration
            // Email: admin@hotel.com, Password: Admin123!
            if (model.Email == "admin@hotel.com" && model.Password == "Admin123!")
            {
                // Find admin user in database
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@hotel.com" && u.Role == UserRole.Admin);
                if (user != null)
                {
                    // Mark as hard-coded login (skip password hash verification)
                    isHardCodedLogin = true;
                }
            }
            // ========== CHECK MANAGER LOGIN ==========
            // Manager accounts use pattern: manager1@hotel.com, manager2@hotel.com, etc.
            // All managers use same password: Manager123!
            else if (model.Email.StartsWith("manager") && model.Email.EndsWith("@hotel.com") && model.Password == "Manager123!")
            {
                // Find manager user by email and role
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Role == UserRole.Manager);
                if (user != null)
                {
                    // Mark as hard-coded login (skip password hash verification)
                    isHardCodedLogin = true;
                }
            }
            // ========== CHECK STAFF LOGIN ==========
            // Staff accounts use pattern: staff1@hotel.com, staff2@hotel.com, etc.
            // All staff use same password: Password123!
            else if (model.Email.StartsWith("staff") && model.Email.EndsWith("@hotel.com") && model.Password == "Password123!")
            {
                // Find staff user by email and role
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Role == UserRole.Staff);
                if (user != null)
                {
                    // Mark as hard-coded login (skip password hash verification)
                    isHardCodedLogin = true;
                }
            }
            // ========== REGULAR USER LOOKUP ==========
            // For all other users (customers), look up by email only
            // Password will be verified using BCrypt hash comparison
            else
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            }

            // ========== PASSWORD VERIFICATION ==========
            // Check if user exists and password is correct
            // For hard-coded logins (admin/manager/staff), skip password hash verification
            // For regular users, verify password using BCrypt hash comparison
            if (user == null || (!isHardCodedLogin && !PasswordService.VerifyPassword(model.Password, user.PasswordHash)))
            {
                // ========== RECORD FAILED LOGIN ATTEMPT ==========
                // User not found OR password doesn't match
                // Record this failed attempt in database for security monitoring
                _context.LoginAttempts.Add(new LoginAttempt
                {
                    Email = model.Email,
                    WasSuccessful = false,  // Mark as failed attempt
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), // Store IP for security tracking
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                // Log security event for audit trail
                await _securityLogger.LogAsync("LoginFailed", null, HttpContext.Connection.RemoteIpAddress?.ToString(), $"Email: {model.Email}");

                // Show generic error message (don't reveal if email exists or not - security best practice)
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // ========== CHECK ACCOUNT STATUS ==========
            // Verify that the user account is active (not deactivated by admin)
            // Deactivated accounts cannot log in even with correct password
            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your account has been deactivated. Please contact support.");
                return View(model);
            }

            // ========== RECORD SUCCESSFUL LOGIN ==========
            // Password is correct and account is active
            // Record successful login attempt in database
            _context.LoginAttempts.Add(new LoginAttempt
            {
                Email = model.Email,
                WasSuccessful = true,  // Mark as successful attempt
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), // Store IP for security tracking
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();

            // ========== CREATE AUTHENTICATION SESSION ==========
            // Sign in the user by creating an authentication cookie
            // This cookie contains user claims (ID, email, role, name) and persists for 7 days
            await AuthenticationHelper.SignInAsync(HttpContext, user);
            
            // Log successful login for security audit trail
            await _securityLogger.LogAsync("LoginSuccess", user.UserId, HttpContext.Connection.RemoteIpAddress?.ToString());

            // ========== POST-LOGIN REDIRECTION ==========
            // Check if user was trying to access a specific page before login
            // If so, redirect them back to that page (e.g., after clicking "Book Now" while not logged in)
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                // Security check: Only redirect to local URLs (prevents open redirect attacks)
                return Redirect(model.ReturnUrl);
            }

            // ========== ROLE-BASED REDIRECTION ==========
            // Redirect user to appropriate page based on their role:
            // - Admin, Manager, Staff → Admin dashboard (can manage system)
            // - Customer → Home page (can browse and book rooms)
            return user.Role switch
            {
                UserRole.Admin => RedirectToAction("Index", "Admin"),      // Admin sees all hotels
                UserRole.Manager => RedirectToAction("Index", "Admin"),   // Manager sees their hotel only
                UserRole.Staff => RedirectToAction("Index", "Admin"),      // Staff sees their hotel only
                _ => RedirectToAction("Index", "Home")                     // Customer sees public home page
            };
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            await AuthenticationHelper.SignOutAsync(HttpContext);
            await _securityLogger.LogAsync("Logout", userId, HttpContext.Connection.RemoteIpAddress?.ToString());
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Index", "Home");
            }

            // Generate simple math captcha
            var random = new Random();
            int num1 = random.Next(1, 10);
            int num2 = random.Next(1, 10);
            TempData["CaptchaSum"] = num1 + num2;
            ViewBag.Num1 = num1;
            ViewBag.Num2 = num2;

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // ========== RATE LIMITING PROTECTION ==========
            // Prevent spam registrations by limiting attempts from same IP address
            // This protects against automated bots creating fake accounts
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            // Count registration attempts from this IP in the last 5 minutes
            var recentAttempts = _context.SecurityLogs
                .Count(s => s.IPAddress == ipAddress &&           // Same IP address
                            s.Action == "Register" &&             // Registration action
                            s.Timestamp > DateTime.Now.AddMinutes(-5)); // Within last 5 minutes
            
            // If too many attempts (3 or more), block registration
            // This prevents automated registration attacks
            if (recentAttempts >= 3)
            {
                // Log the rate limit violation for security monitoring
                await _securityLogger.LogAsync("Register", null, ipAddress, "Rate limit exceeded");
                
                // Show error message to user
                ModelState.AddModelError("", "Too many registration attempts. Please try again after 5 minutes.");
                
                // Regenerate captcha for when user tries again
                var random = new Random();
                int num1 = random.Next(1, 10);
                int num2 = random.Next(1, 10);
                TempData["CaptchaSum"] = num1 + num2;
                ViewBag.Num1 = num1;
                ViewBag.Num2 = num2;
                return View(model);
            }

            // ========== CAPTCHA VALIDATION ==========
            // Verify that user correctly answered the math captcha question
            // This prevents automated bots from registering accounts
            // The captcha answer was stored in TempData when the registration page was first loaded
            if (TempData["CaptchaSum"] is int expectedSum)
            {
                // Compare user's answer with the correct answer stored in TempData
                if (model.CaptchaAnswer != expectedSum)
                {
                    // Answer is incorrect - add error to prevent registration
                    ModelState.AddModelError("CaptchaAnswer", "Incorrect answer to the security question.");
                }
            }
            else
            {
                // ========== SESSION EXPIRATION HANDLING ==========
                // If TempData is lost (e.g., session timeout, page refresh), captcha answer is missing
                // This means the user needs to reload the page to get a new captcha
                ModelState.AddModelError("CaptchaAnswer", "Session expired. Please try again.");
            }

            if (!ModelState.IsValid)
            {
                // Regenerate captcha for retry
                var random = new Random();
                int num1 = random.Next(1, 10);
                int num2 = random.Next(1, 10);
                TempData["CaptchaSum"] = num1 + num2;
                ViewBag.Num1 = num1;
                ViewBag.Num2 = num2;

                return View(model);
            }

            // ========== EMAIL UNIQUENESS CHECK ==========
            // Verify that the email address is not already registered
            // Each user must have a unique email address (used as username)
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                
                // Regenerate captcha for retry (security: new captcha for each attempt)
                var random = new Random();
                int num1 = random.Next(1, 10);
                int num2 = random.Next(1, 10);
                TempData["CaptchaSum"] = num1 + num2;
                ViewBag.Num1 = num1;
                ViewBag.Num2 = num2;
                return View(model);
            }

            // ========== PHONE NUMBER VALIDATION ==========
            // Validate phone number format if provided (phone number is optional)
            // Accepts various formats: +60-12-345-6789, 012-345-6789, or 0123456789
            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                // Regular expression pattern to validate phone number format
                // Allows: international format (+country code), Malaysian format with dashes, or plain digits
                var phonePattern = @"^(\+?[0-9]{1,4}[-]?[0-9]{2,4}[-]?[0-9]{3,4}[-]?[0-9]{3,4}|[0-9]{7,15})$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.PhoneNumber, phonePattern))
                {
                    ModelState.AddModelError("PhoneNumber", "Please enter a valid phone number (e.g., +60-12-345-6789, 012-345-6789, or 0123456789).");
                    
                    // Regenerate captcha for retry
                    var random = new Random();
                    int num1 = random.Next(1, 10);
                    int num2 = random.Next(1, 10);
                    TempData["CaptchaSum"] = num1 + num2;
                    ViewBag.Num1 = num1;
                    ViewBag.Num2 = num2;
                    return View(model);
                }
            }

            // ========== CREATE NEW USER ACCOUNT ==========
            // All validations passed, create the new user account in database
            var user = new User
            {
                FullName = model.FullName,  // User's full name
                Email = model.Email,         // Email address (used as username, must be unique)
                
                // Hash password using BCrypt before storing (NEVER store plain text passwords!)
                // BCrypt automatically handles salting and is resistant to brute-force attacks
                PasswordHash = PasswordService.HashPassword(model.Password),
                
                // Encrypt phone number before storing (privacy protection)
                // Phone numbers are sensitive data and should be encrypted
                PhoneNumber = EncryptionService.Encrypt(model.PhoneNumber ?? ""),
                
                // All new registrations are Customer role by default
                // Admin/Manager/Staff accounts are created by administrators
                Role = UserRole.Customer,
                
                // Email is not verified yet (user must verify via email)
                IsEmailVerified = false,
                
                // New accounts are active by default (can log in after email verification)
                IsActive = true,
                
                // Record when account was created
                CreatedAt = DateTime.Now
            };

            // Save user to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // ========== LOG REGISTRATION EVENT ==========
            // Record successful registration for security audit trail
            await _securityLogger.LogAsync("Register", user.UserId, ipAddress, $"New user registered: {user.Email}");

            // ========== GENERATE EMAIL VERIFICATION CODE ==========
            // Generate a 6-digit OTP (One-Time Password) code for email verification
            // This code will be sent to user's email and must be entered to verify email
            var variant = new Random();
            var otpCode = variant.Next(100000, 999999).ToString(); // Generate random 6-digit code
            
            // Create security token to store the OTP code
            // Token expires after 1 day and can only be used once
            var securityToken = new SecurityToken
            {
                TokenValue = otpCode,                    // The 6-digit code
                UserId = user.UserId,                    // Link to the user
                Type = TokenType.EmailVerification,      // Token type: email verification
                ExpiryDate = DateTime.Now.AddDays(1),    // Expires in 24 hours
                IsUsed = false                           // Not used yet
            };

            // Save token to database
            _context.SecurityTokens.Add(securityToken);
            await _context.SaveChangesAsync();

            // ========== SEND VERIFICATION EMAIL ==========
            // Send the OTP code to user's email address
            // User must enter this code to verify their email and activate account
            try
            {
                // Send email with OTP code using MailKit SMTP service
                SendEmailDirectly1(user.Email, otpCode);
                TempData["SuccessMessage"] = $"Verification code sent to {user.Email}.";
            }
            catch (Exception ex)
            {
                // If email sending fails, log error
                // In development/testing, show code in warning message (for testing purposes only)
                _logger.LogError($"Email failed: {ex.Message}");
                
                // Fallback for testing ONLY - shows code in browser
                // In production, this should redirect to error page instead
                TempData["WarningMessage"] = $"Email failed. Code: {otpCode}";
            }

            // Redirect to the Input Code Page
            return RedirectToAction("VerifyEmail", new { userId = user.UserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(int userId, string otpCode)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Find the token
            var securityToken = await _context.SecurityTokens
                .FirstOrDefaultAsync(st => st.UserId == userId &&
                                          st.TokenValue == otpCode &&
                                          st.Type == TokenType.EmailVerification &&
                                          !st.IsUsed &&
                                          st.ExpiryDate > DateTime.Now);

            if (securityToken == null)
            {
                ViewBag.Error = "Invalid or expired verification code.";
                ViewBag.UserId = userId;
                ViewBag.Email = user.Email;
                return View();
            }

            // Success!
            securityToken.IsUsed = true;
            user.IsEmailVerified = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Email verified successfully! You can now login.";
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult VerifyEmail(int userId)
        {
            var user = _context.Users.Find(userId);

            if (user == null) return NotFound();

            if (user.IsEmailVerified) return RedirectToAction("Login");

            ViewBag.UserId = userId;
            ViewBag.Email = user.Email;

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
                // Try to get user info for display
                var user = await _context.Users.FindAsync(userId);
                ViewBag.Error = "Invalid or expired verification token.";
                ViewBag.UserId = userId;
                ViewBag.Email = user?.Email ?? "";
                return View("VerifyEmail");
            }

            securityToken.User.IsEmailVerified = true;
            securityToken.IsUsed = true;
            await _context.SaveChangesAsync();

            ViewBag.Success = "Email verified successfully! You can now login.";
            ViewBag.UserId = userId;
            ViewBag.Email = securityToken.User.Email;
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
                ViewBag.Message = "If the email exists, a password reset OTP has been sent to your email.";
                return View(model);
            }

            // Generate 6-digit OTP code
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();

            // Store OTP as token (expires in 10 minutes)
            var securityToken = new SecurityToken
            {
                TokenValue = otpCode,
                UserId = user.UserId,
                Type = TokenType.PasswordReset,
                ExpiryDate = DateTime.Now.AddMinutes(5),
                IsUsed = false
            };

            _context.SecurityTokens.Add(securityToken);
            await _context.SaveChangesAsync();

            // Send OTP via email

            try
            {
                SendEmailDirectly(user.Email, otpCode);
                TempData["SuccessMessage"] = $"A password reset OTP has been sent to {user.Email}. Please check your inbox.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email: {ex.Message}");
                // Fallback for testing/debugging if email fails
                TempData["WarningMessage"] = "Email sending failed. Please check your internet connection. (Dev OTP: " + otpCode + ")";
            }

            return RedirectToAction("VerifyOtp", new { email = user.Email });
        }

        private void SendEmailDirectly(string toEmail, string otpCode)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("BudgetStay", "yourgmail@gmail.com"));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "OTP Code for Reset Password";

            string imageUrl = "https://github.com/Finnwindhoek/Testing123/blob/main/Icon.png?raw=true";
            // Properly formatted email body
            message.Body = new TextPart("html")
            {
                Text = $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #D4D4D4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 50px auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
            text-align: center;
        }}
        h2 {{
            color: #2E86C1;
            font-size: 28px;
            margin: 20px 0;
        }}
        p {{
            font-size: 16px;
            color: #333333;
            line-height: 1.5;
        }}
        .otp-code {{
            font-size: 32px;
            font-weight: bold;
            color: #2E86C1;
            margin: 20px 0;
        }}
        .footer {{
            font-size: 12px;
            color: #888888;
            margin-top: 30px;
        }}

        .text{{
            font-size: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <img src='{imageUrl}' alt=""Hotel Logo"" width=""400"" height=""200"">
        <strong><p class='text'>Verify your email address to reset password!</p></strong>
        <p class='text'>Hey there, Welcome to BudgetStay!</strong>.</p>
        <p class='text'>To reset your password, please use the following One-Time Password (OTP):</p>
        <div class='otp-code'>{otpCode}</div>
        <p class='text'>This code is valid for the next 2 minutes. Please do not share this code with anyone.</p>
        <p class='text'>If you did not request this OTP, please ignore this email.</p>
        <div class='footer'>
            &copy; 2025 BudgetStay. All rights reserved.<br>
            Your home, City, Malaysia
        </div>
    </div>
</body>
</html>"
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("kartikramasamymsi@gmail.com", "ymgf blvf zyzj atka");
                client.Send(message);
                client.Disconnect(true);
            }
        }




        private void SendEmailDirectly1(string toEmail, string otpCode)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("BudgetStay", "yourgmail@gmail.com"));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "OTP Code for Email Verification";

            string imageUrl = "https://github.com/Finnwindhoek/Testing123/blob/main/Icon.png?raw=true";
            // Properly formatted email body
            message.Body = new TextPart("html")
            {
                Text = $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #D4D4D4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 50px auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
            text-align: center;
        }}
        h2 {{
            color: #2E86C1;
            font-size: 28px;
            margin: 20px 0;
        }}
        p {{
            font-size: 16px;
            color: #333333;
            line-height: 1.5;
        }}
        .otp-code {{
            font-size: 32px;
            font-weight: bold;
            color: #2E86C1;
            margin: 20px 0;
        }}
        .footer {{
            font-size: 12px;
            color: #888888;
            margin-top: 30px;
        }}

        .text{{
            font-size: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <img src='{imageUrl}' alt=""Hotel Logo"" width=""400"" height=""200"">
        <strong><p class='text'>Verify your email address!</p></strong>
        <p class='text'>Hey there, Welcome to BudgetStay!</strong>.</p>
        <p class='text'>To verify your email, please use the following One-Time Password (OTP):</p>
        <div class='otp-code'>{otpCode}</div>
        <p class='text'>This code is valid for the next 2 minutes. Please do not share this code with anyone.</p>
        <p class='text'>Here's additional voucher for every new members!</p>
        <h3 class='text'>WELCOME10</h3>
        <div class='footer'>
            &copy; 2025 BudgetStay. All rights reserved.<br>
            Your home, City, Malaysia
        </div>
    </div>
</body>
</html>"
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("kartikramasamymsi@gmail.com", "ymgf blvf zyzj atka");
                client.Send(message);
                client.Disconnect(true);
            }
        }





        /// <summary>
        /// Displays the OTP verification page for password reset.
        /// </summary>
        /// <param name="email">User's email address.</param>
        /// <returns>The OTP verification view.</returns>
        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            var model = new VerifyOtpViewModel
            {
                Email = email ?? string.Empty
            };

            return View(model);
        }

        /// <summary>
        /// Verifies the OTP code entered by the user.
        /// If valid, redirects to password reset page.
        /// </summary>
        /// <param name="model">OTP verification view model containing email and OTP code.</param>
        /// <returns>Redirects to ResetPassword if OTP is valid, otherwise shows error.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
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

            // Verify OTP token
            var securityToken = await _context.SecurityTokens
                .FirstOrDefaultAsync(st => st.UserId == user.UserId &&
                                          st.TokenValue == model.OtpCode &&
                                          st.Type == TokenType.PasswordReset &&
                                          !st.IsUsed &&
                                          st.ExpiryDate > DateTime.Now);

            if (securityToken == null)
            {
                ModelState.AddModelError("OtpCode", "Invalid or expired OTP code. Please request a new one.");
                return View(model);
            }

            // Mark OTP as used
            securityToken.IsUsed = true;
            await _context.SaveChangesAsync();

            // Generate a new secure token for password reset (different from OTP)
            var resetToken = GenerateSecureToken();
            var resetSecurityToken = new SecurityToken
            {
                TokenValue = resetToken,
                UserId = user.UserId,
                Type = TokenType.PasswordReset,
                ExpiryDate = DateTime.Now.AddHours(1),
                IsUsed = false
            };

            _context.SecurityTokens.Add(resetSecurityToken);
            await _context.SaveChangesAsync();

            // Redirect to password reset page with the secure token
            return RedirectToAction("ResetPassword", new { token = resetToken, email = user.Email });
        }

        /// <summary>
        /// Displays the password reset form.
        /// </summary>
        /// <param name="token">Password reset token (generated after OTP verification).</param>
        /// <param name="email">User's email address.</param>
        /// <returns>The password reset view.</returns>
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

        /// <summary>
        /// Generates a cryptographically secure 6-digit OTP code for password reset.
        /// Uses RandomNumberGenerator to ensure secure, unpredictable OTPs in multi-threaded environments.
        /// </summary>
        /// <returns>A 6-digit OTP code as a string.</returns>
        private string GenerateSecureOtp()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                // Convert to positive integer and ensure it's in the range 100000-999999
                var value = BitConverter.ToUInt32(bytes, 0);
                var otp = (int)(100000 + (value % 900000));
                return otp.ToString();
            }
        }

        /// <summary>
        /// Generates a cryptographically secure token for email verification and password reset links.
        /// </summary>
        /// <returns>A base64-encoded secure token string.</returns>
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