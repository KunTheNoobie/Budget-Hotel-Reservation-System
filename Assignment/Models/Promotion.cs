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

        // Abuse Prevention Settings
        [Display(Name = "Limit Per Phone Number")]
        public bool LimitPerPhoneNumber { get; set; } = true;

        [Display(Name = "Limit Per Payment Card")]
        public bool LimitPerPaymentCard { get; set; } = true;

        [Display(Name = "Limit Per Device/IP")]
        public bool LimitPerDevice { get; set; } = false;

        [Display(Name = "Limit Per User Account")]
        public bool LimitPerUserAccount { get; set; } = false;

        [Display(Name = "Maximum Uses Per Limit")]
        [Range(1, 1000)]
        public int MaxUsesPerLimit { get; set; } = 1;

        [Display(Name = "Minimum Stay Nights")]
        [Range(0, 30)]
        public int? MinimumNights { get; set; }

        [Display(Name = "Minimum Amount (RM)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MinimumAmount { get; set; }

        [Display(Name = "Maximum Total Uses")]
        [Range(0, int.MaxValue)]
        public int? MaxTotalUses { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<PromotionUsage> PromotionUsages { get; set; } = new List<PromotionUsage>();
    }
}