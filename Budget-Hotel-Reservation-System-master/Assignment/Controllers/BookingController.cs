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

            // ========== FIND AVAILABLE ROOM ==========
            // Find a room that is:
            // 1. Of the correct room type
            // 2. Currently available (not occupied, under maintenance, or being cleaned)
            // 3. Not already booked for the requested dates
            
            var actualRoomTypeId = roomType.RoomTypeId;
            
            // Query for available rooms matching the room type
            var availableRoom = await _context.Rooms
                // Filter by room type and status (must be Available)
                .Where(r => r.RoomTypeId == actualRoomTypeId && r.Status == RoomStatus.Available)
                
                // Check for date conflicts with existing bookings
                // A room is NOT available if there's a booking that:
                // - Is not cancelled, checked out, or no-show (these statuses free up the room)
                // - Has overlapping dates with the requested check-in/check-out dates
                .Where(r => !_context.Bookings.Any(b => b.RoomId == r.RoomId &&
                    // Exclude bookings that don't block the room (cancelled, checked out, no-show)
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.CheckedOut &&
                    b.Status != BookingStatus.NoShow &&
                    // Check for date overlap in three scenarios:
                    // Scenario 1: Existing booking starts before requested check-in and ends after requested check-in
                    // Scenario 2: Existing booking starts before requested check-out and ends after requested check-out
                    // Scenario 3: Existing booking is completely within requested dates
                    ((b.CheckInDate <= checkIn && b.CheckOutDate > checkIn) ||
                     (b.CheckInDate < checkOut && b.CheckOutDate >= checkOut) ||
                     (b.CheckInDate >= checkIn && b.CheckOutDate <= checkOut))))
                
                // Get the first available room (if any)
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

            // ========== PRICE CALCULATION ==========
            // Calculate the number of nights by subtracting check-in from check-out date
            var nights = (checkOut - checkIn).Days;
            
            // Calculate base price: room price per night multiplied by number of nights
            var basePrice = roomType.BasePrice * nights;
            
            // Initialize discount to zero (will be calculated if promotion is applied)
            decimal discount = 0;

            // ========== PROMOTION VALIDATION ==========
            // If package is selected, promotions don't apply (package price is fixed)
            // Only validate promotion if:
            // 1. A promotion ID was provided
            // 2. No package is being booked (packages have fixed prices)
            if (promotionId.HasValue && !packageId.HasValue)
            {
                // ========== GATHER USER INFORMATION FOR PROMOTION VALIDATION ==========
                // Get user from database to access their phone number (needed for abuse prevention)
                var user = await _context.Users.FindAsync(userId.Value);
                
                // Decrypt phone number for validation (phone numbers are encrypted in database)
                var phoneNumber = user?.DecryptedPhoneNumber;
                
                // Get device fingerprint (unique identifier for the user's device/browser)
                // Used to prevent same person using promotion multiple times from different devices
                var deviceFingerprint = GetDeviceFingerprint();
                
                // Get client IP address (used for location-based abuse prevention)
                var ipAddress = GetClientIpAddress();

                // ========== VALIDATE PROMOTION WITH COMPREHENSIVE ABUSE PREVENTION ==========
                // This validation checks:
                // - Promotion is active and within date range
                // - Minimum nights and minimum amount requirements
                // - Maximum total uses across all users
                // - Per-phone-number limit (prevents same person using multiple times)
                // - Per-payment-card limit (prevents same card being used multiple times)
                // - Per-user-account limit (prevents same account using multiple times)
                // - Per-device/IP limit (prevents same device/location using multiple times)
                // Card number is null here because payment hasn't been processed yet
                var (isValid, errorMessage) = await _promotionValidation.ValidatePromotionUsageAsync(
                    promotionId.Value,
                    userId.Value,
                    phoneNumber,
                    null, // Card number not available at booking creation (will be validated at payment)
                    deviceFingerprint,
                    ipAddress,
                    basePrice,
                    nights
                );

                // If promotion validation failed, add error to ModelState
                // This will prevent booking creation and show error message to user
                if (!isValid)
                {
                    ModelState.AddModelError("PromotionId", errorMessage);
                }
                else
                {
                    // ========== CALCULATE DISCOUNT AMOUNT ==========
                    // Promotion is valid, now calculate the discount amount
                    var promotion = await _context.Promotions.FindAsync(promotionId.Value);
                    if (promotion != null)
                    {
                        // Check promotion type: Percentage or Fixed Amount
                        if (promotion.Type == DiscountType.Percentage)
                        {
                            // Percentage discount: Calculate percentage of base price
                            // Example: 10% off RM100 = RM10 discount
                            discount = basePrice * (promotion.Value / 100m);
                            
                            // Safety check: Ensure discount doesn't exceed base price
                            // This prevents negative total prices
                            if (discount > basePrice) discount = basePrice;
                        }
                        else
                        {
                            // Fixed amount discount: Use promotion value directly
                            // Example: RM50 off = RM50 discount
                            discount = promotion.Value;
                            
                            // Safety check: Ensure discount doesn't exceed base price
                            // This prevents negative total prices
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

            // ========== CALCULATE FINAL TOTAL PRICE ==========
            decimal totalPrice;

            // Determine final price based on booking type (package vs regular)
            if (packageId.HasValue)
            {
                // ========== PACKAGE BOOKING PRICING ==========
                // Package bookings have fixed prices that include room + services
                // Promotions do NOT apply to packages (packages already have discounted prices)
                var package = await _context.Packages.FindAsync(packageId.Value);
                if (package != null)
                {
                    // Use the fixed package price (already includes all services)
                    totalPrice = package.TotalPrice;
                    
                    // Clear promotion ID because packages don't accept promotions
                    // This ensures data consistency
                    promotionId = null;
                }
                else
                {
                    // Package not found in database (shouldn't happen, but handle gracefully)
                    // Fall back to regular pricing calculation
                    totalPrice = basePrice - discount;
                }
            }
            else
            {
                // ========== REGULAR BOOKING PRICING ==========
                // Regular room booking: base price minus any discount from promotion
                // If no promotion was applied, discount will be 0, so totalPrice = basePrice
                totalPrice = basePrice - discount;
            }

            // ========== CREATE BOOKING RECORD ==========
            // All validations passed, now create the booking in the database
            try
            {
                // Create new Booking entity with all required information
                var booking = new Booking
                {
                    // Link booking to the user who made it
                    UserId = userId.Value,
                    
                    // Link booking to the available room that was found
                    RoomId = availableRoom.RoomId,
                    
                    // Set check-in and check-out dates from user input
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    
                    // Set total price (calculated above based on booking type)
                    TotalPrice = totalPrice,
                    
                    // Initial status is Pending (waiting for payment)
                    // Status will change to Confirmed after payment is processed
                    Status = BookingStatus.Pending,
                    
                    // Link promotion if one was applied (null for package bookings or no promotion)
                    PromotionId = promotionId, // Will be null for package bookings
                    BookingDate = DateTime.Now
                };

                // Add booking to database context
                _context.Bookings.Add(booking);
                
                // Generate unique QR token for check-in
                // This token is used to verify booking identity when guest checks in via QR code
                // QR code contains this token, which is more secure than using booking ID directly
                booking.QRToken = Guid.NewGuid();
                
                // Save booking to database
                // This creates the booking record and generates the BookingId
                await _context.SaveChangesAsync();

                // Log successful booking creation for audit trail
                _logger.LogInformation("Booking created successfully: BookingId={BookingId}, UserId={UserId}, RoomId={RoomId}",
                    booking.BookingId, userId.Value, availableRoom.RoomId);

                // Redirect to payment page to process payment
                // Booking is created but status is still Pending until payment is completed
                return RedirectToAction("Payment", new { bookingId = booking.BookingId });
            }
            catch (Exception ex)
            {
                // ========== ERROR HANDLING ==========
                // If an error occurs during booking creation, log it and show error to user
                // This could happen due to database errors, concurrency issues, etc.
                _logger.LogError(ex, "Error creating booking for UserId={UserId}, RoomId={RoomId}", userId.Value, availableRoom.RoomId);
                
                // Add error message to ModelState for display to user
                ModelState.AddModelError("", "An error occurred while creating your booking. Please try again.");

                // ========== RESTORE VIEW DATA ==========
                // Restore ViewBag data so user can see the form again and retry
                // This provides a better user experience than showing a blank page
                ViewBag.RoomType = roomType;
                ViewBag.CheckIn = checkIn;
                ViewBag.CheckOut = checkOut;
                
                // Clean up invalid promotions and reload active promotions
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
            // ========== LOAD BOOKING FOR PAYMENT ==========
            // Get the booking that needs payment processing
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            
            // Load booking with all related data needed for payment page
            var booking = await _context.Bookings
                .Include(b => b.User)              // User information
                .Include(b => b.Room)              // Room information
                    .ThenInclude(r => r.RoomType)  // Room type details (price, name, etc.)
                .Include(b => b.Promotion)         // Promotion details (if applied)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            // ========== SECURITY CHECK ==========
            // Verify booking exists and belongs to current user
            // Users can only pay for their own bookings
            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            // ========== BOOKING STATUS CHECK ==========
            // Only allow payment for Pending bookings
            // If booking is already Confirmed, Cancelled, etc., payment has already been processed
            if (booking.Status != BookingStatus.Pending)
            {
                TempData["Error"] = "This booking has already been processed.";
                return RedirectToAction("MyBookings");
            }

            // ========== DETECT PACKAGE BOOKING ==========
            // Determine if this is a package booking by comparing total price with calculated room price
            // Package bookings have fixed prices that include services, so price won't match room price calculation
            var nights = (booking.CheckOutDate - booking.CheckInDate).Days;  // Calculate number of nights
            var calculatedRoomPrice = booking.Room.RoomType.BasePrice * nights;  // Calculate what room price should be
            
            // It's a package booking if:
            // 1. Total price doesn't match calculated room price, AND
            // 2. Either no promotion was applied OR total price is higher than room price (promotions reduce price, packages increase it)
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
            // ========== LOAD BOOKING ==========
            // Get the booking that payment is being processed for
            var userId = AuthenticationHelper.GetUserId(HttpContext);
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == model.BookingId);

            // ========== SECURITY CHECK ==========
            // Verify booking exists and belongs to current user
            // Users can only pay for their own bookings
            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            // ========== BOOKING STATUS CHECK ==========
            // Only allow payment processing for Pending bookings
            // If booking is already Confirmed, payment has already been processed
            if (booking.Status != BookingStatus.Pending)
            {
                TempData["Error"] = "This booking has already been processed.";
                return RedirectToAction("MyBookings");
            }

            // ========== PAYMENT METHOD VALIDATION ==========
            // Validate that user selected a payment method
            if (!model.PaymentMethod.HasValue)
            {
                ModelState.AddModelError("PaymentMethod", "Please select a payment method.");
                
                // Reload booking data for view
                model.Booking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Room)
                        .ThenInclude(r => r.RoomType)
                    .Include(b => b.Promotion)
                    .FirstOrDefaultAsync(b => b.BookingId == model.BookingId);
                return View("Payment", model);
            }

            // ========== CREDIT CARD VALIDATION ==========
            // If payment method is Credit Card, validate all required card fields
            if (model.PaymentMethod.Value == PaymentMethod.CreditCard)
            {
                // Check that all credit card fields are provided
                // Required: Card Number, Cardholder Name, Expiry Month, Expiry Year, CVV
                if (string.IsNullOrEmpty(model.CardNumber) || string.IsNullOrEmpty(model.CardholderName) ||
                    !model.ExpiryMonth.HasValue || !model.ExpiryYear.HasValue || string.IsNullOrEmpty(model.CVV))
                {
                    ModelState.AddModelError("", "Please fill in all credit card details.");
                    
                    // Reload booking data for view
                    model.Booking = await _context.Bookings
                        .Include(b => b.User)
                        .Include(b => b.Room)
                            .ThenInclude(r => r.RoomType)
                        .Include(b => b.Promotion)
                        .FirstOrDefaultAsync(b => b.BookingId == model.BookingId);
                    return View("Payment", model);
                }
            }
            // ========== PAYPAL VALIDATION ==========
            // If payment method is PayPal, validate PayPal email
            else if (model.PaymentMethod.Value == PaymentMethod.PayPal)
            {
                // PayPal requires email address for payment processing
                if (string.IsNullOrEmpty(model.PayPalEmail))
                {
                    ModelState.AddModelError("", "Please enter your PayPal email.");
                    
                    // Reload booking data for view
                    model.Booking = await _context.Bookings
                        .Include(b => b.User)
                        .Include(b => b.Room)
                            .ThenInclude(r => r.RoomType)
                        .Include(b => b.Promotion)
                        .FirstOrDefaultAsync(b => b.BookingId == model.BookingId);
                    return View("Payment", model);
                }
            }
            // ========== BANK TRANSFER VALIDATION ==========
            // If payment method is Bank Transfer, validate all required bank details
            else if (model.PaymentMethod.Value == PaymentMethod.BankTransfer)
            {
                // Check that all bank transfer fields are provided
                // Required: Bank Name, Account Number, Account Holder Name
                if (string.IsNullOrEmpty(model.BankName) || string.IsNullOrEmpty(model.AccountNumber) ||
                    string.IsNullOrEmpty(model.AccountHolderName))
                {
                    ModelState.AddModelError("", "Please fill in all bank transfer details.");
                    
                    // Reload booking data for view
                    model.Booking = await _context.Bookings
                        .Include(b => b.User)
                        .Include(b => b.Room)
                            .ThenInclude(r => r.RoomType)
                        .Include(b => b.Promotion)
                        .FirstOrDefaultAsync(b => b.BookingId == model.BookingId);
                    return View("Payment", model);
                }
            }

            // ========== FINAL VALIDATION CHECK ==========
            // Check if there are any validation errors in ModelState
            // If errors exist, reload booking data and return to payment page
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

            // ========== PROCESS PAYMENT ==========
            // All validations passed, process the payment
            try
            {
                // ========== GENERATE TRANSACTION ID ==========
                // Generate a unique transaction ID for this payment
                // This ID is used for tracking and reference purposes
                // Format varies by payment method (e.g., "CC-20231215-123456" for credit card)
                var transactionId = GenerateTransactionId(model.PaymentMethod.Value);

                // ========== UPDATE BOOKING WITH PAYMENT INFORMATION ==========
                // Payment information is stored directly in the Booking table (not separate Payment table)
                // This simplifies the data model and makes queries easier
                booking.PaymentAmount = booking.TotalPrice;              // Amount paid (equals total price for full payment)
                booking.PaymentMethod = model.PaymentMethod.Value;         // Payment method used (CreditCard, PayPal, BankTransfer)
                booking.PaymentStatus = PaymentStatus.Completed;           // Mark payment as completed
                booking.TransactionId = transactionId;                    // Store transaction ID for reference
                booking.PaymentDate = DateTime.Now;                        // Record when payment was processed
                booking.Status = BookingStatus.Confirmed;                  // Change booking status from Pending to Confirmed
                
                // Save payment information to database
                await _context.SaveChangesAsync();

                // ========== RECORD PROMOTION USAGE ==========
                // If a promotion was used, record the usage for abuse prevention
                // This tracks who used the promotion and prevents multiple uses
                if (booking.PromotionId.HasValue)
                {
                    // Get user information for tracking
                    var bookingUser = await _context.Users.FindAsync(booking.UserId);
                    var phoneNumber = bookingUser?.DecryptedPhoneNumber;  // Decrypt phone number for tracking
                    var cardNumber = model.PaymentMethod.Value == PaymentMethod.CreditCard ? model.CardNumber : null;  // Get card number if credit card
                    var deviceFingerprint = GetDeviceFingerprint();        // Get device identifier
                    var ipAddress = GetClientIpAddress();                 // Get IP address

                    // Record promotion usage with all tracking information
                    // This information is stored in the Booking table for abuse prevention
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

                // ========== SEND CONFIRMATION EMAIL ==========
                // Simulate sending booking confirmation email to user
                // In production, this would use EmailService to send real email
                var user = await _context.Users.FindAsync(booking.UserId);
                if (user != null)
                {
                    // Log email sending (for demonstration - in production, actually send email)
                    _logger.LogInformation("Email sent to {Email} for booking confirmation {BookingId}. Subject: Booking Confirmation - Booking #{BookingId}",
                        user.Email, booking.BookingId, booking.BookingId);
                    
                    // Show message to user about QR code for check-in
                    TempData["EmailSent"] = $"Booking confirmed by {user.FullName}. Show this QR when checking in!";
                }

                // Log successful payment processing
                _logger.LogInformation("Payment processed successfully for booking {BookingId}", booking.BookingId);
                
                // Redirect to booking confirmation page
                // This page shows booking details and QR code for check-in
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

        // ========== HELPER METHODS FOR DEVICE/IP TRACKING ==========
        // These methods are used for promotion abuse prevention
        // They help identify unique devices and locations to prevent same person using promotions multiple times
        
        /// <summary>
        /// Generates a device fingerprint based on browser headers.
        /// Used for promotion abuse prevention (identifying unique devices).
        /// </summary>
        /// <returns>Device fingerprint string or null if generation fails.</returns>
        private string? GetDeviceFingerprint()
        {
            try
            {
                // ========== COLLECT BROWSER INFORMATION ==========
                // Get browser headers that help identify the device/browser
                // These headers are sent by the browser automatically
                var userAgent = Request.Headers["User-Agent"].ToString();        // Browser and OS information
                var acceptLanguage = Request.Headers["Accept-Language"].ToString(); // Preferred languages
                var acceptEncoding = Request.Headers["Accept-Encoding"].ToString(); // Supported encodings

                // ========== CREATE DEVICE FINGERPRINT ==========
                // Combine headers to create a unique identifier for the device
                // In production, use a more sophisticated fingerprinting library
                // This simple method works for basic abuse prevention
                var fingerprint = $"{userAgent}|{acceptLanguage}|{acceptEncoding}";
                var hash = System.Text.Encoding.UTF8.GetBytes(fingerprint);
                return Convert.ToBase64String(hash).Substring(0, Math.Min(50, Convert.ToBase64String(hash).Length));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the client's IP address from request headers.
        /// Handles proxies, load balancers, and direct connections.
        /// Used for promotion abuse prevention (location-based tracking).
        /// </summary>
        /// <returns>Client IP address string or null if retrieval fails.</returns>
        private string? GetClientIpAddress()
        {
            try
            {
                // ========== CHECK FOR FORWARDED IP ==========
                // If application is behind a proxy or load balancer, real IP is in X-Forwarded-For header
                // This is common in production environments (e.g., behind nginx, cloudflare, etc.)
                var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // X-Forwarded-For can contain multiple IPs (client, proxy1, proxy2, ...)
                    // Take the first one (original client IP)
                    var ip = forwardedFor.Split(',')[0].Trim();
                    return ip;
                }

                // ========== CHECK FOR REAL IP HEADER ==========
                // Some proxies use X-Real-IP header instead
                // This is another common way to get the real client IP
                var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp))
                {
                    return realIp;
                }

                // ========== FALLBACK TO DIRECT CONNECTION ==========
                // If no proxy headers, get IP directly from connection
                // This works for direct connections (development, local testing)
                return HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            catch
            {
                // Return null if IP retrieval fails (fail gracefully)
                return null;
            }
        }

        /// <summary>
        /// Generates a unique transaction ID for payment tracking.
        /// Format: [PREFIX]-[DATE]-[RANDOM]
        /// Example: "CC-20231215-123456" for credit card payment.
        /// </summary>
        /// <param name="paymentMethod">The payment method used (determines prefix).</param>
        /// <returns>Unique transaction ID string.</returns>
        private string GenerateTransactionId(PaymentMethod paymentMethod)
        {
            // ========== DETERMINE PREFIX BY PAYMENT METHOD ==========
            // Each payment method gets a unique prefix for easy identification
            // CC = Credit Card, PP = PayPal, BT = Bank Transfer, TXN = Unknown/Default
            var prefix = paymentMethod switch
            {
                PaymentMethod.CreditCard => "CC",      // Credit Card prefix
                PaymentMethod.PayPal => "PP",          // PayPal prefix
                PaymentMethod.BankTransfer => "BT",    // Bank Transfer prefix
                _ => "TXN"                             // Default/Unknown prefix
            };
            // ========== GENERATE TRANSACTION ID ==========
            // Format: [PREFIX]-[DATE]-[RANDOM]
            // Example: "CC-20231215-1234567890"
            // Prefix identifies payment method, date helps with tracking, ticks provide uniqueness
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