using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class RoomImage
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        public int RoomTypeId { get; set; }

        [Required, StringLength(255)]
        public string ImageUrl { get; set; }

        [StringLength(150)]
        public string? Caption { get; set; }

        [ForeignKey("RoomTypeId")]
        public virtual RoomType? RoomType { get; set; }
    }
}