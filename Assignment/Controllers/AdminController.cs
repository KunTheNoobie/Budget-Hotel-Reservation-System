using Assignment.Attributes;
using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Services;
using Assignment.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    [AuthorizeRole(UserRole.Admin, UserRole.Manager, UserRole.Staff)]
    public class AdminController : Controller
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(HotelDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var stats = new
            {
                TotalUsers = _context.Users.Count(),
                TotalHotels = _context.Hotels.Count(),
                TotalRooms = _context.Rooms.Count(),
                TotalBookings = _context.Bookings.Count(),
                PendingBookings = _context.Bookings.Count(b => b.Status == BookingStatus.Pending),
                ConfirmedBookings = _context.Bookings.Count(b => b.Status == BookingStatus.Confirmed)
            };

            ViewBag.Stats = stats;
            return View();
        }

        #region User Management

        public async Task<IActionResult> Users(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            var query = _context.Users.Include(u => u.UserProfile).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.FullName.Contains(searchTerm) || u.Email.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;

            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user, string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                ModelState.AddModelError("Password", "Password must be at least 8 characters.");
                return View(user);
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                user.PasswordHash = PasswordService.HashPassword(password);
                user.CreatedAt = DateTime.Now;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User created successfully.";
                return RedirectToAction("Users");
            }

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, User user, string? newPassword)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.UserId != id))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Role = user.Role;
                existingUser.IsActive = user.IsActive;
                existingUser.IsEmailVerified = user.IsEmailVerified;

                if (!string.IsNullOrEmpty(newPassword) && newPassword.Length >= 8)
                {
                    existingUser.PasswordHash = PasswordService.HashPassword(newPassword);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "User updated successfully.";
                return RedirectToAction("Users");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deletion of main admin account
            if (user.Email == "admin@hotel.com")
            {
                TempData["Error"] = "Cannot delete the main admin account.";
                return RedirectToAction("Users");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction("Users");
        }

        #endregion

        #region Hotel Management

        public async Task<IActionResult> Hotels(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            var query = _context.Hotels.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(h => h.Name.Contains(searchTerm) || h.City.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var hotels = await query
                .OrderBy(h => h.HotelId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;

            return View(hotels);
        }

        [HttpGet]
        public IActionResult CreateHotel()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHotel(Hotel hotel)
        {
            if (ModelState.IsValid)
            {
                _context.Hotels.Add(hotel);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Hotel created successfully.";
                return RedirectToAction("Hotels");
            }

            return View(hotel);
        }

        [HttpGet]
        public async Task<IActionResult> EditHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            return View(hotel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHotel(int id, Hotel hotel)
        {
            if (id != hotel.HotelId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(hotel);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Hotel updated successfully.";
                return RedirectToAction("Hotels");
            }

            return View(hotel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Hotel deleted successfully.";
            return RedirectToAction("Hotels");
        }

        #endregion

        #region Room Management

        public async Task<IActionResult> Rooms(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            var query = _context.Rooms.Include(r => r.RoomType).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r => r.RoomNumber.Contains(searchTerm) || r.RoomType.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var rooms = await query
                .OrderBy(r => r.RoomId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();

            return View(rooms);
        }

        [HttpGet]
        public async Task<IActionResult> CreateRoom()
        {
            ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom(Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Room created successfully.";
                return RedirectToAction("Rooms");
            }

            ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();
            return View(room);
        }

        [HttpGet]
        public async Task<IActionResult> EditRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int id, Room room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(room);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Room updated successfully.";
                return RedirectToAction("Rooms");
            }

            ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Room deleted successfully.";
            return RedirectToAction("Rooms");
        }

        #endregion

        #region Booking Management

        public async Task<IActionResult> Bookings(string searchTerm = "", BookingStatus? status = null, int page = 1, int pageSize = 10)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                    .ThenInclude(r => r.RoomType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => b.User.Email.Contains(searchTerm) || b.User.FullName.Contains(searchTerm));
            }

            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }

            var totalCount = await query.CountAsync();
            var bookings = await query
                .OrderByDescending(b => b.BookingId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBookingStatus(int id, BookingStatus status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = status;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Booking status updated successfully.";
            return RedirectToAction("Bookings");
        }

        [HttpGet]
        public async Task<IActionResult> BookingDetails(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(b => b.Payment)
                .Include(b => b.Promotion)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        #endregion
    }
}

