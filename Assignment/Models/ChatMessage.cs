using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class ChatMessage
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public string MessageText { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        [ForeignKey("TicketId")]
        public virtual SupportTicket SupportTicket { get; set; }
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }
    }
}