using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public enum TicketStatus { Open, InProgress, Resolved, Closed }

    public class SupportTicket
    {
        [Key]
        public int TicketId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, StringLength(200)]
        public string Subject { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastModifiedAt { get; set; }

        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}