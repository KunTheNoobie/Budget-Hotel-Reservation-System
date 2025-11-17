using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class PackageItem
    {
        public int PackageItemId { get; set; }
        public int PackageId { get; set; }
        public int? RoomTypeId { get; set; }
        public int? ServiceId { get; set; }
        public int Quantity { get; set; }

        [ForeignKey("PackageId")]
        public virtual Package Package { get; set; }
        [ForeignKey("RoomTypeId")]
        public virtual RoomType? RoomType { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }
    }
}