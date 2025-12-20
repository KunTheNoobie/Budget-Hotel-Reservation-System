using Assignment.Models;
using Assignment.Models.Data;

namespace Assignment.Services
{
    /// <summary>
    /// Service for logging security events to the database.
    /// Records security-related actions such as login, logout, failed login attempts,
    /// password changes, and other security events for audit and monitoring purposes.
    /// </summary>
    public class SecurityLogger
    {
        /// <summary>
        /// Database context for accessing the SecurityLogs table.
        /// </summary>
        private readonly HotelDbContext _context;

        /// <summary>
        /// Initializes a new instance of the SecurityLogger with the provided database context.
        /// </summary>
        /// <param name="context">The database context for accessing SecurityLogs.</param>
        public SecurityLogger(HotelDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Logs a security event to the database asynchronously.
        /// Records security-related actions for audit trail and monitoring.
        /// </summary>
        /// <param name="action">The type of security action (e.g., "Login", "Logout", "FailedLogin", "Register", "PasswordReset").</param>
        /// <param name="userId">Optional ID of the user associated with this event. Null for system events or anonymous actions.</param>
        /// <param name="ipAddress">Optional IP address from which the event originated. Used for tracking and security monitoring.</param>
        /// <param name="details">Optional additional details or context about the event (e.g., error messages, additional information).</param>
        /// <remarks>
        /// This method fails silently if logging fails to prevent disrupting the user experience.
        /// In production, you might want to log failures to a separate error logging system.
        /// </remarks>
        public async Task LogAsync(string action, int? userId, string? ipAddress, string? details = null)
        {
            try
            {
                // ========== CREATE SECURITY LOG ENTRY ==========
                // Create a new SecurityLog entity to record the security event
                // This provides an audit trail for security monitoring and compliance
                var log = new SecurityLog
                {
                    Action = action,              // Type of security action (e.g., "Login", "Logout", "FailedLogin")
                    UserId = userId,             // User ID if event is associated with a user (null for system events)
                    IPAddress = ipAddress,       // IP address for tracking and security monitoring
                    Details = details,           // Additional context or information about the event
                    Timestamp = DateTime.Now     // When the event occurred
                };

                // ========== SAVE TO DATABASE ==========
                // Add log entry to database context and save
                // This stores the security event permanently for audit purposes
                _context.SecurityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // ========== ERROR HANDLING ==========
                // Fail silently to not disrupt user flow
                // If security logging fails, we don't want to break the application
                // In production, consider logging this error to a separate error logging system
                // This ensures security events are still tracked even if database logging fails
            }
        }
    }
}
