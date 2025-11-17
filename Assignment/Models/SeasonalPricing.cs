using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Assignment.Models
{
    public class SeasonalPricing
    {
        [Key]
        public int PricingId { get; set; }

        [Required]
        public int RoomTypeId { get; set; }

        [Required, StringLength(100)]
        public string RuleName { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [ForeignKey("RoomTypeId")]
        public virtual RoomType RoomType { get; set; }
    }
}