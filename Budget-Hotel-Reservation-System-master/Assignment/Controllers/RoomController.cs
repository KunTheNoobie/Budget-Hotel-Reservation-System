using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    /// <summary>
    /// Controller for browsing and viewing room information.
    /// Provides room catalog with search and filtering capabilities, room details pages,
    /// and availability checking. Public access (no authentication required).
    /// </summary>
    public class RoomController : Controller
    {
        /// <summary>
        /// Database context for accessing room and related data.
        /// </summary>
        private readonly HotelDbContext _context;

        /// <summary>
        /// Logger for recording room browsing events and errors.
        /// </summary>
        private readonly ILogger<RoomController> _logger;

        /// <summary>
        /// Initializes a new instance of the RoomController.
        /// </summary>
        /// <param name="context">Database context for data access.</param>
        /// <param name="logger">Logger instance for logging.</param>
        public RoomController(HotelDbContext context, ILogger<RoomController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Displays the room catalog with search and filtering options.
        /// Supports filtering by search term, room type, maximum price, check-in date, and number of guests.
        /// Implements pagination for large result sets.
        /// </summary>
        /// <param name="searchTerm">Search term for filtering rooms by name or description.</param>
        /// <param name="roomTypeId">Optional room type ID to filter by specific room type.</param>
        /// <param name="maxPrice">Optional maximum price filter.</param>
        /// <param name="checkIn">Optional check-in date for availability checking.</param>
        /// <param name="guests">Optional number of guests for occupancy filtering.</param>
        /// <param name="page">Page number for pagination (default: 1).</param>
        /// <param name="pageSize">Number of items per page (default: 9).</param>
        /// <returns>The room catalog view with filtered results.</returns>
        public async Task<IActionResult> Catalog(string searchTerm = "", int? roomTypeId = null, decimal? maxPrice = null, DateTime? checkIn = null, int? guests = null, int page = 1, int pageSize = 9)
        {
            // ========== ROLE-BASED REDIRECTION ==========
            // Admin, Manager, and Staff should use the admin panel to view rooms
            // Only customers should see the public room catalog
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                var role = AuthenticationHelper.GetUserRole(HttpContext);
                if (role == UserRole.Admin || role == UserRole.Manager || role == UserRole.Staff)
                {
                    // Redirect admin users to admin panel
                    return RedirectToAction("Index", "Admin");
                }
            }
            
            // ========== INPUT VALIDATION ==========
            // Validate and sanitize all user input to prevent errors and security issues
            
            // Validate number of guests (must be between 1 and 20)
            // This prevents invalid values and ensures room occupancy requirements are met
            if (guests.HasValue && (guests.Value < 1 || guests.Value > 20))
            {
                ModelState.AddModelError("Guests", "Number of guests must be between 1 and 20.");
                guests = 1; // Set to minimum valid value
            }

            // Validate maximum price (cannot be negative)
            // Negative prices don't make sense and could cause calculation errors
            if (maxPrice.HasValue && maxPrice.Value < 0)
            {
                ModelState.AddModelError("MaxPrice", "Maximum price cannot be negative.");
                maxPrice = null; // Clear invalid value
            }

            // Validate search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Limit search term length to prevent performance issues
                // Very long search terms can slow down database queries
                if (searchTerm.Length > 200)
                {
                    ModelState.AddModelError("SearchTerm", "Search term cannot exceed 200 characters.");
                    searchTerm = searchTerm.Substring(0, 200); // Truncate to max length
                }
                
                // Check for invalid characters (security: prevent SQL injection attempts)
                // Only allow alphanumeric characters, spaces, commas, periods, and hyphens
                // This prevents malicious input that could be used in attacks
                if (!System.Text.RegularExpressions.Regex.IsMatch(searchTerm, @"^[a-zA-Z0-9\s,.-]+$"))
                {
                    ModelState.AddModelError("SearchTerm", "Search term contains invalid characters. Only letters, numbers, spaces, commas, periods, and hyphens are allowed.");
                }
            }

            // ========== DATE VALIDATION ==========
            // Validate check-in date - must be today or in the future
            // Past dates don't make sense for new bookings
            if (checkIn.HasValue && checkIn.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError("CheckIn", "Check-in date cannot be in the past. Please select today or a future date.");
                checkIn = DateTime.Today; // Set to today as default
            }

            // ========== BUILD BASE QUERY ==========
            // Start with all room types and include related data for display
            // Include Hotel (for location info), RoomImages (for photos), and Amenities (for features)
            var query = _context.RoomTypes
                .Include(rt => rt.Hotel)                    // Include hotel information (name, city, country)
                .Include(rt => rt.RoomImages)                // Include room photos for display
                .Include(rt => rt.RoomTypeAmenities)         // Include amenities relationship
                    .ThenInclude(rta => rta.Amenity)         // Include actual amenity details
                .AsQueryable();                              // Make it queryable so we can add filters

            // ========== SEARCH TERM FILTERING ==========
            // Apply search filter if user provided a search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Normalize search term: trim whitespace and convert to lowercase
                // This makes search case-insensitive and handles extra spaces
                var term = searchTerm.Trim().ToLower();
                
                // Require minimum 2 characters for meaningful search
                // Single character searches are too broad and can cause performance issues
                if (term.Length < 2)
                {
                    ViewBag.SearchTerm = searchTerm;
                    ViewBag.SearchError = "Please enter at least 2 characters to search.";
                    ViewBag.RoomTypeId = roomTypeId;
                    ViewBag.MaxPrice = maxPrice;
                    ViewBag.Guests = guests;
                    ViewBag.CheckIn = checkIn?.ToString("yyyy-MM-dd");
                    ViewBag.CurrentPage = 1;
                    ViewBag.TotalPages = 0;
                    ViewBag.PageSize = pageSize;
                    ViewBag.AllRoomTypes = await _context.RoomTypes.OrderBy(rt => rt.RoomTypeId).ToListAsync();
                    var maxPriceDb = await _context.RoomTypes.MaxAsync(rt => (decimal?)rt.BasePrice) ?? 0;
                    ViewBag.MaxPriceInDb = maxPriceDb > 0 ? maxPriceDb : 1000;
                    ViewBag.Destinations = await _context.Hotels
                        .Select(h => h.City + ", " + h.Country)
                        .Distinct()
                        .OrderBy(d => d)
                        .ToListAsync();
                    return View(new List<RoomType>());
                }
                
                // ========== LOCATION-BASED SEARCH ==========
                // Check if search term contains a comma (e.g., "Kuala Lumpur, Malaysia")
                // This indicates user is searching by location (city, country)
                var locationParts = term.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (locationParts.Length >= 2)
                {
                    // User provided location search (city, country format)
                    var cityTerm = locationParts[0];      // First part is city
                    var countryTerm = locationParts[1];   // Second part is country

                    // Search in hotel city, hotel name, and country
                    // Match city in hotel city or hotel name, AND country in hotel country or hotel name
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
                    // ========== GENERAL SEARCH ==========
                    // User provided general search term (not location-specific)
                    // Search in room type name, hotel name, city, and country
                    // Match if term appears at start of word or as significant match
                    query = query.Where(rt =>
                        rt.Name.ToLower().StartsWith(term) ||                    // Room name starts with term
                        rt.Name.ToLower().Contains(" " + term) ||                // Room name contains term as word
                        (rt.Hotel != null && (
                            rt.Hotel.Name.ToLower().StartsWith(term) ||          // Hotel name starts with term
                            rt.Hotel.Name.ToLower().Contains(" " + term) ||      // Hotel name contains term as word
                            rt.Hotel.City.ToLower().StartsWith(term) ||          // City starts with term
                            rt.Hotel.City.ToLower().Contains(" " + term) ||     // City contains term as word
                            rt.Hotel.Country.ToLower().StartsWith(term) ||      // Country starts with term
                            rt.Hotel.Country.ToLower().Contains(" " + term))));  // Country contains term as word
                }
            }

            // ========== ROOM TYPE FILTER ==========
            // Filter by specific room type if user selected one from dropdown
            if (roomTypeId.HasValue)
            {
                query = query.Where(rt => rt.RoomTypeId == roomTypeId.Value);
            }

            // ========== PRICE FILTER ==========
            // Filter by maximum price (show only rooms at or below this price)
            if (maxPrice.HasValue)
            {
                query = query.Where(rt => rt.BasePrice <= maxPrice.Value);
            }

            // ========== OCCUPANCY FILTER ==========
            // Filter by number of guests (show only rooms that can accommodate this many guests)
            // Room occupancy must be greater than or equal to requested number of guests
            if (guests.HasValue && guests.Value > 0)
            {
                query = query.Where(rt => rt.Occupancy >= guests.Value);
            }

            // ========== GET DISTINCT ROOM TYPES ==========
            // Get list of unique room type IDs that match the filters
            // This prevents duplicates and allows us to get full room type details separately
            var roomTypeIds = await query.Select(rt => rt.RoomTypeId).Distinct().ToListAsync();
            var totalCount = roomTypeIds.Count; // Total count for pagination
            
            // ========== LOAD FULL ROOM TYPE DATA WITH PAGINATION ==========
            // Load complete room type information including all related data
            // Apply pagination: skip previous pages and take only current page items
            var roomTypes = await _context.RoomTypes
                .Include(rt => rt.Hotel)                    // Hotel information
                .Include(rt => rt.RoomImages)               // Room photos
                .Include(rt => rt.RoomTypeAmenities)         // Amenities relationship
                    .ThenInclude(rta => rta.Amenity)        // Amenity details
                .Include(rt => rt.Rooms)                    // Physical rooms
                    .ThenInclude(r => r.Bookings)           // Booking history
                        .ThenInclude(b => b.Reviews)         // Reviews for bookings
                .Where(rt => roomTypeIds.Contains(rt.RoomTypeId))  // Only room types that matched filters
                .OrderBy(rt => rt.RoomTypeId)              // Order by ID for consistent pagination
                .Skip((page - 1) * pageSize)               // Skip items from previous pages
                .Take(pageSize)                             // Take only items for current page
                .ToListAsync();

            // ========== CALCULATE AVAILABLE ROOMS ==========
            // For each room type, count how many rooms are actually available
            // This is calculated from database to ensure accuracy (accounts for current bookings)
            // Always calculate from database to ensure accuracy
            var availableRoomsDict = new Dictionary<int, int>();
            foreach (var rt in roomTypes)
            {
                int availableCount;
                if (checkIn.HasValue)
                {
                    // If check-in date is provided, check for date conflicts
                    var checkOutDate = checkIn.Value.AddDays(1); // Default to 1 night if no check-out specified
                    availableCount = await _context.Rooms
                        .Where(r => r.RoomTypeId == rt.RoomTypeId && r.Status == RoomStatus.Available)
                        .Where(r => !_context.Bookings.Any(b => b.RoomId == r.RoomId &&
                            b.Status != BookingStatus.Cancelled &&
                            b.Status != BookingStatus.CheckedOut &&
                            b.Status != BookingStatus.NoShow &&
                            ((b.CheckInDate <= checkIn.Value && b.CheckOutDate > checkIn.Value) ||
                             (b.CheckInDate < checkOutDate && b.CheckOutDate >= checkOutDate) ||
                             (b.CheckInDate >= checkIn.Value && b.CheckOutDate <= checkOutDate))))
                        .CountAsync();
                }
                else
                {
                    // If no check-in date, count all available rooms without active bookings
                    // Exclude rooms with active bookings (Pending, Confirmed, CheckedIn) that haven't ended
                    availableCount = await _context.Rooms
                        .Where(r => r.RoomTypeId == rt.RoomTypeId && r.Status == RoomStatus.Available)
                        .Where(r => !_context.Bookings.Any(b => b.RoomId == r.RoomId &&
                            (b.Status == BookingStatus.Pending ||
                             b.Status == BookingStatus.Confirmed ||
                             b.Status == BookingStatus.CheckedIn) &&
                            b.CheckOutDate > DateTime.Today))
                        .CountAsync();
                }
                availableRoomsDict[rt.RoomTypeId] = availableCount;
            }
            ViewBag.AvailableRoomsDict = availableRoomsDict;

            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoomTypeId = roomTypeId;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Guests = guests;
            ViewBag.CheckIn = checkIn?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.AllRoomTypes = await _context.RoomTypes.OrderBy(rt => rt.RoomTypeId).ToListAsync();
            var maxPriceInDb = await _context.RoomTypes.MaxAsync(rt => (decimal?)rt.BasePrice) ?? 0;
            ViewBag.MaxPriceInDb = maxPriceInDb > 0 ? maxPriceInDb : 1000; // Default to 1000 if no rooms exist
            ViewBag.Destinations = await _context.Hotels
                .Select(h => h.City + ", " + h.Country)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            return View(roomTypes);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, int reviewPage = 1, int reviewPageSize = 9)
        {
            var roomType = await _context.RoomTypes
                .Include(rt => rt.Hotel)
                .Include(rt => rt.RoomImages)
                .Include(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
                .Include(rt => rt.Rooms)
                    .ThenInclude(r => r.Bookings)
                        .ThenInclude(b => b.Reviews)
                .Include(rt => rt.Rooms)
                    .ThenInclude(r => r.Bookings)
                        .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == id);

            if (roomType == null)
            {
                return NotFound();
            }

            // Get available rooms count (excluding booked rooms)
            // Exclude rooms with any active bookings (Pending, Confirmed, CheckedIn) that haven't ended
            // This ensures real-time sync after bookings are made
            var availableRooms = await _context.Rooms
                .Where(r => r.RoomTypeId == id && r.Status == RoomStatus.Available)
                .Where(r => !_context.Bookings.Any(b => b.RoomId == r.RoomId &&
                    (b.Status == BookingStatus.Pending ||
                     b.Status == BookingStatus.Confirmed ||
                     b.Status == BookingStatus.CheckedIn) &&
                    b.CheckOutDate > DateTime.Today))
                .CountAsync();
            ViewBag.AvailableRooms = availableRooms;

            // Get all reviews for this room type
            var allReviews = roomType.Rooms
                .SelectMany(r => r.Bookings)
                .SelectMany(b => b.Reviews)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();

            // Paginate reviews
            var totalReviews = allReviews.Count;
            var paginatedReviews = allReviews
                .Skip((reviewPage - 1) * reviewPageSize)
                .Take(reviewPageSize)
                .ToList();

            ViewBag.AllReviews = allReviews;
            ViewBag.PaginatedReviews = paginatedReviews;
            ViewBag.ReviewCurrentPage = reviewPage;
            ViewBag.ReviewTotalPages = (int)Math.Ceiling(totalReviews / (double)reviewPageSize);
            ViewBag.ReviewPageSize = reviewPageSize;

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
                
                // Require minimum 2 characters for meaningful search
                if (lowerTerm.Length < 2)
                {
                    return Json(new List<object>());
                }
                
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
                    // More strict matching - require the term to be at the start of words or be a significant match
                    query = query.Where(rt =>
                        rt.Name.ToLower().StartsWith(lowerTerm) ||
                        rt.Name.ToLower().Contains(" " + lowerTerm) ||
                        (rt.Hotel != null && (
                            rt.Hotel.Name.ToLower().StartsWith(lowerTerm) ||
                            rt.Hotel.Name.ToLower().Contains(" " + lowerTerm) ||
                            rt.Hotel.City.ToLower().StartsWith(lowerTerm) ||
                            rt.Hotel.City.ToLower().Contains(" " + lowerTerm) ||
                            rt.Hotel.Country.ToLower().StartsWith(lowerTerm) ||
                            rt.Hotel.Country.ToLower().Contains(" " + lowerTerm))));
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

            // Get distinct room type IDs first to avoid duplicates
            var distinctRoomTypeIds = await query
                .Select(rt => rt.RoomTypeId)
                .Distinct()
                .ToListAsync();
            
            // Then fetch the full room type data for those IDs, ordered by price
            var roomTypesData = await _context.RoomTypes
                .Include(rt => rt.Hotel)
                .Include(rt => rt.RoomImages)
                .Include(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
                .Include(rt => rt.Rooms)
                    .ThenInclude(r => r.Bookings)
                        .ThenInclude(b => b.Reviews)
                .Where(rt => distinctRoomTypeIds.Contains(rt.RoomTypeId))
                .OrderBy(rt => rt.BasePrice)
                .Take(9)
                .ToListAsync();
            
            // Project to anonymous type after materialization to avoid EF translation issues
            var roomTypes = roomTypesData.Select(rt => {
                var reviews = rt.Rooms
                    .SelectMany(r => r.Bookings)
                    .SelectMany(b => b.Reviews)
                    .ToList();
                var avgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                var reviewCount = reviews.Count;
                
                // Calculate available rooms - exclude rooms with overlapping bookings
                // Note: For AJAX search, we don't have check-in date, so we just count Available status rooms
                // The actual availability check happens when user selects dates
                var availableRooms = rt.Rooms.Count(r => r.Status == RoomStatus.Available &&
                    !r.Bookings.Any(b => b.Status != BookingStatus.Cancelled && 
                                        b.Status != BookingStatus.CheckedOut &&
                                        b.Status != BookingStatus.NoShow &&
                                        b.CheckOutDate > DateTime.Today));
                
                return new
            {
                RoomTypeId = rt.RoomTypeId,
                Name = rt.Name,
                Description = rt.Description,
                BasePrice = rt.BasePrice,
                Occupancy = rt.Occupancy,
                Location = rt.Hotel != null ? rt.Hotel.City + ", " + rt.Hotel.Country : "Malaysia",
                    HotelName = rt.Hotel?.Name,
                    HotelImageUrl = rt.Hotel?.ImageUrl,
                ImageUrl = rt.RoomImages.FirstOrDefault()?.ImageUrl,
                    Amenities = rt.RoomTypeAmenities.Select(rta => new { Name = rta.Amenity.Name, ImageUrl = rta.Amenity.ImageUrl }).ToList(),
                    AverageRating = avgRating,
                    ReviewCount = reviewCount,
                    AvailableRooms = availableRooms
                };
            }).ToList();

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

            // Get fresh availability count from database, excluding all conflicting bookings
            var availableRooms = await _context.Rooms
                .Where(r => r.RoomTypeId == roomTypeId && r.Status == RoomStatus.Available)
                .Where(r => !_context.Bookings.Any(b => b.RoomId == r.RoomId &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.CheckedOut &&
                    b.Status != BookingStatus.NoShow &&
                    ((b.CheckInDate <= checkIn && b.CheckOutDate > checkIn) ||
                     (b.CheckInDate < checkOut && b.CheckOutDate >= checkOut) ||
                     (b.CheckInDate >= checkIn && b.CheckOutDate <= checkOut))))
                .CountAsync();

            return Json(new { available = availableRooms > 0, count = availableRooms });
        }
    }
}

