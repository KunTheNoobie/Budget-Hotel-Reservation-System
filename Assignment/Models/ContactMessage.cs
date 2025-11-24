using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    public class ContactMessage
    {
        [Key]
        public int MessageId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Required, StringLength(200)]
        public string Subject { get; set; }

        [Required, StringLength(2000)]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
