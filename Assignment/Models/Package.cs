using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class Package
    {
        [Key]
        public int PackageId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<PackageItem> PackageItems { get; set; } = new List<PackageItem>();
    }
}