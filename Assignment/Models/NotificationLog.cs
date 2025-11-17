using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public enum NotificationType { Email, SMS }
    public enum NotificationStatus { Sent, Failed }

    public class NotificationLog
    {
        [Key]
        public int LogId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public NotificationType Type { get; set; }
        [Required]
        public string Recipient { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Body { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public NotificationStatus Status { get; set; }
        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}