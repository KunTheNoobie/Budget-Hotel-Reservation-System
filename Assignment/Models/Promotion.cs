using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public enum DiscountType { Percentage, FixedAmount }

    public class Promotion
    {
        [Key]
        public int PromotionId { get; set; }

        [Required, StringLength(20)]
        public string Code { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        public DiscountType Type { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        public decimal Value { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}