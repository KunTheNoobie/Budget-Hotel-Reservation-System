using System.ComponentModel.DataAnnotations;

namespace Assignment.ViewModels.Search
{
    public class SearchViewModel
    {
        [StringLength(200, ErrorMessage = "Search term cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s,.-]+$", ErrorMessage = "Search term contains invalid characters")]
        [Display(Name = "Search Term")]
        public string? SearchTerm { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Check-in Date")]
        public DateTime? CheckIn { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Check-out Date")]
        public DateTime? CheckOut { get; set; }

        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
        [Display(Name = "Number of Guests")]
        public int? Guests { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Maximum price must be a positive number")]
        [Display(Name = "Maximum Price")]
        public decimal? MaxPrice { get; set; }

        [Display(Name = "Room Type")]
        public int? RoomTypeId { get; set; }
    }
}

