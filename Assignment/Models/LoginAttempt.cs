using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    public class LoginAttempt
    {
        [Key]
        public int AttemptId { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool WasSuccessful { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }
    }
}