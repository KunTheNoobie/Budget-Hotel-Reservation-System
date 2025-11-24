using Assignment.Attributes;
using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Helpers;
using Assignment.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;

namespace Assignment.Controllers
{
    [AuthorizeRole(UserRole.Customer, UserRole.Admin, UserRole.Manager, UserRole.Staff)]
    public class BookingController : Controller
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<BookingController> _logger;
        private readonly PromotionValidationService _promotionValidation;

        public BookingController(HotelDbContext context, ILogger<BookingController> logger, PromotionValidationService promotionValidation)
        {
            _context = context;
            _logger = logger;
            _promotionValidation = promotionValidation;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int roomTypeId, DateTime? checkIn, DateTime? checkOut, int? packageId)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Security");
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
                        .Select(pi => pi.RoomTypeId.Value)
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
                        .Select(pi => pi.RoomTypeId.Value)
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
                            .Select(pi => pi.RoomTypeId.Value)
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

            if (promotionId.HasValue)
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
                            .Select(pi => pi.RoomTypeId.Value)
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

            var totalPrice = basePrice - discount;

            if (packageId.HasValue)
            {
                var package = await _context.Packages.FindAsync(packageId.Value);
                if (package != null)
                {
                    // Override price with package price
                    totalPrice = package.TotalPrice;
                    // Note: We don't store the package ID in the booking because the schema doesn't support it
                    // This is a price-only integration as per requirements
                }
            }

            // Create booking
            var booking = new Booking
            {
                UserId = userId.Value,
                RoomId = availableRoom.RoomId,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                TotalPrice = totalPrice,
                Status = BookingStatus.Pending,
                PromotionId = promotionId,
                BookingDate = DateTime.Now
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Payment", new { bookingId = booking.BookingId });
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
                var user = await _context.Users.FindAsync(booking.UserId);
                var phoneNumber = user?.DecryptedPhoneNumber;
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

            TempData["Success"] = "Payment processed successfully!";
            return RedirectToAction("BookingConfirmation", new { bookingId = booking.BookingId });
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

            // Generate QR Code
            var qrCodeData = $"BookingID:{booking.BookingId}|User:{booking.User.Email}|Room:{booking.Room.RoomNumber}|CheckIn:{booking.CheckInDate:yyyy-MM-dd}|CheckOut:{booking.CheckOutDate:yyyy-MM-dd}";
            ViewBag.QRCodeData = qrCodeData;

            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Security");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(b => b.Reviews)
                .Where(b => b.UserId == userId.Value)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            // Check which bookings can be reviewed
            var bookingReviewStatus = bookings.ToDictionary(
                b => b.BookingId,
                b => new
                {
                    CanReview = (b.Status == BookingStatus.Confirmed || 
                                b.Status == BookingStatus.CheckedOut || 
                                b.Status == BookingStatus.CheckedIn) &&
                               !b.Reviews.Any(r => r.UserId == userId),
                    HasReview = b.Reviews.Any(r => r.UserId == userId)
                }
            );

            ViewBag.BookingReviewStatus = bookingReviewStatus;

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> BookingDetails(int id)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(b => b.Promotion)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null || (booking.UserId != userId && AuthenticationHelper.GetUserRole(HttpContext) != UserRole.Admin))
            {
                return NotFound();
            }

            // Check if user can leave a review
            var canReview = booking.UserId == userId && 
                           (booking.Status == BookingStatus.Confirmed || 
                            booking.Status == BookingStatus.CheckedOut || 
                            booking.Status == BookingStatus.CheckedIn) &&
                           !booking.Reviews.Any(r => r.UserId == userId);

            ViewBag.CanReview = canReview;
            ViewBag.HasReview = booking.Reviews.Any(r => r.UserId == userId);
            ViewBag.ExistingReview = booking.Reviews.FirstOrDefault(r => r.UserId == userId);

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            if (booking.Status == BookingStatus.Cancelled)
            {
                TempData["Error"] = "This booking is already cancelled.";
                return RedirectToAction("MyBookings");
            }

            if (booking.Status == BookingStatus.CheckedIn || booking.Status == BookingStatus.CheckedOut)
            {
                TempData["Error"] = "Cannot cancel a booking that has already been checked in or checked out.";
                return RedirectToAction("MyBookings");
            }

            if (booking.CheckInDate < DateTime.Today)
            {
                TempData["Error"] = "Cannot cancel a booking with a check-in date in the past.";
                return RedirectToAction("MyBookings");
            }

            booking.Status = BookingStatus.Cancelled;
            
            // Update cancellation details directly on Booking
            booking.CancellationDate = DateTime.Now;
            booking.RefundAmount = booking.PaymentStatus == PaymentStatus.Completed ? booking.TotalPrice * 0.8m : 0; // 80% refund
            booking.CancellationReason = "Cancelled by customer";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking cancelled successfully.";
            return RedirectToAction("MyBookings");
        }

        [HttpGet]
        public async Task<IActionResult> ViewReceipt(int id)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
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
