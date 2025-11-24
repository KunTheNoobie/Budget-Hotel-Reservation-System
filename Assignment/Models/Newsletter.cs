using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    public class Newsletter
    {
        [Key]
        public int NewsletterId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        public DateTime SubscribedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}

