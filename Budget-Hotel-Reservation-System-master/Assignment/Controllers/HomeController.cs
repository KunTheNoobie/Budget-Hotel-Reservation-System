using System.Diagnostics;
using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Helpers;
using Assignment.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Assignment.Controllers
{
    /// <summary>
    /// Controller for handling home page and public-facing pages.
    /// Manages the main landing page, featured rooms, packages display, and public information pages.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Logger for recording application events and errors.
        /// </summary>
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Database context for accessing hotel reservation data.
        /// </summary>
        private readonly HotelDbContext _context;

        /// <summary>
        /// Initializes a new instance of the HomeController.
        /// </summary>
        /// <param name="logger">Logger instance for logging.</param>
        /// <param name="context">Database context for data access.</param>
        public HomeController(ILogger<HomeController> logger, HotelDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Displays the home page with featured rooms, packages, and statistics.
        /// Randomly selects 3 featured rooms that change on each page load.
        /// </summary>
        /// <returns>The home page view with featured content.</returns>
        public async Task<IActionResult> Index()
        {
            // ========== ROLE-BASED REDIRECTION ==========
            // Admin, Manager, and Staff should use the admin panel, not the public home page
            // Only customers see the public home page with featured rooms
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                var role = AuthenticationHelper.GetUserRole(HttpContext);
                if (role == UserRole.Admin || role == UserRole.Manager || role == UserRole.Staff)
                {
                    // Redirect admin users to admin dashboard
                    return RedirectToAction("Index", "Admin");
                }
            }

            // ========== LOAD ALL ROOM TYPES ==========
            // Load all room types with related data for featured rooms selection
            // Include Hotel (for location), RoomImages (for photos), Rooms (for availability), Bookings, and Reviews
            var allRooms = await _context.RoomTypes
                .Include(rt => rt.Hotel)                    // Hotel information
                .Include(rt => rt.RoomImages)               // Room photos
                .Include(rt => rt.Rooms)                    // Physical rooms
                    .ThenInclude(r => r.Bookings)           // Booking history
                        .ThenInclude(b => b.Reviews)         // Reviews for bookings
                .ToListAsync();
            
            // ========== SELECT FEATURED ROOMS ==========
            // Randomly select exactly 3 different rooms that change on each page refresh
            // This makes the home page dynamic and shows different rooms each time
            // Use Random with current time as seed for better randomization per request
            var random = new Random((int)DateTime.Now.Ticks);
            var featuredRooms = allRooms
                .OrderBy(r => random.Next())  // Randomize order
                .Take(3)                      // Take first 3
                .ToList();
            
            // ========== ENSURE MINIMUM FEATURED ROOMS ==========
            // If we have less than 3 rooms but more rooms available, add more to reach 3
            // This ensures the home page always shows 3 featured rooms (if available)
            if (featuredRooms.Count < 3 && allRooms.Count > featuredRooms.Count)
            {
                var remainingRooms = allRooms.Except(featuredRooms).OrderBy(r => random.Next()).Take(3 - featuredRooms.Count);
                featuredRooms.AddRange(remainingRooms);
            }

            // ========== CALCULATE REVIEW DATA ==========
            // For each featured room, calculate review statistics (count and average rating)
            // This data is displayed on the home page to show room popularity
            var reviewData = new Dictionary<int, RoomReviewInfo>();
            foreach (var room in featuredRooms)
            {
                // Get all reviews for this room type (from all bookings of all rooms of this type)
                var reviews = room.Rooms
                    .SelectMany(r => r.Bookings)      // Get all bookings for all rooms of this type
                    .SelectMany(b => b.Reviews)        // Get all reviews for those bookings
                    .ToList();

                // Store review statistics for this room type
                reviewData[room.RoomTypeId] = new RoomReviewInfo
                {
                    ReviewCount = reviews.Count,                                    // Total number of reviews
                    AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 4.5  // Average rating (default 4.5 if no reviews)
                };
            }

            // ========== CALCULATE STATISTICS ==========
            // Calculate statistics to display on home page (builds trust and credibility)
            var hotelCount = await _context.Hotels.CountAsync();                    // Total number of hotels
            var happyGuestsCount = await _context.Bookings.Select(b => b.UserId).Distinct().CountAsync();  // Unique customers who have booked
            var averageRating = await _context.Reviews.AnyAsync()
                ? Math.Round(await _context.Reviews.AverageAsync(r => r.Rating), 1)  // Average rating from all reviews
                : 4.6;                                                              // Default rating if no reviews exist
            
            // ========== CREATE STATISTICS VIEW MODEL ==========
            // Package statistics for display on home page
            var stats = new HomeStatsViewModel
            {
                // Use actual count if higher than minimum, otherwise use minimum for display
                // This ensures statistics always look good (minimum values for marketing)
                HotelCount = hotelCount >= 10 ? hotelCount : 10,                    // At least 10 hotels
                HappyGuests = happyGuestsCount >= 2 ? happyGuestsCount : 2,        // At least 2 happy guests
                ActiveBookings = await _context.Bookings.CountAsync(b => b.Status != BookingStatus.Cancelled),  // Active bookings (not cancelled)
                AverageRating = averageRating
            };

            // ========== LOAD AND SELECT FEATURED PACKAGES ==========
            // Get all active packages and randomly select 3 to feature on home page
            // Packages include room types and services bundled together
            var allPackages = await _context.Packages
                .Where(p => p.IsActive)                    // Only active packages
                .Include(p => p.PackageItems)              // Include package items
                    .ThenInclude(pi => pi.Service)         // Include service details
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.RoomType)       // Include room type details
                .ToListAsync();
            
            // Randomly select exactly 3 packages (or all if less than 3 available)
            // This makes the home page dynamic - different packages shown each time
            var packages = allPackages
                .OrderBy(p => random.Next())              // Randomize order
                .Take(Math.Min(3, allPackages.Count))    // Take up to 3 packages
                .ToList();

            // ========== CREATE PACKAGE SUMMARIES ==========
            // Convert packages to view models with highlights for display
            var packageSummaries = packages.Select(p => new PackageSummaryViewModel
            {
                PackageId = p.PackageId,
                Name = p.Name,
                Description = p.Description ?? "Curated stay with hand-picked perks.",  // Default description if none
                TotalPrice = p.TotalPrice,
                ImageUrl = p.ImageUrl,
                // Create highlights list showing what's included in the package
                // Format: "Service Name xQuantity" or "Room Type Name xQuantity"
                Highlights = p.PackageItems
                    .Select(pi => pi.Service != null
                        ? $"{pi.Service.Name} x{pi.Quantity}"      // Service highlight
                        : pi.RoomType != null
                            ? $"{pi.RoomType.Name} x{pi.Quantity}"  // Room type highlight
                            : string.Empty)
                    .Where(h => !string.IsNullOrWhiteSpace(h))      // Remove empty highlights
                    .Distinct()                                     // Remove duplicates
                    .ToList(),
                RoomTypeId = p.PackageItems.FirstOrDefault(pi => pi.RoomType != null)?.RoomTypeId  // Get room type ID for booking link
            }).ToList();

            // ========== LOAD AND SELECT FEATURED SERVICES ==========
            // Get all services and randomly select 4 to feature on home page
            // Services are additional amenities customers can purchase
            var allServices = await _context.Services.ToListAsync();
            
            // Randomly select exactly 4 services (or all if less than 4 available)
            var services = allServices
                .OrderBy(s => random.Next())              // Randomize order
                .Take(Math.Min(4, allServices.Count))    // Take up to 4 services
                .ToList();

            // ========== GET DESTINATIONS ==========
            // Get list of unique destinations (City, Country) for search/filter dropdown
            // This helps users find hotels in specific locations
            var destinations = await _context.Hotels
                .Select(h => h.City + ", " + h.Country)  // Format: "Kuala Lumpur, Malaysia"
                .Distinct()                              // Remove duplicates
                .OrderBy(d => d)                         // Sort alphabetically
                .ToListAsync();

            // ========== LOAD CUSTOMER REVIEWS FOR TESTIMONIALS ==========
            // Get customer reviews to display in testimonials section on home page
            // Reviews are linked to Booking (not directly to User), so we get user info from Booking.User
            var allReviews = await _context.Reviews
                .Include(r => r.Booking)                 // Include booking
                    .ThenInclude(b => b.User)            // Include user (for reviewer name)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Room)            // Include room
                        .ThenInclude(rm => rm.RoomType)  // Include room type (for room name)
                .Where(r => r.Booking != null && r.Booking.User != null)  // Ensure booking and user exist
                .OrderByDescending(r => r.ReviewDate)    // Get most recent reviews first
                .ToListAsync();
            
            // ========== FILTER REVIEWS ==========
            // Prefer reviews with comments (more meaningful testimonials)
            // If no reviews with comments exist, show any reviews
            var reviewsWithComments = allReviews.Where(r => !string.IsNullOrWhiteSpace(r.Comment)).ToList();
            var reviewsToShow = reviewsWithComments.Any() ? reviewsWithComments : allReviews;
            
            // ========== SELECT FEATURED REVIEWS ==========
            // Randomly select exactly 3 reviews (or all if less than 3 available)
            // This makes testimonials section dynamic
            var customerReviews = reviewsToShow
                .OrderBy(r => random.Next())              // Randomize order
                .Take(Math.Min(3, reviewsToShow.Count))  // Take up to 3 reviews
                .ToList();

            var model = new HomeViewModel
            {
                FeaturedRooms = featuredRooms,
                ReviewData = reviewData,
                Destinations = destinations,
                Stats = stats,
                Packages = packageSummaries,
                HighlightedServices = services,
                CustomerReviews = customerReviews
            };

            return View(model);
        }

        public async Task<IActionResult> Packages(int page = 1, int pageSize = 9)
        {
            // Redirect Admin/Manager/Staff to admin panel
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                var role = AuthenticationHelper.GetUserRole(HttpContext);
                if (role == UserRole.Admin || role == UserRole.Manager || role == UserRole.Staff)
                {
                    return RedirectToAction("Index", "Admin");
                }
            }

            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 9;

            var packagesQuery = _context.Packages
                .Where(p => p.IsActive)
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.Service)
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.RoomType)
                        .ThenInclude(rt => rt.Hotel);

            var totalCount = await packagesQuery.CountAsync();

            var packages = await packagesQuery
                .OrderBy(p => p.PackageId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var packageSummaries = packages.Select(p => {
                // Get room type directly from PackageItems - Hotel should be loaded via ThenInclude
                var roomType = p.PackageItems
                    .Where(pi => pi.RoomType != null)
                    .Select(pi => pi.RoomType)
                    .FirstOrDefault();
                
                var services = p.PackageItems.Where(pi => pi.Service != null).ToList();
                var roomItems = p.PackageItems.Where(pi => pi.RoomType != null).ToList();
                
                // Calculate individual prices for savings display
                decimal individualPrice = 0;
                if (roomType != null)
                {
                    individualPrice += roomType.BasePrice * (roomItems.Any() ? roomItems.Sum(ri => ri.Quantity) : 1);
                }
                foreach (var serviceItem in services)
                {
                    if (serviceItem.Service != null)
                    {
                        individualPrice += serviceItem.Service.Price * serviceItem.Quantity;
                    }
                }
                
                return new PackageSummaryViewModel
                {
                    PackageId = p.PackageId,
                    Name = p.Name,
                    Description = p.Description ?? "Curated stay with hand-picked perks.",
                    TotalPrice = p.TotalPrice,
                    ImageUrl = p.ImageUrl,
                    Highlights = p.PackageItems
                        .Select(pi => pi.Service != null
                            ? $"{pi.Service.Name} x{pi.Quantity}"
                            : pi.RoomType != null
                                ? $"{pi.RoomType.Name} x{pi.Quantity}"
                                : string.Empty)
                        .Where(h => !string.IsNullOrWhiteSpace(h))
                        .Distinct()
                        .ToList(),
                    RoomTypeId = roomType?.RoomTypeId,
                    HotelName = roomType?.Hotel?.Name,
                    Location = roomType?.Hotel != null ? $"{roomType.Hotel.City}, {roomType.Hotel.Country}" : null,
                    IndividualPrice = individualPrice > 0 ? individualPrice : null,
                    RoomTypeName = roomType?.Name,
                    Occupancy = roomType?.Occupancy
                };
            }).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;

            return View(packageSummaries);
        }

        public async Task<IActionResult> PackageDetails(int id)
        {
            var package = await _context.Packages
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.Service)
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.RoomType)
                        .ThenInclude(rt => rt.Hotel)
                .FirstOrDefaultAsync(p => p.PackageId == id && p.IsActive);

            if (package == null)
            {
                return NotFound();
            }

            // Get room type ID from package items first
            var roomTypeIdFromPackage = package.PackageItems
                .Where(pi => pi.RoomTypeId.HasValue)
                .Select(pi => pi.RoomTypeId.Value)
                .FirstOrDefault();
            
            var services = package.PackageItems
                .Where(pi => pi.Service != null && pi.ServiceId.HasValue)
                .ToList();
            
            // Get room items - ensure we're getting items with RoomTypeId
            var roomItems = package.PackageItems
                .Where(pi => pi.RoomTypeId.HasValue)
                .ToList();
            
            // Always reload room type with Hotel to ensure correct data
            // This is necessary because navigation properties from ThenInclude might not always work correctly
            RoomType? roomType = null;
            if (roomTypeIdFromPackage > 0)
            {
                roomType = await _context.RoomTypes
                    .Include(rt => rt.Hotel)
                    .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeIdFromPackage);
            }
            
            // If no room type found in package items, use fallback for booking only
            // But don't show wrong hotel info in the display
            RoomType? fallbackRoomType = null;
            if (roomType == null)
            {
                // For booking purposes only, use fallback
                fallbackRoomType = await _context.RoomTypes
                    .Include(rt => rt.Hotel)
                    .OrderBy(rt => rt.RoomTypeId)
                    .FirstOrDefaultAsync();
            }
            
            // Calculate individual prices for savings display
            decimal individualPrice = 0;
            if (roomType != null)
            {
                individualPrice += roomType.BasePrice * (roomItems.Any() ? roomItems.Sum(ri => ri.Quantity) : 1);
            }
            foreach (var serviceItem in services)
            {
                if (serviceItem.Service != null)
                {
                    individualPrice += serviceItem.Service.Price * serviceItem.Quantity;
                }
            }

            var packageSummary = new PackageSummaryViewModel
            {
                PackageId = package.PackageId,
                Name = package.Name,
                Description = package.Description ?? "Curated stay with hand-picked perks.",
                TotalPrice = package.TotalPrice,
                ImageUrl = package.ImageUrl,
                Highlights = package.PackageItems
                    .Select(pi => pi.Service != null
                        ? $"{pi.Service.Name} x{pi.Quantity}"
                        : pi.RoomType != null
                            ? $"{pi.RoomType.Name} x{pi.Quantity}"
                            : string.Empty)
                    .Where(h => !string.IsNullOrWhiteSpace(h))
                    .Distinct()
                    .ToList(),
                // Use actual room type from package, or fallback for booking only
                RoomTypeId = roomType?.RoomTypeId ?? fallbackRoomType?.RoomTypeId,
                // Only set hotel info from the ACTUAL package room type, NOT from fallback
                // This ensures correct location is displayed
                HotelName = roomType?.Hotel?.Name,
                Location = roomType?.Hotel != null ? $"{roomType.Hotel.City}, {roomType.Hotel.Country}" : null,
                IndividualPrice = individualPrice > 0 ? individualPrice : null,
                RoomTypeName = roomType?.Name,
                Occupancy = roomType?.Occupancy
            };

            // Pass detailed package information to ViewBag
            // Always use the correctly loaded room type (not fallback for display)
            ViewBag.Package = package;
            ViewBag.RoomType = roomType; // Use the correctly loaded room type with Hotel
            ViewBag.FallbackRoomType = fallbackRoomType; // Only for booking if needed
            ViewBag.Services = services;
            ViewBag.RoomItems = roomItems;
            ViewBag.IndividualPrice = individualPrice;
            ViewBag.IsAuthenticated = AuthenticationHelper.IsAuthenticated(HttpContext);
            
            // Also pass hotel information separately to ensure it's available
            if (roomType?.Hotel != null)
            {
                ViewBag.Hotel = roomType.Hotel;
            }
            
            // Get room amenities and images if room type exists
            if (roomType != null)
            {
                var amenities = await _context.RoomTypeAmenities
                    .Where(rta => rta.RoomTypeId == roomType.RoomTypeId)
                    .Include(rta => rta.Amenity)
                    .Select(rta => rta.Amenity)
                    .OrderBy(a => a.AmenityId)
                    .ToListAsync();
                ViewBag.Amenities = amenities;

                // Get room images
                var roomImages = await _context.RoomImages
                    .Where(ri => ri.RoomTypeId == roomType.RoomTypeId)
                    .ToListAsync();
                ViewBag.RoomImages = roomImages;

                // Get reviews for this room type (Review is linked to Booking, user info from Booking.User)
                var reviews = await _context.Reviews
                    .Include(r => r.Booking)
                        .ThenInclude(b => b.User)
                    .Include(r => r.Booking)
                        .ThenInclude(b => b.Room)
                            .ThenInclude(rm => rm.RoomType)
                    .Where(r => r.Booking != null && r.Booking.Room != null && r.Booking.Room.RoomTypeId == roomType.RoomTypeId)
                    .OrderByDescending(r => r.ReviewDate)
                    .Take(5)
                    .ToListAsync();
                ViewBag.Reviews = reviews;
                ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                ViewBag.ReviewCount = reviews.Count;
            }

            return View(packageSummary);
        }

        public IActionResult Privacy()
        {
            // Redirect Admin/Manager/Staff to admin panel
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                var role = AuthenticationHelper.GetUserRole(HttpContext);
                if (role == UserRole.Admin || role == UserRole.Manager || role == UserRole.Staff)
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            return View();
        }

        public IActionResult About()
        {
            // Redirect Admin/Manager/Staff to admin panel
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                var role = AuthenticationHelper.GetUserRole(HttpContext);
                if (role == UserRole.Admin || role == UserRole.Manager || role == UserRole.Staff)
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            return View();
        }

        public IActionResult Careers()
        {
            // Redirect Admin/Manager/Staff to admin panel
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                var role = AuthenticationHelper.GetUserRole(HttpContext);
                if (role == UserRole.Admin || role == UserRole.Manager || role == UserRole.Staff)
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            return View();
        }

        public IActionResult Press()
        {
            // Redirect Admin/Manager/Staff to admin panel
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                var role = AuthenticationHelper.GetUserRole(HttpContext);
                if (role == UserRole.Admin || role == UserRole.Manager || role == UserRole.Staff)
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            return View();
        }

        public IActionResult Blog()
        {
            // Redirect Admin/Manager/Staff to admin panel
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                var role = AuthenticationHelper.GetUserRole(HttpContext);
                if (role == UserRole.Admin || role == UserRole.Manager || role == UserRole.Staff)
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubscribeNewsletter(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                TempData["NewsletterError"] = "Please enter a valid email address.";
                return RedirectToAction("Blog");
            }

            try
            {
                // Check if email already exists
                var existing = await _context.Newsletters.FirstOrDefaultAsync(n => n.Email == email);
                if (existing != null)
                {
                    if (existing.IsActive)
                    {
                        TempData["NewsletterInfo"] = "You're already subscribed to our newsletter!";
                    }
                    else
                    {
                        existing.IsActive = true;
                        existing.SubscribedAt = DateTime.Now;
                        await _context.SaveChangesAsync();
                        TempData["NewsletterSuccess"] = "Thank you for resubscribing to our newsletter!";
                    }
                }
                else
                {
                    var newsletter = new Newsletter
                    {
                        Email = email,
                        SubscribedAt = DateTime.Now,
                        IsActive = true
                    };
                    _context.Newsletters.Add(newsletter);
                    await _context.SaveChangesAsync();
                    TempData["NewsletterSuccess"] = "Thank you for subscribing to our newsletter!";
                }
            }
            catch (Exception)
            {
                TempData["NewsletterError"] = "An error occurred while subscribing. Please try again.";
            }

            return RedirectToAction("Blog");
        }

        public IActionResult HelpCenter()
        {
            // Redirect Admin/Manager/Staff to admin panel
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                var role = AuthenticationHelper.GetUserRole(HttpContext);
                if (role == UserRole.Admin || role == UserRole.Manager || role == UserRole.Staff)
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            return View();
        }

        public IActionResult Contact()
        {
            // Redirect Admin/Manager/Staff to admin panel
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                var role = AuthenticationHelper.GetUserRole(HttpContext);
                if (role == UserRole.Admin || role == UserRole.Manager || role == UserRole.Staff)
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            return View();
        }

        public IActionResult Terms()
        {
            // Redirect Admin/Manager/Staff to admin panel
            if (AuthenticationHelper.IsAuthenticated(HttpContext))
            {
                var role = AuthenticationHelper.GetUserRole(HttpContext);
                if (role == UserRole.Admin || role == UserRole.Manager || role == UserRole.Staff)
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? id = null)
        {
            // Use 'id' parameter to match default route pattern {controller}/{action}/{id}
            // When UseStatusCodePagesWithReExecute("/Home/Error/{0}") is called,
            // the status code is passed as the 'id' parameter
            if (id == 404)
                return View("NotFound");
            if (id == 403)
                return View("Forbidden");
            if (id == 400)
                return View("BadRequest");
            
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
