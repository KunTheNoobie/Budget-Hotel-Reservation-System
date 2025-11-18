using Assignment.Models;
using Assignment.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class RoomController : Controller
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<RoomController> _logger;

        public RoomController(HotelDbContext context, ILogger<RoomController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Catalog(string searchTerm = "", int? roomTypeId = null, decimal? maxPrice = null, DateTime? checkIn = null, int? guests = null, int page = 1, int pageSize = 9)
        {
            var query = _context.RoomTypes
                .Include(rt => rt.Hotel)
                .Include(rt => rt.RoomImages)
                .Include(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                var locationParts = term.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (locationParts.Length >= 2)
                {
                    var cityTerm = locationParts[0];
                    var countryTerm = locationParts[1];

                    query = query.Where(rt => rt.Hotel != null &&
                        (
                            rt.Hotel.City.ToLower().Contains(cityTerm) ||
                            rt.Hotel.Name.ToLower().Contains(cityTerm)
                        ) &&
                        (
                            rt.Hotel.Country.ToLower().Contains(countryTerm) ||
                            rt.Hotel.Name.ToLower().Contains(countryTerm)
                        ));
                }
                else
                {
                    query = query.Where(rt =>
                        rt.Name.ToLower().Contains(term) ||
                        (rt.Hotel != null && (
                            rt.Hotel.Name.ToLower().Contains(term) ||
                            rt.Hotel.City.ToLower().Contains(term) ||
                            rt.Hotel.Country.ToLower().Contains(term))));
                }
            }

            if (roomTypeId.HasValue)
            {
                query = query.Where(rt => rt.RoomTypeId == roomTypeId.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(rt => rt.BasePrice <= maxPrice.Value);
            }

            if (guests.HasValue && guests.Value > 0)
            {
                query = query.Where(rt => rt.Occupancy >= guests.Value);
            }

            var totalCount = await query.CountAsync();
            var roomTypes = await query
                .OrderBy(rt => rt.RoomTypeId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoomTypeId = roomTypeId;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Guests = guests;
            ViewBag.CheckIn = checkIn?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.AllRoomTypes = await _context.RoomTypes.OrderBy(rt => rt.RoomTypeId).ToListAsync();
            ViewBag.MaxPriceInDb = await _context.RoomTypes.MaxAsync(rt => (decimal?)rt.BasePrice) ?? 0;
            ViewBag.Destinations = await _context.Hotels
                .Select(h => h.City + ", " + h.Country)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            return View(roomTypes);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var roomType = await _context.RoomTypes
                .Include(rt => rt.Hotel)
                .Include(rt => rt.RoomImages)
                .Include(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
                .Include(rt => rt.Rooms)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == id);

            if (roomType == null)
            {
                return NotFound();
            }

            // Get available rooms count (excluding booked rooms)
            var availableRooms = await _context.Rooms
                .Where(r => r.RoomTypeId == id && r.Status == RoomStatus.Available)
                .Where(r => !_context.Bookings.Any(b => b.RoomId == r.RoomId &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.CheckedOut))
                .CountAsync();
            ViewBag.AvailableRooms = availableRooms;

            return View(roomType);
        }

        [HttpGet]
        public async Task<IActionResult> SearchAjax(string term = "", int? roomTypeId = null, decimal? maxPrice = null, int? guests = null)
        {
            var query = _context.RoomTypes
                .Include(rt => rt.Hotel)
                .Include(rt => rt.RoomImages)
                .Include(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
                .AsQueryable();

            if (!string.IsNullOrEmpty(term))
            {
                var lowerTerm = term.Trim().ToLower();
                var locationParts = lowerTerm.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (locationParts.Length >= 2)
                {
                    var cityTerm = locationParts[0];
                    var countryTerm = locationParts[1];

                    query = query.Where(rt => rt.Hotel != null &&
                        (
                            rt.Hotel.City.ToLower().Contains(cityTerm) ||
                            rt.Hotel.Name.ToLower().Contains(cityTerm)
                        ) &&
                        (
                            rt.Hotel.Country.ToLower().Contains(countryTerm) ||
                            rt.Hotel.Name.ToLower().Contains(countryTerm)
                        ));
                }
                else
                {
                    query = query.Where(rt =>
                        rt.Name.ToLower().Contains(lowerTerm) ||
                        (rt.Hotel != null && (
                            rt.Hotel.Name.ToLower().Contains(lowerTerm) ||
                            rt.Hotel.City.ToLower().Contains(lowerTerm) ||
                            rt.Hotel.Country.ToLower().Contains(lowerTerm))));
                }
            }

            if (roomTypeId.HasValue)
            {
                query = query.Where(rt => rt.RoomTypeId == roomTypeId.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(rt => rt.BasePrice <= maxPrice.Value);
            }

            if (guests.HasValue && guests.Value > 0)
            {
                query = query.Where(rt => rt.Occupancy >= guests.Value);
            }

            var roomTypes = await query
                .OrderBy(rt => rt.BasePrice)
                .Take(9)
                .Select(rt => new
                {
                    rt.RoomTypeId,
                    rt.Name,
                    rt.Description,
                    rt.BasePrice,
                    rt.Occupancy,
                    Location = rt.Hotel != null ? rt.Hotel.City + ", " + rt.Hotel.Country : "Malaysia",
                    ImageUrl = rt.RoomImages.FirstOrDefault() != null ? rt.RoomImages.FirstOrDefault().ImageUrl : null,
                    Amenities = rt.RoomTypeAmenities.Select(rta => rta.Amenity.Name).ToList()
                })
                .ToListAsync();

            return Json(roomTypes);
        }

        [HttpPost]
        public async Task<IActionResult> CheckAvailability(int roomTypeId, DateTime checkIn, DateTime checkOut)
        {
            if (checkIn >= checkOut)
            {
                return Json(new { available = false, message = "Check-out date must be after check-in date." });
            }

            if (checkIn < DateTime.Today)
            {
                return Json(new { available = false, message = "Check-in date cannot be in the past." });
            }

            var availableRooms = await _context.Rooms
                .Where(r => r.RoomTypeId == roomTypeId && r.Status == RoomStatus.Available)
                .Where(r => !_context.Bookings.Any(b => b.RoomId == r.RoomId &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.CheckedOut &&
                    ((b.CheckInDate <= checkIn && b.CheckOutDate > checkIn) ||
                     (b.CheckInDate < checkOut && b.CheckOutDate >= checkOut) ||
                     (b.CheckInDate >= checkIn && b.CheckOutDate <= checkOut))))
                .CountAsync();

            return Json(new { available = availableRooms > 0, count = availableRooms });
        }
    }
}

