using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    public class SecurityLog
    {
        [Key]
        public int LogId { get; set; }

        public int? UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } // Login, Logout, FailedLogin, etc.

        [StringLength(50)]
        public string? IPAddress { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Details { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation property
        public virtual User? User { get; set; }
    }
}
