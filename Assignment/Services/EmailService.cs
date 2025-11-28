using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace Assignment.Services
{
    /// <summary>
    /// Service for sending emails using MailKit and SMTP.
    /// Handles email verification links and OTP codes for password reset.
    /// Supports configurable SMTP settings for different email providers.
    /// </summary>
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        /// <summary>
        /// SMTP server hostname from configuration.
        /// </summary>
        private string SmtpHost => _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";

        /// <summary>
        /// SMTP server port from configuration.
        /// </summary>
        private int SmtpPort => _configuration.GetValue<int>("EmailSettings:SmtpPort", 587);

        /// <summary>
        /// Email address used as sender (from address).
        /// </summary>
        private string FromEmail => _configuration["EmailSettings:FromEmail"] ?? "noreply@hotel.com";

        /// <summary>
        /// Display name for the sender email.
        /// </summary>
        private string FromName => _configuration["EmailSettings:FromName"] ?? "Budget Hotel Reservation System";

        /// <summary>
        /// SMTP username for authentication.
        /// </summary>
        private string SmtpUsername => _configuration["EmailSettings:SmtpUsername"] ?? string.Empty;

        /// <summary>
        /// SMTP password for authentication.
        /// </summary>
        private string SmtpPassword => _configuration["EmailSettings:SmtpPassword"] ?? string.Empty;

        /// <summary>
        /// Base URL of the application for generating email links.
        /// </summary>
        private string BaseUrl => _configuration["EmailSettings:BaseUrl"] ?? "https://localhost:5001";

        /// <summary>
        /// Initializes a new instance of the EmailService.
        /// </summary>
        /// <param name="configuration">Configuration for accessing email settings.</param>
        /// <param name="logger">Logger for recording email sending events.</param>
        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Sends an email verification link to the user's email address.
        /// </summary>
        /// <param name="toEmail">Recipient's email address.</param>
        /// <param name="toName">Recipient's name.</param>
        /// <param name="verificationToken">Unique verification token.</param>
        /// <param name="userId">User ID for the verification link.</param>
        /// <returns>True if email was sent successfully, false otherwise.</returns>
        public async Task<bool> SendVerificationEmailAsync(string toEmail, string toName, string verificationToken, int userId)
        {
            var verificationUrl = $"{BaseUrl}/Security/ConfirmEmail?userId={userId}&token={verificationToken}";

            var subject = "Verify Your Email Address - Budget Hotel Reservation";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to Budget Hotel Reservation!</h1>
        </div>
        <div class='content'>
            <p>Hello {toName},</p>
            <p>Thank you for registering with Budget Hotel Reservation System. To complete your registration and activate your account, please verify your email address by clicking the button below:</p>
            <div style='text-align: center;'>
                <a href='{verificationUrl}' class='button'>Verify Email Address</a>
            </div>
            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #007bff;'>{verificationUrl}</p>
            <p><strong>This link will expire in 24 hours.</strong></p>
            <p>If you did not create an account with us, please ignore this email.</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} Budget Hotel Reservation System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, toName, subject, body);
        }

        /// <summary>
        /// Sends a password reset OTP code to the user's email address.
        /// </summary>
        /// <param name="toEmail">Recipient's email address.</param>
        /// <param name="toName">Recipient's name.</param>
        /// <param name="otpCode">6-digit OTP code for password reset.</param>
        /// <returns>True if email was sent successfully, false otherwise.</returns>
        public async Task<bool> SendPasswordResetOtpAsync(string toEmail, string toName, string otpCode)
        {
            var subject = "Password Reset OTP - Budget Hotel Reservation";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-radius: 0 0 5px 5px; }}
        .otp-box {{ background-color: #fff; border: 2px solid #dc3545; border-radius: 5px; padding: 20px; text-align: center; margin: 20px 0; }}
        .otp-code {{ font-size: 32px; font-weight: bold; color: #dc3545; letter-spacing: 5px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Hello {toName},</p>
            <p>We received a request to reset your password for your Budget Hotel Reservation account.</p>
            <p>Please use the following One-Time Password (OTP) to reset your password:</p>
            <div class='otp-box'>
                <div class='otp-code'>{otpCode}</div>
            </div>
            <div class='warning'>
                <p><strong>⚠️ Security Notice:</strong></p>
                <ul style='margin: 10px 0; padding-left: 20px;'>
                    <li>This OTP is valid for <strong>10 minutes</strong> only.</li>
                    <li>Do not share this OTP with anyone.</li>
                    <li>If you did not request a password reset, please ignore this email and your password will remain unchanged.</li>
                </ul>
            </div>
            <p>Enter this OTP on the password reset page to proceed with resetting your password.</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} Budget Hotel Reservation System. All rights reserved.</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, toName, subject, body);
        }

        /// <summary>
        /// Sends an email using SMTP.
        /// </summary>
        /// <param name="toEmail">Recipient's email address.</param>
        /// <param name="toName">Recipient's name.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email body (HTML format).</param>
        /// <returns>True if email was sent successfully, false otherwise.</returns>
        private async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            try
            {
                // Create email message
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(FromName, FromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                // Create HTML body
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                // Send email using SMTP
                using (var client = new SmtpClient())
                {
                    // Connect to SMTP server
                    await client.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.StartTls);

                    // Authenticate if credentials are provided
                    if (!string.IsNullOrEmpty(SmtpUsername) && !string.IsNullOrEmpty(SmtpPassword))
                    {
                        await client.AuthenticateAsync(SmtpUsername, SmtpPassword);
                    }

                    // Send email
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    _logger.LogInformation($"Email sent successfully to {toEmail} with subject: {subject}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}. Error: {ex.Message}");
                return false;
            }
        }
    }
}

