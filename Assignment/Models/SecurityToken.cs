using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public enum TokenType { PasswordReset, EmailVerification }

    public class SecurityToken
    {
        [Key]
        public int TokenId { get; set; }

        [Required]
        [StringLength(256)]
        public string TokenValue { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public TokenType Type { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; } = false;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}