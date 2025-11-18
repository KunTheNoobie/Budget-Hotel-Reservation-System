using Assignment.Attributes;
using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Helpers;
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

        public BookingController(HotelDbContext context, ILogger<BookingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int roomTypeId, DateTime? checkIn, DateTime? checkOut)
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
            ViewBag.Promotions = await _context.Promotions
                .Where(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now && p.IsActive)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int roomTypeId, DateTime checkIn, DateTime checkOut, int? promotionId)
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

            var roomType = await _context.RoomTypes
                .Include(rt => rt.Rooms)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);

            if (roomType == null)
            {
                return NotFound();
            }

            // Find available room
            var availableRoom = await _context.Rooms
                .Where(r => r.RoomTypeId == roomTypeId && r.Status == RoomStatus.Available)
                .Where(r => !_context.Bookings.Any(b => b.RoomId == r.RoomId &&
                    b.Status != BookingStatus.Cancelled &&
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
                ViewBag.Promotions = await _context.Promotions
                    .Where(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now && p.IsActive)
                    .ToListAsync();
                return View();
            }

            // Calculate price
            var nights = (checkOut - checkIn).Days;
            var basePrice = roomType.BasePrice * nights;
            decimal discount = 0;

            if (promotionId.HasValue)
            {
                var promotion = await _context.Promotions.FindAsync(promotionId.Value);
                if (promotion != null && promotion.IsActive)
                {
                    if (promotion.Type == DiscountType.Percentage)
                    {
                        discount = basePrice * (promotion.Value / 100m);
                    }
                    else
                    {
                        discount = promotion.Value;
                    }
                }
            }

            var totalPrice = basePrice - discount;

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
                .Include(b => b.Payment)
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
            if (model.PaymentMethod == PaymentMethod.CreditCard)
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
            else if (model.PaymentMethod == PaymentMethod.PayPal)
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
            else if (model.PaymentMethod == PaymentMethod.BankTransfer)
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
            var transactionId = GenerateTransactionId(model.PaymentMethod);

            // Create payment
            var payment = new Payment
            {
                BookingId = model.BookingId,
                Amount = booking.TotalPrice,
                PaymentMethod = model.PaymentMethod,
                Status = PaymentStatus.Completed,
                TransactionId = transactionId,
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            booking.Status = BookingStatus.Confirmed;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment processed successfully!";
            return RedirectToAction("BookingConfirmation", new { bookingId = booking.BookingId });
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
                .Include(b => b.Payment)
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
                .Include(b => b.Payment)
                .Where(b => b.UserId == userId.Value)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

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
                .Include(b => b.Payment)
                .Include(b => b.Promotion)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null || (booking.UserId != userId && AuthenticationHelper.GetUserRole(HttpContext) != UserRole.Admin))
            {
                return NotFound();
            }

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            var booking = await _context.Bookings
                .Include(b => b.Payment)
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

            booking.Status = BookingStatus.Cancelled;
            
            // Create cancellation record
            var cancellation = new BookingCancellation
            {
                BookingId = id,
                CancellationDate = DateTime.Now,
                RefundAmount = booking.Payment?.Status == PaymentStatus.Completed ? booking.TotalPrice * 0.8m : 0, // 80% refund
                Reason = "Cancelled by customer"
            };

            _context.BookingCancellations.Add(cancellation);
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
                .Include(b => b.Payment)
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
