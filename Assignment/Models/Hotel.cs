using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    public class Hotel
    {
        [Key]
        public int HotelId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [Required, StringLength(255)]
        public string Address { get; set; }

        [Required, StringLength(50)]
        public string City { get; set; }

        [StringLength(10)]
        public string? PostalCode { get; set; }

        [Required, StringLength(50)]
        public string Country { get; set; }

        [Phone, StringLength(20)]
        public string? ContactNumber { get; set; }

        [EmailAddress, StringLength(100)]
        public string? ContactEmail { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}