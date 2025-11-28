using Assignment.Attributes;
using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Services;
using Assignment.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Assignment.Controllers
{
    /// <summary>
    /// Controller for administrative operations and management functions.
    /// Provides comprehensive admin dashboard, CRUD operations for all entities,
    /// user management, hotel management, booking management, and system statistics.
    /// Requires Admin, Manager, or Staff role for access.
    /// </summary>
    [AuthorizeRole(UserRole.Admin, UserRole.Manager, UserRole.Staff)]
    public class AdminController : Controller
    {
        /// <summary>
        /// Database context for accessing all system data.
        /// </summary>
        private readonly HotelDbContext _context;

        /// <summary>
        /// Logger for recording admin operations and errors.
        /// </summary>
        private readonly ILogger<AdminController> _logger;

        /// <summary>
        /// Web host environment for file operations (image uploads, etc.).
        /// </summary>
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Initializes a new instance of the AdminController.
        /// </summary>
        /// <param name="context">Database context for data access.</param>
        /// <param name="logger">Logger instance for logging.</param>
        /// <param name="environment">Web host environment for file operations.</param>
        public AdminController(HotelDbContext context, ILogger<AdminController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Validates and sanitizes search parameters to prevent injection attacks and ensure data integrity.
        /// </summary>
        /// <param name="searchTerm">Search term to validate (max 200 characters, alphanumeric only).</param>
        /// <param name="page">Page number (ensures minimum value of 1).</param>
        /// <param name="pageSize">Page size (ensures value between 1 and 100, defaults to 10).</param>
        private void ValidateSearchParameters(ref string searchTerm, ref int page, ref int pageSize)
        {
            // Validate search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (searchTerm.Length > 200)
                {
                    ModelState.AddModelError("SearchTerm", "Search term cannot exceed 200 characters.");
                    searchTerm = searchTerm.Substring(0, 200);
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(searchTerm, @"^[a-zA-Z0-9\s@.-]+$"))
                {
                    ModelState.AddModelError("SearchTerm", "Search term contains invalid characters.");
                }
            }

            // Validate pagination
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
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

            // Data for Charts
            // 1. Bookings by Hotel
            var bookingsByHotel = _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
                .ThenInclude(rt => rt.Hotel)
                .GroupBy(b => b.Room.RoomType.Hotel.Name)
                .Select(g => new { HotelName = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.HotelLabels = bookingsByHotel.Select(x => x.HotelName).ToArray();
            ViewBag.HotelData = bookingsByHotel.Select(x => x.Count).ToArray();

            // 2. Revenue Trend (Last 7 Days)
            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-i)).Reverse().ToList();
            var revenueData = new List<decimal>();
            var dateLabels = new List<string>();

            foreach (var date in last7Days)
            {
                var dailyRevenue = _context.Bookings
                    .Where(b => b.PaymentDate.HasValue && 
                               b.PaymentDate.Value.Date == date && 
                               b.PaymentStatus == PaymentStatus.Completed)
                    .Sum(b => b.PaymentAmount ?? 0);
                
                revenueData.Add(dailyRevenue);
                dateLabels.Add(date.ToString("MMM dd"));
            }

            ViewBag.RevenueLabels = dateLabels.ToArray();
            ViewBag.RevenueData = revenueData.ToArray();

            ViewBag.Stats = stats;
            return View();
        }

        #region User Management

        public async Task<IActionResult> Users(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            // Validate search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (searchTerm.Length > 200)
                {
                    ModelState.AddModelError("SearchTerm", "Search term cannot exceed 200 characters.");
                    searchTerm = searchTerm.Substring(0, 200);
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(searchTerm, @"^[a-zA-Z0-9\s@.-]+$"))
                {
                    ModelState.AddModelError("SearchTerm", "Search term contains invalid characters.");
                }
            }

            // Validate pagination
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.Users.AsQueryable();

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
            // Remove PasswordHash from validation since we handle it separately
            ModelState.Remove("PasswordHash");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                ModelState.AddModelError("FullName", "Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address.");
            }

            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("Password", "Password is required.");
            }
            else if (password.Length < 8)
            {
                ModelState.AddModelError("Password", "Password must be at least 8 characters.");
            }
            else
            {
                var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(password, passwordPattern))
                {
                    ModelState.AddModelError("Password", "Password must contain uppercase, lowercase, number, and special character (@$!%*?&).");
                }
            }

            // Check for duplicate email
            if (!string.IsNullOrWhiteSpace(user.Email) && await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            // Validate phone number if provided
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                var phonePattern = @"^(\+?[0-9]{1,4}[-]?[0-9]{2,4}[-]?[0-9]{3,4}[-]?[0-9]{3,4}|[0-9]{7,15})$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(user.PhoneNumber, phonePattern))
                {
                    ModelState.AddModelError("PhoneNumber", "Please enter a valid phone number (e.g., +60-12-345-6789, 012-345-6789, or 0123456789).");
                }
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

            // Remove PasswordHash from validation since we handle it separately
            ModelState.Remove("PasswordHash");

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                ModelState.AddModelError("FullName", "Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address.");
            }

            // Check for duplicate email
            if (!string.IsNullOrWhiteSpace(user.Email) && await _context.Users.AnyAsync(u => u.Email == user.Email && u.UserId != id))
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            // Validate phone number if provided
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                var phonePattern = @"^(\+?[0-9]{1,4}[-]?[0-9]{2,4}[-]?[0-9]{3,4}[-]?[0-9]{3,4}|[0-9]{7,15})$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(user.PhoneNumber, phonePattern))
                {
                    ModelState.AddModelError("PhoneNumber", "Please enter a valid phone number (e.g., +60-12-345-6789, 012-345-6789, or 0123456789).");
                }
            }

            // Validate new password if provided
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword.Length < 8)
                {
                    ModelState.AddModelError("NewPassword", "Password must be at least 8 characters.");
                }
                else
                {
                    var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
                    if (!System.Text.RegularExpressions.Regex.IsMatch(newPassword, passwordPattern))
                    {
                        ModelState.AddModelError("NewPassword", "Password must contain uppercase, lowercase, number, and special character (@$!%*?&).");
                    }
                }
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
            var user = await _context.Users
                .Include(u => u.Bookings)
                .FirstOrDefaultAsync(u => u.UserId == id);
            
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

            // Check if user has bookings
            if (user.Bookings.Any())
            {
                TempData["Error"] = "Cannot delete a user that has bookings. Please cancel or complete all bookings first.";
                return RedirectToAction("Users");
            }

            // Soft delete security logs associated with this user
            var securityLogs = await _context.SecurityLogs
                .Where(sl => sl.UserId == id)
                .IgnoreQueryFilters() // Include deleted logs to avoid issues
                .ToListAsync();
            if (securityLogs.Any())
            {
                foreach (var log in securityLogs.Where(l => !l.IsDeleted))
                {
                    log.IsDeleted = true;
                    log.DeletedAt = DateTime.Now;
                }
            }

            // Soft delete reviews associated with this user (through bookings)
            // Review is now linked to Booking, user info from Booking.UserId
            var reviewsToDelete = await _context.Reviews
                .Include(r => r.Booking)
                .Where(r => r.Booking != null && r.Booking.UserId == id && !r.IsDeleted)
                .ToListAsync();
            if (reviewsToDelete.Any())
            {
                foreach (var review in reviewsToDelete)
                {
                    review.IsDeleted = true;
                    review.DeletedAt = DateTime.Now;
                }
            }

            // Soft delete user
            user.IsDeleted = true;
            user.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction("Users");
        }

        #endregion

        #region Hotel Management

        public async Task<IActionResult> Hotels(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            ValidateSearchParameters(ref searchTerm, ref page, ref pageSize);

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
        public async Task<IActionResult> CreateHotel(Hotel hotel, IFormFile? imageFile, string? imageUrl)
        {
            // Check for duplicate hotel name
            if (await _context.Hotels.AnyAsync(h => h.Name == hotel.Name))
            {
                ModelState.AddModelError("Name", "A hotel with this name already exists.");
            }

            // Check for duplicate address
            if (await _context.Hotels.AnyAsync(h => h.Address == hotel.Address && h.City == hotel.City))
            {
                ModelState.AddModelError("Address", "A hotel at this address already exists.");
            }

            // Validate contact number if provided
            if (!string.IsNullOrWhiteSpace(hotel.ContactNumber))
            {
                var phonePattern = @"^(\+?[0-9]{1,4}[-]?[0-9]{2,4}[-]?[0-9]{3,4}[-]?[0-9]{3,4}|[0-9]{7,15})$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(hotel.ContactNumber, phonePattern))
                {
                    ModelState.AddModelError("ContactNumber", "Please enter a valid phone number (e.g., +60-12-345-6789, 012-345-6789, or 0123456789).");
                }
            }

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "hotels");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    hotel.ImageUrl = $"/uploads/hotels/{uniqueFileName}";
                }
                else if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    hotel.ImageUrl = imageUrl;
                }

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
        public async Task<IActionResult> EditHotel(int id, Hotel hotel, IFormFile? imageFile, string? imageUrl)
        {
            if (id != hotel.HotelId)
            {
                return NotFound();
            }

            // Check for duplicate hotel name (excluding current hotel)
            if (await _context.Hotels.AnyAsync(h => h.Name == hotel.Name && h.HotelId != id))
            {
                ModelState.AddModelError("Name", "A hotel with this name already exists.");
            }

            // Check for duplicate address (excluding current hotel)
            if (await _context.Hotels.AnyAsync(h => h.Address == hotel.Address && h.City == hotel.City && h.HotelId != id))
            {
                ModelState.AddModelError("Address", "A hotel at this address already exists.");
            }

            // Validate contact number if provided
            if (!string.IsNullOrWhiteSpace(hotel.ContactNumber))
            {
                var phonePattern = @"^(\+?[0-9]{1,4}[-]?[0-9]{2,4}[-]?[0-9]{3,4}[-]?[0-9]{3,4}|[0-9]{7,15})$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(hotel.ContactNumber, phonePattern))
                {
                    ModelState.AddModelError("ContactNumber", "Please enter a valid phone number (e.g., +60-12-345-6789, 012-345-6789, or 0123456789).");
                }
            }

            if (ModelState.IsValid)
            {
                var existingHotel = await _context.Hotels.FindAsync(id);
                if (existingHotel == null)
                {
                    return NotFound();
                }

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "hotels");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(existingHotel.ImageUrl) && existingHotel.ImageUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, existingHotel.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    existingHotel.ImageUrl = $"/uploads/hotels/{uniqueFileName}";
                }
                else if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    // Delete old file if exists and is local
                    if (!string.IsNullOrEmpty(existingHotel.ImageUrl) && existingHotel.ImageUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, existingHotel.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    existingHotel.ImageUrl = imageUrl;
                }

                // Update other fields
                existingHotel.Name = hotel.Name;
                existingHotel.Address = hotel.Address;
                existingHotel.City = hotel.City;
                existingHotel.PostalCode = hotel.PostalCode;
                existingHotel.Country = hotel.Country;
                existingHotel.ContactNumber = hotel.ContactNumber;
                existingHotel.ContactEmail = hotel.ContactEmail;
                existingHotel.Description = hotel.Description;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Hotel updated successfully.";
                return RedirectToAction("Hotels");
            }

            return View(hotel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHotelImage(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            // Delete physical file if it's a local upload
            if (!string.IsNullOrEmpty(hotel.ImageUrl) && hotel.ImageUrl.StartsWith("/uploads/"))
            {
                var filePath = Path.Combine(_environment.WebRootPath, hotel.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            hotel.ImageUrl = null;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hotel image deleted successfully.";
            return RedirectToAction("EditHotel", new { id });
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

            // Soft delete hotel
            hotel.IsDeleted = true;
            hotel.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Hotel deleted successfully.";
            return RedirectToAction("Hotels");
        }

        #endregion

        #region Room Type Amenity Management

        [HttpGet]
        public async Task<IActionResult> ManageRoomTypeAmenities(int roomTypeId)
        {
            var roomType = await _context.RoomTypes
                .Include(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);

            if (roomType == null)
            {
                return NotFound();
            }

            var allAmenities = await _context.Amenities
                .OrderBy(a => a.AmenityId)
                .ToListAsync();

            ViewBag.AllAmenities = allAmenities;
            return View(roomType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAmenityToRoomType(int roomTypeId, int amenityId)
        {
            var roomType = await _context.RoomTypes.FindAsync(roomTypeId);
            if (roomType == null)
            {
                return NotFound();
            }

            var amenity = await _context.Amenities.FindAsync(amenityId);
            if (amenity == null)
            {
                return NotFound();
            }

            // Check if already exists
            var exists = await _context.RoomTypeAmenities
                .AnyAsync(rta => rta.RoomTypeId == roomTypeId && rta.AmenityId == amenityId);

            if (!exists)
            {
                var roomTypeAmenity = new RoomTypeAmenity
                {
                    RoomTypeId = roomTypeId,
                    AmenityId = amenityId
                };
                _context.RoomTypeAmenities.Add(roomTypeAmenity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Amenity added successfully.";
            }
            else
            {
                TempData["Error"] = "This amenity is already assigned to this room type.";
            }

            return RedirectToAction("ManageRoomTypeAmenities", new { roomTypeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAmenityFromRoomType(int roomTypeId, int amenityId)
        {
            var roomTypeAmenity = await _context.RoomTypeAmenities
                .FirstOrDefaultAsync(rta => rta.RoomTypeId == roomTypeId && rta.AmenityId == amenityId);

            if (roomTypeAmenity != null)
            {
                _context.RoomTypeAmenities.Remove(roomTypeAmenity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Amenity removed successfully.";
            }

            return RedirectToAction("ManageRoomTypeAmenities", new { roomTypeId });
        }

        #endregion

        #region Room Type Management

        [HttpGet]
        public async Task<IActionResult> CreateRoomType()
        {
            ViewBag.Hotels = await _context.Hotels.OrderBy(h => h.Name).ToListAsync();
            ViewBag.Amenities = await _context.Amenities.OrderBy(a => a.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoomType(RoomType roomType, IFormFile? imageFile, string? imageUrl, int[]? selectedAmenities)
        {
            // Validate room type name
            if (string.IsNullOrWhiteSpace(roomType.Name))
            {
                ModelState.AddModelError("Name", "Room type name is required.");
            }
            else if (await _context.RoomTypes.AnyAsync(rt => rt.Name == roomType.Name && rt.HotelId == roomType.HotelId))
            {
                ModelState.AddModelError("Name", "A room type with this name already exists in this hotel.");
            }

            // Validate price
            if (roomType.BasePrice <= 0)
            {
                ModelState.AddModelError("BasePrice", "Base price must be greater than 0.");
            }

            // Validate occupancy
            if (roomType.Occupancy <= 0)
            {
                ModelState.AddModelError("Occupancy", "Occupancy must be greater than 0.");
            }

            // Validate hotel
            if (roomType.HotelId <= 0)
            {
                ModelState.AddModelError("HotelId", "Please select a hotel.");
            }
            else if (!await _context.Hotels.AnyAsync(h => h.HotelId == roomType.HotelId))
            {
                ModelState.AddModelError("HotelId", "Selected hotel does not exist.");
            }

            if (ModelState.IsValid)
            {
                _context.RoomTypes.Add(roomType);
                await _context.SaveChangesAsync(); // Save first to get RoomTypeId

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "roomtypes");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // Create RoomImage entry
                    var roomImage = new RoomImage
                    {
                        RoomTypeId = roomType.RoomTypeId,
                        ImageUrl = $"/uploads/roomtypes/{uniqueFileName}",
                        Caption = roomType.Name
                    };
                    _context.RoomImages.Add(roomImage);
                }
                else if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    var roomImage = new RoomImage
                    {
                        RoomTypeId = roomType.RoomTypeId,
                        ImageUrl = imageUrl,
                        Caption = roomType.Name
                    };
                    _context.RoomImages.Add(roomImage);
                }

                // Handle amenities
                if (selectedAmenities != null && selectedAmenities.Length > 0)
                {
                    foreach (var amenityId in selectedAmenities)
                    {
                        var roomTypeAmenity = new RoomTypeAmenity
                        {
                            RoomTypeId = roomType.RoomTypeId,
                            AmenityId = amenityId
                        };
                        _context.RoomTypeAmenities.Add(roomTypeAmenity);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Room type created successfully.";
                return RedirectToAction("RoomTypes");
            }

            ViewBag.Hotels = await _context.Hotels.OrderBy(h => h.Name).ToListAsync();
            ViewBag.Amenities = await _context.Amenities.OrderBy(a => a.Name).ToListAsync();
            return View(roomType);
        }

        [HttpGet]
        public async Task<IActionResult> EditRoomType(int id)
        {
            var roomType = await _context.RoomTypes
                .Include(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
                .Include(rt => rt.RoomImages)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == id);

            if (roomType == null)
            {
                return NotFound();
            }

            ViewBag.Hotels = await _context.Hotels.OrderBy(h => h.Name).ToListAsync();
            ViewBag.Amenities = await _context.Amenities.OrderBy(a => a.Name).ToListAsync();
            return View(roomType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoomType(int id, RoomType roomType, IFormFile? imageFile, string? imageUrl, int[]? selectedAmenities)
        {
            if (id != roomType.RoomTypeId)
            {
                return NotFound();
            }

            // Validate room type name
            if (string.IsNullOrWhiteSpace(roomType.Name))
            {
                ModelState.AddModelError("Name", "Room type name is required.");
            }
            else if (await _context.RoomTypes.AnyAsync(rt => rt.Name == roomType.Name && rt.HotelId == roomType.HotelId && rt.RoomTypeId != id))
            {
                ModelState.AddModelError("Name", "A room type with this name already exists in this hotel.");
            }

            // Validate price
            if (roomType.BasePrice <= 0)
            {
                ModelState.AddModelError("BasePrice", "Base price must be greater than 0.");
            }

            // Validate occupancy
            if (roomType.Occupancy <= 0)
            {
                ModelState.AddModelError("Occupancy", "Occupancy must be greater than 0.");
            }

            // Validate hotel
            if (roomType.HotelId <= 0)
            {
                ModelState.AddModelError("HotelId", "Please select a hotel.");
            }
            else if (!await _context.Hotels.AnyAsync(h => h.HotelId == roomType.HotelId))
            {
                ModelState.AddModelError("HotelId", "Selected hotel does not exist.");
            }

            if (ModelState.IsValid)
            {
                var existingRoomType = await _context.RoomTypes
                    .Include(rt => rt.RoomImages)
                    .Include(rt => rt.RoomTypeAmenities)
                    .FirstOrDefaultAsync(rt => rt.RoomTypeId == id);

                if (existingRoomType == null)
                {
                    return NotFound();
                }

                // Update basic properties
                existingRoomType.Name = roomType.Name;
                existingRoomType.Description = roomType.Description;
                existingRoomType.Occupancy = roomType.Occupancy;
                existingRoomType.BasePrice = roomType.BasePrice;
                existingRoomType.HotelId = roomType.HotelId;

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "roomtypes");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // Add new image (keep existing ones)
                    var roomImage = new RoomImage
                    {
                        RoomTypeId = existingRoomType.RoomTypeId,
                        ImageUrl = $"/uploads/roomtypes/{uniqueFileName}",
                        Caption = existingRoomType.Name
                    };
                    existingRoomType.RoomImages.Add(roomImage);
                }
                else if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    // Add new image from URL
                    var roomImage = new RoomImage
                    {
                        RoomTypeId = existingRoomType.RoomTypeId,
                        ImageUrl = imageUrl,
                        Caption = existingRoomType.Name
                    };
                    existingRoomType.RoomImages.Add(roomImage);
                }

                // Handle amenities - remove all existing and add selected ones
                var existingAmenities = existingRoomType.RoomTypeAmenities.ToList();
                _context.RoomTypeAmenities.RemoveRange(existingAmenities);

                if (selectedAmenities != null && selectedAmenities.Length > 0)
                {
                    foreach (var amenityId in selectedAmenities)
                    {
                        var roomTypeAmenity = new RoomTypeAmenity
                        {
                            RoomTypeId = existingRoomType.RoomTypeId,
                            AmenityId = amenityId
                        };
                        _context.RoomTypeAmenities.Add(roomTypeAmenity);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Room type updated successfully.";
                return RedirectToAction("RoomTypes");
            }

            ViewBag.Hotels = await _context.Hotels.OrderBy(h => h.Name).ToListAsync();
            ViewBag.Amenities = await _context.Amenities.OrderBy(a => a.Name).ToListAsync();
            return View(roomType);
        }

        public async Task<IActionResult> RoomTypes(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            ValidateSearchParameters(ref searchTerm, ref page, ref pageSize);

            var query = _context.RoomTypes
                .Include(rt => rt.Hotel)
                .Include(rt => rt.RoomImages)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(rt => rt.Name.Contains(searchTerm) || rt.Hotel.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var roomTypes = await query
                .OrderBy(rt => rt.RoomTypeId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;

            return View(roomTypes);
        }

        #endregion

        #region Room Management

        public async Task<IActionResult> Rooms(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            ValidateSearchParameters(ref searchTerm, ref page, ref pageSize);
            
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
            // Trim room number to avoid whitespace issues
            if (!string.IsNullOrWhiteSpace(room.RoomNumber))
            {
                room.RoomNumber = room.RoomNumber.Trim();
            }

            // Check for duplicate room number
            if (!string.IsNullOrWhiteSpace(room.RoomNumber) && await _context.Rooms.AnyAsync(r => r.RoomNumber == room.RoomNumber))
            {
                ModelState.AddModelError("RoomNumber", "A room with this room number already exists.");
            }

            // Validate RoomTypeId is selected
            if (room.RoomTypeId <= 0)
            {
                ModelState.AddModelError("RoomTypeId", "Please select a room type.");
            }

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

            // Trim room number to avoid whitespace issues
            if (!string.IsNullOrWhiteSpace(room.RoomNumber))
            {
                room.RoomNumber = room.RoomNumber.Trim();
            }

            // Check for duplicate room number (excluding current room)
            if (!string.IsNullOrWhiteSpace(room.RoomNumber) && await _context.Rooms.AnyAsync(r => r.RoomNumber == room.RoomNumber && r.RoomId != id))
            {
                ModelState.AddModelError("RoomNumber", "A room with this room number already exists.");
            }

            // Validate RoomTypeId is selected
            if (room.RoomTypeId <= 0)
            {
                ModelState.AddModelError("RoomTypeId", "Please select a room type.");
            }

            if (ModelState.IsValid)
            {
                var existingRoom = await _context.Rooms.FindAsync(id);
                if (existingRoom == null)
                {
                    return NotFound();
                }

                existingRoom.RoomNumber = room.RoomNumber;
                existingRoom.RoomTypeId = room.RoomTypeId;
                existingRoom.Status = room.Status;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Room updated successfully.";
                return RedirectToAction("Rooms");
            }

            ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoomType(int id)
        {
            var roomType = await _context.RoomTypes
                .Include(rt => rt.Rooms)
                .Include(rt => rt.RoomImages)
                .Include(rt => rt.RoomTypeAmenities)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == id);

            if (roomType == null)
            {
                return NotFound();
            }

            // Check if room type has associated rooms
            if (roomType.Rooms.Any())
            {
                TempData["Error"] = "Cannot delete a room type that has associated rooms. Please delete all rooms first.";
                return RedirectToAction("RoomTypes");
            }

            // Delete all room images (physical files)
            foreach (var image in roomType.RoomImages)
            {
                if (image.ImageUrl.StartsWith("/uploads/"))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, image.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }

            // Soft delete room images
            foreach (var image in roomType.RoomImages.Where(img => !img.IsDeleted))
            {
                image.IsDeleted = true;
                image.DeletedAt = DateTime.Now;
            }
            
            // Soft delete room type
            roomType.IsDeleted = true;
            roomType.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Room type deleted successfully.";
            return RedirectToAction("RoomTypes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Bookings)
                .FirstOrDefaultAsync(r => r.RoomId == id);
            
            if (room == null)
            {
                return NotFound();
            }

            // Check if room has any bookings
            if (room.Bookings.Any())
            {
                TempData["Error"] = "Cannot delete a room that has associated bookings. Please cancel or complete all bookings first.";
                return RedirectToAction("Rooms");
            }

            // Soft delete room
            room.IsDeleted = true;
            room.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Room deleted successfully.";
            return RedirectToAction("Rooms");
        }

        #endregion

        #region Booking Management

        public async Task<IActionResult> Bookings(string searchTerm = "", BookingStatus? status = null, int page = 1, int pageSize = 10)
        {
            ValidateSearchParameters(ref searchTerm, ref page, ref pageSize);

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
                .OrderBy(b => b.BookingId)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Soft delete associated reviews
            var reviewsToDelete = booking.Reviews.Where(r => !r.IsDeleted).ToList();
            if (reviewsToDelete.Any())
            {
                foreach (var review in reviewsToDelete)
                {
                    review.IsDeleted = true;
                    review.DeletedAt = DateTime.Now;
                }
            }

            // Soft delete booking
            booking.IsDeleted = true;
            booking.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Booking deleted successfully.";
            return RedirectToAction("Bookings");
        }

        [HttpGet]
        public async Task<IActionResult> BookingDetails(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(b => b.Promotion)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> ContactMessages(int page = 1, int pageSize = 10)
        {
            var query = _context.ContactMessages.OrderBy(m => m.MessageId).AsQueryable();

            var totalCount = await query.CountAsync();
            var messages = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.UnreadCount = await _context.ContactMessages.CountAsync(m => !m.IsRead);

            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Message marked as read.";
            }
            return RedirectToAction("ContactMessages");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteContactMessage(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                // Soft delete message
                message.IsDeleted = true;
                message.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Message deleted successfully.";
            }
            return RedirectToAction("ContactMessages");
        }

        public async Task<IActionResult> ExportBookingsToCsv(string searchTerm = "", BookingStatus? status = null)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
                .AsQueryable();

            // Apply same filters as the Bookings action
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => b.User.Email.Contains(searchTerm) || b.User.FullName.Contains(searchTerm));
            }

            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }

            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var builder = new System.Text.StringBuilder();
            
            // Add BOM for UTF-8 to help Excel recognize the encoding
            builder.Append('\uFEFF');
            
            // CSV Header
            builder.AppendLine("BookingId,UserEmail,UserName,RoomNumber,RoomType,CheckIn,CheckOut,TotalPrice,Status,BookingDate,PaymentStatus,PaymentMethod");

            // Helper function to escape CSV values
            Func<string, string> escapeCsv = (value) =>
            {
                if (string.IsNullOrEmpty(value))
                    return string.Empty;
                
                // If value contains comma, quote, or newline, wrap in quotes and escape quotes
                if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                    return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
                return value;
            };

            foreach (var b in bookings)
            {
                var userEmail = b.User?.Email ?? string.Empty;
                var userName = b.User?.FullName ?? string.Empty;
                var roomNumber = b.Room?.RoomNumber ?? string.Empty;
                var roomType = b.Room?.RoomType?.Name ?? string.Empty;
                var paymentStatus = b.PaymentStatus.ToString();
                var paymentMethod = b.PaymentMethod?.ToString() ?? string.Empty;

                // Format dates in a way Excel will recognize (MM/DD/YYYY format)
                var checkInDate = b.CheckInDate.ToString("MM/dd/yyyy");
                var checkOutDate = b.CheckOutDate.ToString("MM/dd/yyyy");
                var bookingDate = b.BookingDate.ToString("MM/dd/yyyy HH:mm:ss");

                builder.AppendLine($"{b.BookingId}," +
                    $"{escapeCsv(userEmail)}," +
                    $"{escapeCsv(userName)}," +
                    $"{escapeCsv(roomNumber)}," +
                    $"{escapeCsv(roomType)}," +
                    $"{checkInDate}," +
                    $"{checkOutDate}," +
                    $"{b.TotalPrice:F2}," +
                    $"{b.Status}," +
                    $"{bookingDate}," +
                    $"{paymentStatus}," +
                    $"{escapeCsv(paymentMethod)}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            return File(bytes, "text/csv; charset=utf-8", $"bookings_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        #endregion

        #region Room Image Management


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoomImage(int imageId)
        {
            var roomImage = await _context.RoomImages.FindAsync(imageId);
            if (roomImage == null)
            {
                return NotFound();
            }

            var roomTypeId = roomImage.RoomTypeId;

            // Delete physical file if it's a local upload
            if (roomImage.ImageUrl.StartsWith("/uploads/"))
            {
                var filePath = Path.Combine(_environment.WebRootPath, roomImage.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Soft delete room image
            roomImage.IsDeleted = true;
            roomImage.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Room image deleted successfully.";
            return RedirectToAction("RoomTypes");
        }

        #endregion

        #region Package Image Management

        [HttpGet]
        public async Task<IActionResult> EditPackageImage(int packageId)
        {
            var package = await _context.Packages.FindAsync(packageId);
            if (package == null)
            {
                return NotFound();
            }

            return View(package);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePackageImage(int packageId, IFormFile? imageFile, string? imageUrl)
        {
            var package = await _context.Packages.FindAsync(packageId);
            if (package == null)
            {
                return NotFound();
            }

            string finalImageUrl = string.Empty;

            // Handle file upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "packages");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Delete old file if it exists and is local
                if (!string.IsNullOrEmpty(package.ImageUrl) && package.ImageUrl.StartsWith("/uploads/"))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, package.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                finalImageUrl = $"/uploads/packages/{uniqueFileName}";
            }
            // Handle URL
            else if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                // Delete old file if it exists and is local
                if (!string.IsNullOrEmpty(package.ImageUrl) && package.ImageUrl.StartsWith("/uploads/"))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, package.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                finalImageUrl = imageUrl;
            }
            else
            {
                TempData["Error"] = "Please provide either an image file or an image URL.";
                return View("EditPackageImage", package);
            }

            package.ImageUrl = finalImageUrl;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Package image updated successfully.";
            return RedirectToAction("EditPackageImage", new { packageId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePackageImage(int id)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package == null)
            {
                return NotFound();
            }

            // Delete physical file if it's a local upload
            if (!string.IsNullOrEmpty(package.ImageUrl) && package.ImageUrl.StartsWith("/uploads/"))
            {
                var filePath = Path.Combine(_environment.WebRootPath, package.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            package.ImageUrl = null;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Package image deleted successfully.";
            return RedirectToAction("EditPackage", new { id });
        }

        #endregion

        #region Package Management

        public async Task<IActionResult> Packages(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            ValidateSearchParameters(ref searchTerm, ref page, ref pageSize);

            var query = _context.Packages.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || (p.Description != null && p.Description.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var packages = await query
                .OrderBy(p => p.PackageId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;

            return View(packages);
        }

        [HttpGet]
        public async Task<IActionResult> CreatePackage()
        {
            ViewBag.RoomTypes = await _context.RoomTypes
                .Include(rt => rt.Hotel)
                .OrderBy(rt => rt.RoomTypeId)
                .ToListAsync();
            ViewBag.Services = await _context.Services
                .OrderBy(s => s.ServiceId)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePackage(Package package, IFormFile? imageFile, string? imageUrl, 
            int? roomTypeId, int? serviceId, int roomQuantity = 1, int serviceQuantity = 1)
        {
            // Validate package name
            if (string.IsNullOrWhiteSpace(package.Name))
            {
                ModelState.AddModelError("Name", "Package name is required.");
            }
            else if (await _context.Packages.AnyAsync(p => p.Name == package.Name))
            {
                ModelState.AddModelError("Name", "A package with this name already exists.");
            }

            // Validate price
            if (package.TotalPrice <= 0)
            {
                ModelState.AddModelError("TotalPrice", "Total price must be greater than 0.");
            }

            // Validate that at least one package item is provided
            if (!roomTypeId.HasValue && !serviceId.HasValue)
            {
                ModelState.AddModelError("", "Please add at least one room type or service to the package.");
            }

            // Validate quantities
            if (roomTypeId.HasValue && roomQuantity < 1)
            {
                ModelState.AddModelError("", "Room quantity must be at least 1.");
            }
            if (serviceId.HasValue && serviceQuantity < 1)
            {
                ModelState.AddModelError("", "Service quantity must be at least 1.");
            }

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "packages");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    package.ImageUrl = $"/uploads/packages/{uniqueFileName}";
                }
                else if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    package.ImageUrl = imageUrl;
                }

                _context.Packages.Add(package);
                await _context.SaveChangesAsync();

                // Add package items
                if (roomTypeId.HasValue)
                {
                    var roomItem = new PackageItem
                    {
                        PackageId = package.PackageId,
                        RoomTypeId = roomTypeId.Value,
                        Quantity = roomQuantity
                    };
                    _context.PackageItems.Add(roomItem);
                }

                if (serviceId.HasValue)
                {
                    var serviceItem = new PackageItem
                    {
                        PackageId = package.PackageId,
                        ServiceId = serviceId.Value,
                        Quantity = serviceQuantity
                    };
                    _context.PackageItems.Add(serviceItem);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Package created successfully.";
                return RedirectToAction("Packages");
            }

            ViewBag.RoomTypes = await _context.RoomTypes
                .Include(rt => rt.Hotel)
                .OrderBy(rt => rt.RoomTypeId)
                .ToListAsync();
            ViewBag.Services = await _context.Services
                .OrderBy(s => s.ServiceId)
                .ToListAsync();
            return View(package);
        }

        [HttpGet]
        public async Task<IActionResult> EditPackage(int id)
        {
            var package = await _context.Packages
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.RoomType)
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.Service)
                .FirstOrDefaultAsync(p => p.PackageId == id);

            if (package == null)
            {
                return NotFound();
            }

            ViewBag.RoomTypes = await _context.RoomTypes
                .Include(rt => rt.Hotel)
                .OrderBy(rt => rt.RoomTypeId)
                .ToListAsync();
            ViewBag.Services = await _context.Services
                .OrderBy(s => s.ServiceId)
                .ToListAsync();
            ViewBag.PackageItems = package.PackageItems.ToList();

            return View(package);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPackage(int id, Package package, IFormFile? imageFile, string? imageUrl)
        {
            if (id != package.PackageId)
            {
                return NotFound();
            }

            // Validate package name
            if (string.IsNullOrWhiteSpace(package.Name))
            {
                ModelState.AddModelError("Name", "Package name is required.");
            }
            else if (await _context.Packages.AnyAsync(p => p.Name == package.Name && p.PackageId != id))
            {
                ModelState.AddModelError("Name", "A package with this name already exists.");
            }

            // Validate price
            if (package.TotalPrice <= 0)
            {
                ModelState.AddModelError("TotalPrice", "Total price must be greater than 0.");
            }

            if (ModelState.IsValid)
            {
                var existingPackage = await _context.Packages.FindAsync(id);
                if (existingPackage == null)
                {
                    return NotFound();
                }

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "packages");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Delete old file if it exists and is local
                    if (!string.IsNullOrEmpty(existingPackage.ImageUrl) && existingPackage.ImageUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, existingPackage.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    existingPackage.ImageUrl = $"/uploads/packages/{uniqueFileName}";
                }
                else if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    // Delete old file if it exists and is local, and new one is a URL
                    if (!string.IsNullOrEmpty(existingPackage.ImageUrl) && existingPackage.ImageUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, existingPackage.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    existingPackage.ImageUrl = imageUrl;
                }
                // If neither imageFile nor imageUrl is provided, keep the existing image
                // (existingPackage.ImageUrl remains unchanged)

                existingPackage.Name = package.Name;
                existingPackage.Description = package.Description;
                existingPackage.TotalPrice = package.TotalPrice;
                existingPackage.IsActive = package.IsActive;

                // Handle removed items
                var removedItemsJson = Request.Form["removedItems"].ToString();
                if (!string.IsNullOrEmpty(removedItemsJson))
                {
                    var removedItemIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(removedItemsJson);
                    if (removedItemIds != null && removedItemIds.Any())
                    {
                        var itemsToRemove = await _context.PackageItems
                            .Where(pi => pi.PackageId == id && removedItemIds.Contains(pi.PackageItemId))
                            .ToListAsync();
                        _context.PackageItems.RemoveRange(itemsToRemove);
                    }
                }

                // Handle added items
                var addedItemsJson = Request.Form["addedItems"].ToString();
                if (!string.IsNullOrEmpty(addedItemsJson))
                {
                    using (var doc = System.Text.Json.JsonDocument.Parse(addedItemsJson))
                    {
                        foreach (var item in doc.RootElement.EnumerateArray())
                        {
                            var itemType = item.GetProperty("type").GetString();
                            var itemId = item.GetProperty("itemId").GetInt32();
                            var quantity = item.GetProperty("quantity").GetInt32();

                            if (itemType == "roomType")
                            {
                                var packageItem = new PackageItem
                                {
                                    PackageId = id,
                                    RoomTypeId = itemId,
                                    ServiceId = null,
                                    Quantity = quantity
                                };
                                _context.PackageItems.Add(packageItem);
                            }
                            else if (itemType == "service")
                            {
                                var packageItem = new PackageItem
                                {
                                    PackageId = id,
                                    RoomTypeId = null,
                                    ServiceId = itemId,
                                    Quantity = quantity
                                };
                                _context.PackageItems.Add(packageItem);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Package updated successfully.";
                return RedirectToAction("Packages");
            }

            ViewBag.RoomTypes = await _context.RoomTypes
                .Include(rt => rt.Hotel)
                .OrderBy(rt => rt.RoomTypeId)
                .ToListAsync();
            ViewBag.Services = await _context.Services
                .OrderBy(s => s.ServiceId)
                .ToListAsync();
            // Reload ViewBag data for the view
            ViewBag.RoomTypes = await _context.RoomTypes
                .Include(rt => rt.Hotel)
                .OrderBy(rt => rt.RoomTypeId)
                .ToListAsync();
            ViewBag.Services = await _context.Services
                .OrderBy(s => s.ServiceId)
                .ToListAsync();
            
            var packageWithItems = await _context.Packages
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.RoomType)
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.Service)
                .FirstOrDefaultAsync(p => p.PackageId == id);
            ViewBag.PackageItems = packageWithItems?.PackageItems.ToList() ?? new List<PackageItem>();

            return View(package);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var package = await _context.Packages
                .Include(p => p.PackageItems)
                .FirstOrDefaultAsync(p => p.PackageId == id);

            if (package == null)
            {
                return NotFound();
            }

            // Soft delete package items
            foreach (var item in package.PackageItems.Where(pi => !pi.IsDeleted))
            {
                item.IsDeleted = true;
                item.DeletedAt = DateTime.Now;
            }

            // Delete image file if local
            if (!string.IsNullOrEmpty(package.ImageUrl) && package.ImageUrl.StartsWith("/uploads/"))
            {
                var filePath = Path.Combine(_environment.WebRootPath, package.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Soft delete package
            package.IsDeleted = true;
            package.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Package deleted successfully.";
            return RedirectToAction("Packages");
        }

        #endregion

        #region Review Management

        public async Task<IActionResult> Reviews(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            ValidateSearchParameters(ref searchTerm, ref page, ref pageSize);

            // Review is linked to Booking, user info from Booking.User
            var query = _context.Reviews
                .Include(r => r.Booking)
                    .ThenInclude(b => b.User)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Room)
                        .ThenInclude(r => r.RoomType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r => (r.Booking != null && r.Booking.User != null && r.Booking.User.FullName.Contains(searchTerm)) || 
                                        (r.Booking != null && r.Booking.User != null && r.Booking.User.Email.Contains(searchTerm)) ||
                                        (r.Comment != null && r.Comment.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var reviews = await query
                .OrderBy(r => r.ReviewId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;

            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            // Soft delete review
            review.IsDeleted = true;
            review.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Review deleted successfully.";
            return RedirectToAction("Reviews");
        }

        #endregion

        #region Amenity Management

        public async Task<IActionResult> Amenities(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            ValidateSearchParameters(ref searchTerm, ref page, ref pageSize);

            var query = _context.Amenities.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a => a.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var amenities = await query
                .OrderBy(a => a.AmenityId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;

            return View(amenities);
        }

        [HttpGet]
        public IActionResult CreateAmenity()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAmenity(Amenity amenity, IFormFile? imageFile, string? imageUrl)
        {
            // Validate amenity name
            if (string.IsNullOrWhiteSpace(amenity.Name))
            {
                ModelState.AddModelError("Name", "Amenity name is required.");
            }
            else if (await _context.Amenities.AnyAsync(a => a.Name == amenity.Name))
        {
                ModelState.AddModelError("Name", "An amenity with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "amenities");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    amenity.ImageUrl = $"/uploads/amenities/{uniqueFileName}";
                }
                else if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    amenity.ImageUrl = imageUrl;
                }

                _context.Amenities.Add(amenity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Amenity created successfully.";
                return RedirectToAction("Amenities");
            }

            return View(amenity);
        }

        [HttpGet]
        public async Task<IActionResult> EditAmenity(int id)
        {
            var amenity = await _context.Amenities.FindAsync(id);
            if (amenity == null)
            {
                return NotFound();
            }

            return View(amenity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAmenity(int id, Amenity amenity, IFormFile? imageFile, string? imageUrl)
        {
            if (id != amenity.AmenityId)
            {
                return NotFound();
            }

            // Validate amenity name
            if (string.IsNullOrWhiteSpace(amenity.Name))
            {
                ModelState.AddModelError("Name", "Amenity name is required.");
            }
            else if (await _context.Amenities.AnyAsync(a => a.Name == amenity.Name && a.AmenityId != id))
            {
                ModelState.AddModelError("Name", "An amenity with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                var existingAmenity = await _context.Amenities.FindAsync(id);
                if (existingAmenity == null)
                {
                    return NotFound();
                }

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "amenities");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Delete old file if it exists and is local
                    if (!string.IsNullOrEmpty(existingAmenity.ImageUrl) && existingAmenity.ImageUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, existingAmenity.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    amenity.ImageUrl = $"/uploads/amenities/{uniqueFileName}";
                }
                else if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    // Delete old file if it exists and is local, and new one is a URL
                    if (!string.IsNullOrEmpty(existingAmenity.ImageUrl) && existingAmenity.ImageUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, existingAmenity.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    amenity.ImageUrl = imageUrl;
                }
                else
                {
                    // Keep existing image if no new image provided
                    amenity.ImageUrl = existingAmenity.ImageUrl;
                }

                // Update other properties
                existingAmenity.Name = amenity.Name;
                existingAmenity.ImageUrl = amenity.ImageUrl;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Amenity updated successfully.";
                return RedirectToAction("Amenities");
            }

            return View(amenity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAmenity(int id)
        {
            var amenity = await _context.Amenities.FindAsync(id);
            if (amenity == null)
            {
                return NotFound();
            }

            // Soft delete amenity
            amenity.IsDeleted = true;
            amenity.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Amenity deleted successfully.";
            return RedirectToAction("Amenities");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAmenityImage(int id)
        {
            var amenity = await _context.Amenities.FindAsync(id);
            if (amenity == null)
            {
                return NotFound();
            }

            // Delete physical file if it's a local upload
            if (!string.IsNullOrEmpty(amenity.ImageUrl) && amenity.ImageUrl.StartsWith("/uploads/"))
            {
                var filePath = Path.Combine(_environment.WebRootPath, amenity.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            amenity.ImageUrl = null;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Amenity image deleted successfully.";
            return RedirectToAction("EditAmenity", new { id });
        }

        #endregion

        #region Promotion Management

        public async Task<IActionResult> Promotions(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            ValidateSearchParameters(ref searchTerm, ref page, ref pageSize);

            // Clean up invalid promotions before displaying
            var promotionValidation = HttpContext.RequestServices.GetRequiredService<Assignment.Services.PromotionValidationService>();
            var deactivatedCount = await promotionValidation.DeactivateInvalidPromotionsAsync();
            if (deactivatedCount > 0)
            {
                TempData["Info"] = $"{deactivatedCount} promotion(s) were automatically deactivated (expired or reached max usage).";
            }

            var query = _context.Promotions.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Code.Contains(searchTerm) || (p.Description != null && p.Description.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var promotions = await query
                .OrderBy(p => p.PromotionId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calculate usage counts for each promotion and store in dictionary
            var usageCountsDict = new Dictionary<int, int>();
            foreach (var promotion in promotions)
            {
                var usageCount = await _context.Bookings
                    .CountAsync(b => b.PromotionId == promotion.PromotionId && b.PromotionUsedAt != null);
                usageCountsDict[promotion.PromotionId] = usageCount;
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.UsageCountsDict = usageCountsDict;

            return View(promotions);
        }

        [HttpGet]
        public IActionResult CreatePromotion()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePromotion(Promotion promotion)
        {
            if (promotion.StartDate >= promotion.EndDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
            }

            if (await _context.Promotions.AnyAsync(p => p.Code == promotion.Code))
            {
                ModelState.AddModelError("Code", "Promotion code already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Promotion created successfully.";
                return RedirectToAction("Promotions");
            }

            return View(promotion);
        }

        [HttpGet]
        public async Task<IActionResult> EditPromotion(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPromotion(int id, Promotion promotion)
        {
            if (id != promotion.PromotionId)
            {
                return NotFound();
            }

            if (promotion.StartDate >= promotion.EndDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
            }

            if (await _context.Promotions.AnyAsync(p => p.Code == promotion.Code && p.PromotionId != id))
            {
                ModelState.AddModelError("Code", "Promotion code already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(promotion);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Promotion updated successfully.";
                return RedirectToAction("Promotions");
            }

            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            // Check if there are any bookings using this promotion
            var bookingsUsingPromotion = await _context.Bookings
                .Where(b => b.PromotionId == id)
                .CountAsync();

            if (bookingsUsingPromotion > 0)
            {
                TempData["Error"] = $"Cannot delete this promotion. It is currently being used by {bookingsUsingPromotion} booking(s). To preserve receipt accuracy, promotions that have been used in bookings cannot be deleted. You can deactivate it instead by editing the promotion.";
                return RedirectToAction("Promotions");
            }

            // Check if there are any promotion usages (stored in Bookings table)
            var promotionUsagesCount = await _context.Bookings
                .Where(b => b.PromotionId == id && b.PromotionUsedAt != null)
                .CountAsync();
            
            if (promotionUsagesCount > 0)
            {
                TempData["Error"] = $"Cannot delete this promotion. It has been used {promotionUsagesCount} time(s). To preserve receipt accuracy, promotions that have been used cannot be deleted. You can deactivate it instead by editing the promotion.";
                return RedirectToAction("Promotions");
            }

            // Soft delete promotion (safe to soft delete even if used)
            promotion.IsDeleted = true;
            promotion.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Promotion deleted successfully.";
            return RedirectToAction("Promotions");
        }

        #endregion

        #region Service Management

        public async Task<IActionResult> Services(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            ValidateSearchParameters(ref searchTerm, ref page, ref pageSize);

            var query = _context.Services.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.Name.Contains(searchTerm) || (s.Description != null && s.Description.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var services = await query
                .OrderBy(s => s.ServiceId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;

            return View(services);
        }

        [HttpGet]
        public IActionResult CreateService()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(Service service)
        {
            // Validate service name
            if (string.IsNullOrWhiteSpace(service.Name))
            {
                ModelState.AddModelError("Name", "Service name is required.");
            }
            else if (await _context.Services.AnyAsync(s => s.Name == service.Name))
            {
                ModelState.AddModelError("Name", "A service with this name already exists.");
            }

            // Validate price
            if (service.Price <= 0)
            {
                ModelState.AddModelError("Price", "Price must be greater than 0.");
            }

            if (ModelState.IsValid)
            {
                _context.Services.Add(service);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Service created successfully.";
                return RedirectToAction("Services");
            }

            return View(service);
        }

        [HttpGet]
        public async Task<IActionResult> EditService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(int id, Service service)
        {
            if (id != service.ServiceId)
            {
                return NotFound();
            }

            // Validate service name
            if (string.IsNullOrWhiteSpace(service.Name))
            {
                ModelState.AddModelError("Name", "Service name is required.");
            }
            else if (await _context.Services.AnyAsync(s => s.Name == service.Name && s.ServiceId != id))
            {
                ModelState.AddModelError("Name", "A service with this name already exists.");
            }

            // Validate price
            if (service.Price <= 0)
            {
                ModelState.AddModelError("Price", "Price must be greater than 0.");
            }

            if (ModelState.IsValid)
            {
                var existingService = await _context.Services.FindAsync(id);
                if (existingService == null)
                {
                    return NotFound();
                }

                existingService.Name = service.Name;
                existingService.Description = service.Description;
                existingService.Price = service.Price;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Service updated successfully.";
                return RedirectToAction("Services");
            }

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            // Check if service is used in any packages
            var packageItemsUsingService = await _context.PackageItems
                .Where(pi => pi.ServiceId == id && !pi.IsDeleted)
                .AnyAsync();

            if (packageItemsUsingService)
            {
                TempData["Error"] = "Cannot delete a service that is used in packages. Please remove it from all packages first.";
                return RedirectToAction("Services");
            }

            // Soft delete service
            service.IsDeleted = true;
            service.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Service deleted successfully.";
            return RedirectToAction("Services");
        }

        #endregion
    }
}

