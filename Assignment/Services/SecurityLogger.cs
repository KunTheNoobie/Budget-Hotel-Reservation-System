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
        /// </summary>
        /// <param name="action">The type of security action (e.g., "Login", "Logout", "FailedLogin").</param>
        /// <param name="userId">Optional ID of the user associated with this event. Null for system events.</param>
        /// <param name="ipAddress">Optional IP address from which the event originated.</param>
        /// <param name="details">Optional additional details or context about the event.</param>
        /// <remarks>
        /// This method fails silently if logging fails to prevent disrupting the user experience.
        /// In production, you might want to log failures to a separate error logging system.
        /// </remarks>
        public async Task LogAsync(string action, int? userId, string? ipAddress, string? details = null)
        {
            try
            {
                // Create a new security log entry
                var log = new SecurityLog
                {
                    Action = action,
                    UserId = userId,
                    IPAddress = ipAddress,
                    Details = details,
                    Timestamp = DateTime.Now
                };

                // Add to database and save
                _context.SecurityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Fail silently to not disrupt user flow
                // In production, consider logging this error to a separate error logging system
            }
        }
    }
}
