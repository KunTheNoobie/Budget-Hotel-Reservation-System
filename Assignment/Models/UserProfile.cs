using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class UserProfile
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(255)]
        public string? ProfilePictureUrl { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(10)]
        public string PreferredLanguage { get; set; } = "en-US";

        [StringLength(50)]
        public string Theme { get; set; } = "Default";

        public virtual User User { get; set; }
    }
}