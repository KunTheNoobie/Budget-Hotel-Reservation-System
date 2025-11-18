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
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HotelDbContext _context;

        public HomeController(ILogger<HomeController> logger, HotelDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredRooms = await _context.RoomTypes
                .Include(rt => rt.Hotel)
                .Include(rt => rt.RoomImages)
                .Include(rt => rt.Rooms)
                    .ThenInclude(r => r.Bookings)
                        .ThenInclude(b => b.Review)
                .OrderBy(rt => rt.BasePrice)
                .Take(3)
                .ToListAsync();

            var reviewData = new Dictionary<int, RoomReviewInfo>();
            foreach (var room in featuredRooms)
            {
                var reviews = room.Rooms
                    .SelectMany(r => r.Bookings)
                    .Where(b => b.Review != null)
                    .Select(b => b.Review!)
                    .ToList();

                reviewData[room.RoomTypeId] = new RoomReviewInfo
                {
                    ReviewCount = reviews.Count,
                    AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 4.5
                };
            }

            var stats = new HomeStatsViewModel
            {
                HotelCount = await _context.Hotels.CountAsync(),
                HappyGuests = await _context.Bookings.Select(b => b.UserId).Distinct().CountAsync(),
                ActiveBookings = await _context.Bookings.CountAsync(b => b.Status != BookingStatus.Cancelled),
                AverageRating = await _context.Reviews.AnyAsync()
                    ? Math.Round(await _context.Reviews.AverageAsync(r => r.Rating), 1)
                    : 4.5
            };

            var packages = await _context.Packages
                .Where(p => p.IsActive)
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.Service)
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.RoomType)
                .Take(3)
                .ToListAsync();

            var packageSummaries = packages.Select(p => new PackageSummaryViewModel
            {
                Name = p.Name,
                Description = p.Description ?? "Curated stay with hand-picked perks.",
                TotalPrice = p.TotalPrice,
                Highlights = p.PackageItems
                    .Select(pi => pi.Service != null
                        ? $"{pi.Service.Name} x{pi.Quantity}"
                        : pi.RoomType != null
                            ? $"{pi.RoomType.Name} x{pi.Quantity}"
                            : string.Empty)
                    .Where(h => !string.IsNullOrWhiteSpace(h))
                    .Distinct()
                    .ToList()
            }).ToList();

            var services = await _context.Services
                .OrderBy(s => s.ServiceId)
                .Take(4)
                .ToListAsync();

            var destinations = await _context.Hotels
                .Select(h => h.City + ", " + h.Country)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            var model = new HomeViewModel
            {
                FeaturedRooms = featuredRooms,
                ReviewData = reviewData,
                Destinations = destinations,
                Stats = stats,
                Packages = packageSummaries,
                HighlightedServices = services
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Careers()
        {
            return View();
        }

        public IActionResult Press()
        {
            return View();
        }

        public IActionResult Blog()
        {
            return View();
        }

        public IActionResult HelpCenter()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
