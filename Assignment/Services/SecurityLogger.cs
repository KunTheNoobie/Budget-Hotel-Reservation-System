using Assignment.Models;
using Assignment.Models.Data;

namespace Assignment.Services
{
    public class SecurityLogger
    {
        private readonly HotelDbContext _context;

        public SecurityLogger(HotelDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string action, int? userId, string? ipAddress, string? details = null)
        {
            try
            {
                var log = new SecurityLog
                {
                    Action = action,
                    UserId = userId,
                    IPAddress = ipAddress,
                    Details = details,
                    Timestamp = DateTime.Now
                };

                _context.SecurityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Fail silently to not disrupt user flow
            }
        }
    }
}
