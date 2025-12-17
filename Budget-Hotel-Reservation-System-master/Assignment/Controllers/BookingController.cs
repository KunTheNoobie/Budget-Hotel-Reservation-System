using Assignment.Attributes;
using Assignment.Helpers;
using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Services;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using static QRCoder.PayloadGenerator;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace Assignment.Controllers
{
    /// <summary>
    /// Controller for handling hotel room bookings.
    /// Manages booking creation, payment processing, booking history, cancellation,
    /// receipt generation, and QR code generation for booking confirmations.
    /// Requires authentication (Customer, Admin, Manager, or Staff roles).
    /// Note: Only Customers can create bookings; Admin/Manager/Staff can view/manage bookings but cannot create them.
    /// </summary>
    [AuthorizeRole(UserRole.Customer, UserRole.Admin, UserRole.Manager, UserRole.Staff)]
    public class BookingController : Controller
    {
        /// <summary>
        /// Database context for accessing booking and related data.
        /// </summary>
        private readonly HotelDbContext _context;

        /// <summary>
        /// Logger for recording booking events and errors.
        /// </summary>
        private readonly ILogger<BookingController> _logger;

        /// <summary>
        /// Service for validating promotion code usage and preventing abuse.
        /// </summary>
        private readonly PromotionValidationService _promotionValidation;

        /// <summary>
        /// Service for automatically updating booking statuses.
        /// </summary>
        private readonly BookingStatusUpdateService _bookingStatusUpdate;

        /// <summary>
        /// Initializes a new instance of the BookingController.
        /// </summary>
        /// <param name="context">Database context for data access.</param>
        /// <param name="logger">Logger instance for logging.</param>
        /// <param name="promotionValidation">Service for promotion validation.</param>
        /// <param name="bookingStatusUpdate">Service for automatic booking status updates.</param>
        public BookingController(HotelDbContext context, ILogger<BookingController> logger, PromotionValidationService promotionValidation, BookingStatusUpdateService bookingStatusUpdate)
        {
            _context = context;
            _logger = logger;
            _promotionValidation = promotionValidation;
            _bookingStatusUpdate = bookingStatusUpdate;
        }

        /// <summary>
        /// Displays the booking creation page for a specific room type or package.
        /// Allows users to select dates, apply promotion codes, and proceed to payment.
        /// Admin, Manager, and Staff cannot create bookings.
        /// </summary>
        /// <param name="roomTypeId">ID of the room type to book.</param>
        /// <param name="checkIn">Optional check-in date (defaults to tomorrow).</param>
        /// <param name="checkOut">Optional check-out date (defaults to day after tomorrow).</param>
        /// <param name="packageId">Optional package ID if booking a package deal.</param>
        /// <returns>The booking creation page view.</returns>
        [HttpGet]
        public async Task<IActionResult> Create(int roomTypeId, DateTime? checkIn, DateTime? checkOut, int? packageId)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Security");
            }

            // Prevent Admin, Manager, and Staff from creating bookings
            var userRole = AuthenticationHelper.GetUserRole(HttpContext);
            if (userRole == UserRole.Admin || userRole == UserRole.Manager || userRole == UserRole.Staff)
            {
                TempData["Error"] = "Administrators, Managers, and Staff cannot create bookings. Please use a customer account.";
                return RedirectToAction("Index", "Home");
            }

            var roomType = await _context.RoomTypes
                .Include(rt => rt.Rooms)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);

            if (roomType == null)
            {
                return NotFound();
            }

            ViewBag.RoomType = roomType;
            ViewBag.CheckIn = checkIn ?? DateTime.Today.AddDays(1);
            ViewBag.CheckOut = checkOut ?? DateTime.Today.AddDays(2);

            // Clean up invalid promotions before loading
            await _promotionValidation.DeactivateInvalidPromotionsAsync();

            ViewBag.Promotions = await _context.Promotions
                .Where(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now && p.IsActive)
                .ToListAsync();

            if (packageId.HasValue)
            {
                var package = await _context.Packages
                    .Include(p => p.PackageItems)
                        .ThenInclude(pi => pi.Service)
                    .Include(p => p.PackageItems)
                        .ThenInclude(pi => pi.RoomType)
                            .ThenInclude(rt => rt.Hotel)
                    .FirstOrDefaultAsync(p => p.PackageId == packageId.Value);

                if (package != null)
                {
                    // Get the room type from the package, not from the roomTypeId parameter
                    var packageRoomTypeId = package.PackageItems
                        .Where(pi => pi.RoomTypeId.HasValue)
                        .Select(pi => pi.RoomTypeId!.Value)
                        .FirstOrDefault();

                    // If package has a room type, use it instead of the parameter
                    if (packageRoomTypeId > 0)
                    {
                        var packageRoomType = await _context.RoomTypes
                            .Include(rt => rt.Rooms)
                            .FirstOrDefaultAsync(rt => rt.RoomTypeId == packageRoomTypeId);

                        if (packageRoomType != null)
                        {
                            roomType = packageRoomType;
                            ViewBag.RoomType = roomType;
                        }
                    }

                    ViewBag.Package = package;
                    ViewBag.IsPackageBooking = true;
                    ViewBag.PackageServices = package.PackageItems
                        .Where(pi => pi.Service != null && pi.ServiceId.HasValue)
                        .ToList();
                    ViewBag.PackageRoomItems = package.PackageItems
                        .Where(pi => pi.RoomTypeId.HasValue)
                        .ToList();
                }
            }
            else
            {
                // Explicitly set to false for non-package bookings
                ViewBag.IsPackageBooking = false;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int roomTypeId, DateTime checkIn, DateTime checkOut, int? promotionId, int? packageId)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Security");
            }

            // Prevent Admin, Manager, and Staff from creating bookings
            var userRole = AuthenticationHelper.GetUserRole(HttpContext);
            if (userRole == UserRole.Admin || userRole == UserRole.Manager || userRole == UserRole.Staff)
            {
                TempData["Error"] = "Administrators, Managers, and Staff cannot create bookings. Please use a customer account.";
                return RedirectToAction("Index", "Home");
            }

            if (checkIn >= checkOut)
            {
                ModelState.AddModelError("", "Check-out date must be after check-in date.");
            }

            if (checkIn < DateTime.Today)
            {
                ModelState.AddModelError("", "Check-in date cannot be in the past.");
            }

            // If packageId is provided, use the room type from the package instead of the parameter
            RoomType? roomType = null;
            if (packageId.HasValue)
            {
                var package = await _context.Packages
                    .Include(p => p.PackageItems)
                    .FirstOrDefaultAsync(p => p.PackageId == packageId.Value);

                if (package != null)
                {
                    var packageRoomTypeId = package.PackageItems
                        .Where(pi => pi.RoomTypeId.HasValue)
                        .Select(pi => pi.RoomTypeId!.Value)
                        .FirstOrDefault();

                    if (packageRoomTypeId > 0)
                    {
                        roomType = await _context.RoomTypes
                            .Include(rt => rt.Rooms)
                            .FirstOrDefaultAsync(rt => rt.RoomTypeId == packageRoomTypeId);
                    }
                }
            }

            // If no package room type found, use the parameter
            if (roomType == null)
            {
                roomType = await _context.RoomTypes
                    .Include(rt => rt.Rooms)
                    .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);
            }

            if (roomType == null)
            {
                return NotFound();
            }

            // Find available room using the correct room type
            var actualRoomTypeId = roomType.RoomTypeId;
            var availableRoom = await _context.Rooms
                .Where(r => r.RoomTypeId == actualRoomTypeId && r.Status == RoomStatus.Available)
                .Where(r => !_context.Bookings.Any(b => b.RoomId == r.RoomId &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.CheckedOut &&
                    b.Status != BookingStatus.NoShow &&
                    ((b.CheckInDate <= checkIn && b.CheckOutDate > checkIn) ||
                     (b.CheckInDate < checkOut && b.CheckOutDate >= checkOut) ||
                     (b.CheckInDate >= checkIn && b.CheckOutDate <= checkOut))))
                .FirstOrDefaultAsync();

            if (availableRoom == null)
            {
                ModelState.AddModelError("", "No rooms available for the selected dates.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.RoomType = roomType;
                ViewBag.CheckIn = checkIn;
                ViewBag.CheckOut = checkOut;
                // Clean up invalid promotions before loading
                await _promotionValidation.DeactivateInvalidPromotionsAsync();

                ViewBag.Promotions = await _context.Promotions
                    .Where(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now && p.IsActive)
                    .ToListAsync();

                if (packageId.HasValue)
                {
                    var package = await _context.Packages
                        .Include(p => p.PackageItems)
                            .ThenInclude(pi => pi.Service)
                        .Include(p => p.PackageItems)
                            .ThenInclude(pi => pi.RoomType)
                        .FirstOrDefaultAsync(p => p.PackageId == packageId.Value);

                    if (package != null)
                    {
                        // Get the room type from the package
                        var packageRoomTypeId = package.PackageItems
                            .Where(pi => pi.RoomTypeId.HasValue)
                            .Select(pi => pi.RoomTypeId!.Value)
                            .FirstOrDefault();

                        if (packageRoomTypeId > 0)
                        {
                            var packageRoomType = await _context.RoomTypes
                                .Include(rt => rt.Rooms)
                                .FirstOrDefaultAsync(rt => rt.RoomTypeId == packageRoomTypeId);

                            if (packageRoomType != null)
                            {
                                roomType = packageRoomType;
                                ViewBag.RoomType = roomType;
                            }
                        }

                        ViewBag.Package = package;
                        ViewBag.IsPackageBooking = true;
                        ViewBag.PackageServices = package.PackageItems
                            .Where(pi => pi.Service != null && pi.ServiceId.HasValue)
                            .ToList();
                        ViewBag.PackageRoomItems = package.PackageItems
                            .Where(pi => pi.RoomTypeId.HasValue)
                            .ToList();
                    }
                }

                return View();
            }

            // Calculate price
            var nights = (checkOut - checkIn).Days;
            var basePrice = roomType.BasePrice * nights;
            decimal discount = 0;

            // If package is selected, promotions don't apply (package price is fixed)
            if (promotionId.HasValue && !packageId.HasValue)
            {
                // Get user info for validation
                var user = await _context.Users.FindAsync(userId.Value);
                var phoneNumber = user?.DecryptedPhoneNumber;
                var deviceFingerprint = GetDeviceFingerprint();
                var ipAddress = GetClientIpAddress();

                // Validate promotion with abuse prevention
                var (isValid, errorMessage) = await _promotionValidation.ValidatePromotionUsageAsync(
                    promotionId.Value,
                    userId.Value,
                    phoneNumber,
                    null, // Card number not available at booking creation
                    deviceFingerprint,
                    ipAddress,
                    basePrice,
                    nights
                );

                if (!isValid)
                {
                    ModelState.AddModelError("PromotionId", errorMessage);
                }
                else
                {
                    var promotion = await _context.Promotions.FindAsync(promotionId.Value);
                    if (promotion != null)
                    {
                        if (promotion.Type == DiscountType.Percentage)
                        {
                            discount = basePrice * (promotion.Value / 100m);
                            // Ensure discount doesn't exceed base price
                            if (discount > basePrice) discount = basePrice;
                        }
                        else
                        {
                            discount = promotion.Value;
                            // Ensure discount doesn't exceed base price
                            if (discount > basePrice) discount = basePrice;
                        }
                    }
                }
            }
            else if (packageId.HasValue && promotionId.HasValue)
            {
                // Package and promotion both selected - silently remove promotion
                // (UI should prevent this, but handle it gracefully if it happens)
                promotionId = null;
            }

            // Check ModelState again after promotion validation
            if (!ModelState.IsValid)
            {
                ViewBag.RoomType = roomType;
                ViewBag.CheckIn = checkIn;
                ViewBag.CheckOut = checkOut;
                // Clean up invalid promotions before loading
                await _promotionValidation.DeactivateInvalidPromotionsAsync();

                ViewBag.Promotions = await _context.Promotions
                    .Where(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now && p.IsActive)
                    .ToListAsync();

                if (packageId.HasValue)
                {
                    var package = await _context.Packages
                        .Include(p => p.PackageItems)
                            .ThenInclude(pi => pi.Service)
                        .Include(p => p.PackageItems)
                            .ThenInclude(pi => pi.RoomType)
                        .FirstOrDefaultAsync(p => p.PackageId == packageId.Value);

                    if (package != null)
                    {
                        // Get the room type from the package
                        var packageRoomTypeId = package.PackageItems
                            .Where(pi => pi.RoomTypeId.HasValue)
                            .Select(pi => pi.RoomTypeId!.Value)
                            .FirstOrDefault();

                        if (packageRoomTypeId > 0)
                        {
                            var packageRoomType = await _context.RoomTypes
                                .Include(rt => rt.Rooms)
                                .FirstOrDefaultAsync(rt => rt.RoomTypeId == packageRoomTypeId);

                            if (packageRoomType != null)
                            {
                                roomType = packageRoomType;
                                ViewBag.RoomType = roomType;
                            }
                        }

                        ViewBag.Package = package;
                        ViewBag.IsPackageBooking = true;
                        ViewBag.PackageServices = package.PackageItems
                            .Where(pi => pi.Service != null && pi.ServiceId.HasValue)
                            .ToList();
                        ViewBag.PackageRoomItems = package.PackageItems
                            .Where(pi => pi.RoomTypeId.HasValue)
                            .ToList();
                    }
                }

                return View();
            }

            decimal totalPrice;

            if (packageId.HasValue)
            {
                var package = await _context.Packages.FindAsync(packageId.Value);
                if (package != null)
                {
                    // Use package price - promotions don't apply to packages
                    totalPrice = package.TotalPrice;
                    promotionId = null; // Clear promotion for package bookings
                }
                else
                {
                    // Package not found, use regular pricing
                    totalPrice = basePrice - discount;
                }
            }
            else
            {
                // Regular booking with optional promotion
                totalPrice = basePrice - discount;
            }

            // Create booking
            try
            {
                var booking = new Booking
                {
                    UserId = userId.Value,
                    RoomId = availableRoom.RoomId,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    TotalPrice = totalPrice,
                    Status = BookingStatus.Pending,
                    PromotionId = promotionId, // Will be null for package bookings
                    BookingDate = DateTime.Now
                };

                _context.Bookings.Add(booking);
                booking.QRToken = Guid.NewGuid();
                await _context.SaveChangesAsync();

                _logger.LogInformation("Booking created successfully: BookingId={BookingId}, UserId={UserId}, RoomId={RoomId}",
                    booking.BookingId, userId.Value, availableRoom.RoomId);

                return RedirectToAction("Payment", new { bookingId = booking.BookingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking for UserId={UserId}, RoomId={RoomId}", userId.Value, availableRoom.RoomId);
                ModelState.AddModelError("", "An error occurred while creating your booking. Please try again.");

                ViewBag.RoomType = roomType;
                ViewBag.CheckIn = checkIn;
                ViewBag.CheckOut = checkOut;
                await _promotionValidation.DeactivateInvalidPromotionsAsync();
                ViewBag.Promotions = await _context.Promotions
                    .Where(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now && p.IsActive)
                    .ToListAsync();

                if (packageId.HasValue)
                {
                    var package = await _context.Packages
                        .Include(p => p.PackageItems)
                            .ThenInclude(pi => pi.Service)
                        .Include(p => p.PackageItems)
                            .ThenInclude(pi => pi.RoomType)
                        .FirstOrDefaultAsync(p => p.PackageId == packageId.Value);

                    if (package != null)
                    {
                        ViewBag.Package = package;
                        ViewBag.IsPackageBooking = true;
                    }
                }

                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Payment(int bookingId)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(b => b.Promotion)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            if (booking.Status != BookingStatus.Pending)
            {
                TempData["Error"] = "This booking has already been processed.";
                return RedirectToAction("MyBookings");
            }

            // Detect if this is a package booking and load package information
            var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
            var calculatedRoomPrice = booking.Room.RoomType.BasePrice * nights;
            var isPackageBooking = booking.TotalPrice != calculatedRoomPrice &&
                                  (booking.Promotion == null || booking.TotalPrice > calculatedRoomPrice);

            if (isPackageBooking)
            {
                // Find the package that matches this booking
                var package = await _context.Packages
                    .Include(p => p.PackageItems)
                        .ThenInclude(pi => pi.Service)
                    .Include(p => p.PackageItems)
                        .ThenInclude(pi => pi.RoomType)
                    .FirstOrDefaultAsync(p => p.TotalPrice == booking.TotalPrice &&
                                             p.PackageItems.Any(pi => pi.RoomTypeId == booking.Room.RoomTypeId));

                if (package != null)
                {
                    ViewBag.Package = package;
                    ViewBag.PackageServices = package.PackageItems
                        .Where(pi => pi.Service != null && pi.ServiceId.HasValue)
                        .ToList();
                }
            }

            var viewModel = new ViewModels.Booking.PaymentViewModel
            {
                BookingId = bookingId,
                Booking = booking
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(ViewModels.Booking.PaymentViewModel model)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == model.BookingId);

            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            if (booking.Status != BookingStatus.Pending)
            {
                TempData["Error"] = "This booking has already been processed.";
                return RedirectToAction("MyBookings");
            }

            // Validate payment method specific fields
            if (!model.PaymentMethod.HasValue)
            {
                ModelState.AddModelError("PaymentMethod", "Please select a payment method.");
                model.Booking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Room)
                        .ThenInclude(r => r.RoomType)
                    .Include(b => b.Promotion)
                    .FirstOrDefaultAsync(b => b.BookingId == model.BookingId);
                return View("Payment", model);
            }

            if (model.PaymentMethod.Value == PaymentMethod.CreditCard)
            {
                if (string.IsNullOrEmpty(model.CardNumber) || string.IsNullOrEmpty(model.CardholderName) ||
                    !model.ExpiryMonth.HasValue || !model.ExpiryYear.HasValue || string.IsNullOrEmpty(model.CVV))
                {
                    ModelState.AddModelError("", "Please fill in all credit card details.");
                    model.Booking = await _context.Bookings
                        .Include(b => b.User)
                        .Include(b => b.Room)
                            .ThenInclude(r => r.RoomType)
                        .Include(b => b.Promotion)
                        .FirstOrDefaultAsync(b => b.BookingId == model.BookingId);
                    return View("Payment", model);
                }
            }
            else if (model.PaymentMethod.Value == PaymentMethod.PayPal)
            {
                if (string.IsNullOrEmpty(model.PayPalEmail))
                {
                    ModelState.AddModelError("", "Please enter your PayPal email.");
                    model.Booking = await _context.Bookings
                        .Include(b => b.User)
                        .Include(b => b.Room)
                            .ThenInclude(r => r.RoomType)
                        .Include(b => b.Promotion)
                        .FirstOrDefaultAsync(b => b.BookingId == model.BookingId);
                    return View("Payment", model);
                }
            }
            else if (model.PaymentMethod.Value == PaymentMethod.BankTransfer)
            {
                if (string.IsNullOrEmpty(model.BankName) || string.IsNullOrEmpty(model.AccountNumber) ||
                    string.IsNullOrEmpty(model.AccountHolderName))
                {
                    ModelState.AddModelError("", "Please fill in all bank transfer details.");
                    model.Booking = await _context.Bookings
                        .Include(b => b.User)
                        .Include(b => b.Room)
                            .ThenInclude(r => r.RoomType)
                        .Include(b => b.Promotion)
                        .FirstOrDefaultAsync(b => b.BookingId == model.BookingId);
                    return View("Payment", model);
                }
            }

            if (!ModelState.IsValid)
            {
                model.Booking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Room)
                        .ThenInclude(r => r.RoomType)
                    .Include(b => b.Promotion)
                    .FirstOrDefaultAsync(b => b.BookingId == model.BookingId);
                return View("Payment", model);
            }

            try
            {
                // Auto-generate transaction ID
                var transactionId = GenerateTransactionId(model.PaymentMethod.Value);

                // Update booking with payment information (Payment merged into Booking)
                booking.PaymentAmount = booking.TotalPrice;
                booking.PaymentMethod = model.PaymentMethod.Value;
                booking.PaymentStatus = PaymentStatus.Completed;
                booking.TransactionId = transactionId;
                booking.PaymentDate = DateTime.Now;
                booking.Status = BookingStatus.Confirmed;
                await _context.SaveChangesAsync();

                // Record promotion usage after successful payment
                if (booking.PromotionId.HasValue)
                {
                    var bookingUser = await _context.Users.FindAsync(booking.UserId);
                    var phoneNumber = bookingUser?.DecryptedPhoneNumber;
                    var cardNumber = model.PaymentMethod.Value == PaymentMethod.CreditCard ? model.CardNumber : null;
                    var deviceFingerprint = GetDeviceFingerprint();
                    var ipAddress = GetClientIpAddress();

                    await _promotionValidation.RecordPromotionUsageAsync(
                        booking.PromotionId.Value,
                        booking.BookingId,
                        booking.UserId,
                        phoneNumber,
                        cardNumber,
                        deviceFingerprint,
                        ipAddress
                    );
                }

                // Simulate email sending
                var user = await _context.Users.FindAsync(booking.UserId);
                if (user != null)
                {
                    _logger.LogInformation("Email sent to {Email} for booking confirmation {BookingId}. Subject: Booking Confirmation - Booking #{BookingId}",
                        user.Email, booking.BookingId, booking.BookingId);
                    TempData["EmailSent"] = $"Booking confirmed by {user.FullName}. Show this QR when checking in!";
                }

                _logger.LogInformation("Payment processed successfully for booking {BookingId}", booking.BookingId);
                //TempData["Success"] = "Payment processed successfully!";
                return RedirectToAction("BookingConfirmation", new { bookingId = booking.BookingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for booking {BookingId}", booking.BookingId);
                TempData["Error"] = "An error occurred while processing your payment. Please try again or contact support.";
                model.Booking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Room)
                        .ThenInclude(r => r.RoomType)
                    .Include(b => b.Promotion)
                    .FirstOrDefaultAsync(b => b.BookingId == model.BookingId);
                return View("Payment", model);
            }
        }

        // Helper methods for device/IP tracking
        private string? GetDeviceFingerprint()
        {
            try
            {
                var userAgent = Request.Headers["User-Agent"].ToString();
                var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                var acceptEncoding = Request.Headers["Accept-Encoding"].ToString();

                // Create a simple fingerprint (in production, use a more sophisticated method)
                var fingerprint = $"{userAgent}|{acceptLanguage}|{acceptEncoding}";
                var hash = System.Text.Encoding.UTF8.GetBytes(fingerprint);
                return Convert.ToBase64String(hash).Substring(0, Math.Min(50, Convert.ToBase64String(hash).Length));
            }
            catch
            {
                return null;
            }
        }

        private string? GetClientIpAddress()
        {
            try
            {
                // Check for forwarded IP (if behind proxy/load balancer)
                var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    var ip = forwardedFor.Split(',')[0].Trim();
                    return ip;
                }

                // Check for real IP
                var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp))
                {
                    return realIp;
                }

                // Fallback to connection remote IP
                return HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private string GenerateTransactionId(PaymentMethod paymentMethod)
        {
            var prefix = paymentMethod switch
            {
                PaymentMethod.CreditCard => "CC",
                PaymentMethod.PayPal => "PP",
                PaymentMethod.BankTransfer => "BT",
                _ => "TXN"
            };
            return $"{prefix}-{DateTime.Now:yyyyMMdd}-{DateTime.Now.Ticks.ToString().Substring(10)}";
        }

        [HttpGet]
        public async Task<IActionResult> BookingConfirmation(int bookingId)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            var userRole = AuthenticationHelper.GetUserRole(HttpContext);

            // Only customers can view booking confirmation - Admin/Manager/Staff should use Admin panel
            if (userRole != UserRole.Customer)
            {
                TempData["Error"] = "Administrators, Managers, and Staff cannot access customer booking features. Please use the Admin panel to view booking details.";
                return RedirectToAction("Bookings", "Admin");
            }

            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(b => b.Promotion)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            if (booking.QRToken == null)
            {
                booking.QRToken = Guid.NewGuid();
                await _context.SaveChangesAsync();
            }

            return View(booking);
        }

        public async Task<IActionResult> CheckIn(Guid token)
        {
            // 1️⃣ Find the booking by QR token
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.QRToken == token);

            if (booking == null)
            {
                return NotFound("Invalid or expired QR code.");
            }

            //Prevent double check-in
            if (booking.Status == BookingStatus.Confirmed || booking.Status == BookingStatus.Pending)
            {
                booking.Status = BookingStatus.CheckedIn;
                booking.CheckInTime = DateTime.Now; // Record the actual check-in time

                await _context.SaveChangesAsync();
                return View("CheckInSuccess", booking);
            }
            else if (booking.Status == BookingStatus.CheckedIn)
            {
                if (booking.CheckInTime.HasValue && DateTime.Now.Date > booking.CheckInTime.Value.Date)
                {
                    booking.Status = BookingStatus.CheckedOut;
                    await _context.SaveChangesAsync();
                    return View("CheckOutSuccess", booking);

                }
                else
                {
                    return View("AlreadyCheckedIn", booking);
                }
            }
            return View("BookingStatusError", booking);
        }

        [HttpPost]
        [Route("Admin/Bookings/QrCheckIn")]
        public async Task<IActionResult> QrCheckIn([FromBody] string scannedData)
        {
            try
            {
                int bookingId = 0;

                // -----------------------------------------------------------
                // 1️⃣ PARSING LOGIC: Extract ID from the complex QR string
                // -----------------------------------------------------------
                if (string.IsNullOrEmpty(scannedData))
                {
                    return Json(new { success = false, message = "Empty QR data." });
                }

                // The QR format from your code is: "BookingID:14|User:ahmad@example.com|..."
                if (scannedData.Contains("BookingID:"))
                {
                    try
                    {
                        var parts = scannedData.Split('|'); // Split by pipe
                        var idPart = parts[0].Split(':')[1]; // Get the number after BookingID:
                        int.TryParse(idPart, out bookingId);
                    }
                    catch
                    {
                        return Json(new { success = false, message = "Error parsing QR format." });
                    }
                }
                else
                {
                    // Fallback for simple number QRs (e.g. "14")
                    int.TryParse(scannedData, out bookingId);
                }

                if (bookingId == 0)
                {
                    return Json(new { success = false, message = "Could not extract a valid Booking ID from the QR." });
                }

                // -----------------------------------------------------------
                // 2️⃣ FIND BOOKING
                // -----------------------------------------------------------
                var booking = await _context.Bookings.FindAsync(bookingId);

                if (booking == null)
                {
                    return Json(new { success = false, message = $"Booking #{bookingId} not found." });
                }

                // -----------------------------------------------------------
                // 3️⃣ CHECK-IN LOGIC (First Scan)
                // -----------------------------------------------------------
                if (booking.Status == BookingStatus.Confirmed || booking.Status == BookingStatus.Pending)
                {
                    booking.Status = BookingStatus.CheckedIn;

                    // ✅ RECORDS CURRENT TIME
                    booking.CheckInTime = DateTime.Now;

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Check-In Successful!" });
                }

                // -----------------------------------------------------------
                // 4️⃣ CHECK-OUT LOGIC (Second Scan)
                // -----------------------------------------------------------
                else if (booking.Status == BookingStatus.CheckedIn)
                {
                    // Ensure CheckInTime is not null (fallback to date only if missing)
                    if (!booking.CheckInTime.HasValue)
                    {
                        booking.CheckInTime = DateTime.Now.Date;
                    }

                    // ✅ PREVENT SAME-DAY CHECKOUT
                    // This logic ensures they cannot check out the moment after checking in.
                    // They must wait until at least tomorrow.
                    if (DateTime.Now.Date > booking.CheckInTime.Value.Date)
                    {
                        booking.Status = BookingStatus.CheckedOut;

                        // ✅ NEW: Record the Check-Out Time
                        booking.CheckOutTime = DateTime.Now;

                        _context.Update(booking);
                        await _context.SaveChangesAsync();
                        return Json(new { success = true, message = "Check-Out Successful!" });
                    }
                    else
                    {
                        // Warning if trying to check out too early
                        return Json(new { success = false, message = "Guest is already Checked In. Cannot Check-Out on the same day." });
                    }
                }

                // -----------------------------------------------------------
                // 5️⃣ OTHER STATUSES
                // -----------------------------------------------------------
                else if (booking.Status == BookingStatus.CheckedOut)
                {
                    return Json(new { success = false, message = "This booking is already Checked Out." });
                }

                return Json(new { success = false, message = $"Booking status is {booking.Status}. Cannot process." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Security");
            }

            // Only customers can view their bookings - Admin/Manager/Staff should use Admin panel
            var userRole = AuthenticationHelper.GetUserRole(HttpContext);
            if (userRole != UserRole.Customer)
            {
                TempData["Error"] = "Administrators, Managers, and Staff cannot access customer booking features. Please use the Admin panel to manage bookings.";
                return RedirectToAction("Index", "Admin");
            }

            // Automatically update booking statuses (check-in, check-out, no-show)
            try
            {
                await _bookingStatusUpdate.UpdateBookingStatusesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error updating booking statuses automatically");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(b => b.Reviews)
                .Where(b => b.UserId == userId.Value)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            // Check which bookings can be reviewed (Review is linked to Booking, user info from Booking.UserId)
            var bookingReviewStatus = bookings.ToDictionary(
                b => b.BookingId,
                b => new
                {
                    CanReview = (b.Status == BookingStatus.Confirmed ||
                                b.Status == BookingStatus.CheckedOut ||
                                b.Status == BookingStatus.CheckedIn) &&
                               !b.Reviews.Any(),
                    HasReview = b.Reviews.Any()
                }
            );

            ViewBag.BookingReviewStatus = bookingReviewStatus;

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> BookingDetails(int id)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            var userRole = AuthenticationHelper.GetUserRole(HttpContext);

            // Only customers can view booking details - Admin/Manager/Staff should use Admin panel
            if (userRole != UserRole.Customer)
            {
                TempData["Error"] = "Administrators, Managers, and Staff cannot access customer booking features. Please use the Admin panel to view booking details.";
                return RedirectToAction("Bookings", "Admin");
            }

            // Review is linked to Booking, user info from Booking.User
            // Note: Review.Booking is automatically populated by EF, so we don't need to include it
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(b => b.Promotion)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            // Detect if this is a package booking and load package information
            var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
            var calculatedRoomPrice = booking.Room.RoomType.BasePrice * nights;
            var isPackageBooking = booking.TotalPrice != calculatedRoomPrice &&
                                  (booking.Promotion == null || booking.TotalPrice > calculatedRoomPrice);

            if (isPackageBooking)
            {
                // Find the package that matches this booking
                var package = await _context.Packages
                    .Include(p => p.PackageItems)
                        .ThenInclude(pi => pi.Service)
                    .Include(p => p.PackageItems)
                        .ThenInclude(pi => pi.RoomType)
                    .FirstOrDefaultAsync(p => p.TotalPrice == booking.TotalPrice &&
                                             p.PackageItems.Any(pi => pi.RoomTypeId == booking.Room.RoomTypeId));

                if (package != null)
                {
                    ViewBag.Package = package;
                    ViewBag.PackageServices = package.PackageItems
                        .Where(pi => pi.Service != null && pi.ServiceId.HasValue)
                        .ToList();
                }
            }

            // Check if user can leave a review (Review is linked to Booking, user info from Booking.UserId)
            var canReview = booking.UserId == userId &&
                           (booking.Status == BookingStatus.Confirmed ||
                            booking.Status == BookingStatus.CheckedOut ||
                            booking.Status == BookingStatus.CheckedIn) &&
                           !booking.Reviews.Any();

            ViewBag.CanReview = canReview;
            ViewBag.HasReview = booking.Reviews.Any();
            ViewBag.ExistingReview = booking.Reviews.FirstOrDefault();

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            var userRole = AuthenticationHelper.GetUserRole(HttpContext);

            // Only customers can cancel bookings - Admin/Manager/Staff should use Admin panel
            if (userRole != UserRole.Customer)
            {
                TempData["Error"] = "Administrators, Managers, and Staff cannot cancel bookings. Please use the Admin panel to manage bookings.";
                return RedirectToAction("Bookings", "Admin");
            }

            var booking = await _context.Bookings
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            // Only Pending bookings can be cancelled
            if (booking.Status != BookingStatus.Pending)
            {
                string errorMessage = booking.Status switch
                {
                    BookingStatus.Cancelled => "This booking is already cancelled.",
                    BookingStatus.Confirmed => "Cannot cancel a confirmed booking. Please contact support for assistance.",
                    BookingStatus.CheckedIn => "Cannot cancel a booking that has already been checked in.",
                    BookingStatus.CheckedOut => "Cannot cancel a booking that has already been checked out.",
                    BookingStatus.NoShow => "Cannot cancel a no-show booking.",
                    _ => "This booking cannot be cancelled."
                };
                TempData["Error"] = errorMessage;
                return RedirectToAction("MyBookings");
            }

            // Check if booking has a review - if reviewed, cannot cancel
            if (booking.Reviews != null && booking.Reviews.Any())
            {
                TempData["Error"] = "Cannot cancel a booking that has been reviewed.";
                return RedirectToAction("MyBookings");
            }

            if (booking.CheckInDate < DateTime.Today)
            {
                TempData["Error"] = "Cannot cancel a booking with a check-in date in the past.";
                return RedirectToAction("MyBookings");
            }

            try
            {
                booking.Status = BookingStatus.Cancelled;

                // Update cancellation details directly on Booking
                booking.CancellationDate = DateTime.Now;
                booking.RefundAmount = booking.PaymentStatus == PaymentStatus.Completed ? booking.TotalPrice * 0.8m : 0; // 80% refund
                booking.CancellationReason = "Cancelled by customer";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Booking cancelled successfully: BookingId={BookingId}, UserId={UserId}", id, userId);
                TempData["Success"] = "Booking cancelled successfully.";
                return RedirectToAction("MyBookings");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking: BookingId={BookingId}, UserId={UserId}", id, userId);
                TempData["Error"] = "An error occurred while cancelling your booking. Please try again or contact support.";
                return RedirectToAction("MyBookings");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewReceipt(int id)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            var userRole = AuthenticationHelper.GetUserRole(HttpContext);

            // Only customers can view receipts - Admin/Manager/Staff should use Admin panel
            if (userRole != UserRole.Customer)
            {
                TempData["Error"] = "Administrators, Managers, and Staff cannot access customer booking features. Please use the Admin panel to view booking receipts.";
                return RedirectToAction("Bookings", "Admin");
            }

            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(b => b.Promotion)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            // Detect if this is a package booking and load package information
            var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
            var calculatedRoomPrice = booking.Room.RoomType.BasePrice * nights;
            var isPackageBooking = booking.TotalPrice != calculatedRoomPrice &&
                                  (booking.Promotion == null || booking.TotalPrice > calculatedRoomPrice);

            if (isPackageBooking)
            {
                // Find the package that matches this booking
                var package = await _context.Packages
                    .Include(p => p.PackageItems)
                        .ThenInclude(pi => pi.Service)
                    .Include(p => p.PackageItems)
                        .ThenInclude(pi => pi.RoomType)
                    .FirstOrDefaultAsync(p => p.TotalPrice == booking.TotalPrice &&
                                             p.PackageItems.Any(pi => pi.RoomTypeId == booking.Room.RoomTypeId));

                if (package != null)
                {
                    ViewBag.Package = package;
                    ViewBag.PackageServices = package.PackageItems
                        .Where(pi => pi.Service != null && pi.ServiceId.HasValue)
                        .ToList();
                }
            }

            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> QRCode(int id)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            var qrCodeData = $"BookingID:{booking.BookingId}|User:{booking.User.Email}|Room:{booking.Room.RoomNumber}|CheckIn:{booking.CheckInDate:yyyy-MM-dd}|CheckOut:{booking.CheckOutDate:yyyy-MM-dd}";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrData = qrGenerator.CreateQrCode(qrCodeData, QRCodeGenerator.ECCLevel.Q);
                using (PngByteQRCode qrCode = new PngByteQRCode(qrData))
                {
                    byte[] qrCodeBytes = qrCode.GetGraphic(20);
                    return File(qrCodeBytes, "image/png");
                }
            }
        }

    }
}