BMIT2023 Web and Mobile Systems Assignment
Budget Hotel Reservation System

NOTE: For comprehensive documentation, please refer to README.md in the root directory.
This file (README.txt) contains essential information for assignment submission.

===========================================================================
LOGIN CREDENTIALS
===========================================================================

1. ADMIN ACCOUNT
   Email:    admin@hotel.com
   Password: Admin123!
   Role:     Admin (Full Access)
   Note:     Full system access - can manage all aspects of the system

2. MANAGER ACCOUNTS (10 managers, one per hotel)
   Email:    manager1@hotel.com through manager10@hotel.com
   Password: Manager123! (same for all managers)
   Role:     Manager (Full Access scoped to assigned hotel)
   Note:     Each manager can only see/manage their assigned hotel
             Cannot create hotels or users
             Can create room types, rooms, packages, promotions for their hotel

3. STAFF ACCOUNTS (10 staff, one per hotel)
   Email:    staff1@hotel.com through staff10@hotel.com
   Password: Password123! (same for all staff)
   Role:     Staff (Limited Admin Access scoped to assigned hotel)
   Note:     Each staff can only see/manage their assigned hotel
             Cannot create hotels, users, room types, rooms, packages, or promotions
             Can view and manage bookings for their hotel

4. CUSTOMER ACCOUNTS
   Email:    ahmad@example.com
   Password: Ahmad123!
   Role:     Customer (Email Verified, Has Bookings)
   
   Email:    siti@example.com
   Password: Siti123!
   Role:     Customer (Email Verified, Has Pending Bookings)
   
   Email:    charlie@example.com
   Password: Charlie123!
   Role:     Customer (Email NOT Verified - for testing email verification flow)

===========================================================================
PROJECT SETUP
===========================================================================
1. Open the solution file (Assignment.sln) in Visual Studio 2022.
2. Open "appsettings.json" and ensure:
   - "EncryptionKey" is set (min 32 chars).
   - "SecuritySettings" (MaxLoginAttempts, LockoutMinutes) are configured.
3. Open Package Manager Console.
4. Run "Update-Database" to create the database and seed initial data.
5. Run the application (F5).
   - The system will also seed sample favorites/wishlist data for demo customers.

===========================================================================
MODULE ASSIGNMENTS (Team of 4)
===========================================================================

Each team member is responsible for at least 2 core modules:

TEAM MEMBER 1 (PIC):
1. Security Module
   - User registration with email verification
   - Login/logout with cookie-based authentication
   - Password reset with secure tokens
   - Login attempt tracking and blocking (3 attempts, 15-min lockout)
   - Role-based authorization
   - Rate limiting on registration
   - Math captcha for registration
   - Security logging

2. Customer Module
   - Profile management (view and edit)
   - Profile picture upload with preview
   - Change password
   - View booking history
   - Cancel bookings
   - Favorites/Wishlist feature
   - User preferences (language, theme)

TEAM MEMBER 2 (PIC):
1. Admin Module - User & Hotel Management
   - Admin dashboard with statistics and charts
   - User management (CRUD operations)
   - Hotel management (CRUD operations)
   - Admin account protection
   - Export bookings to CSV

2. Review Module
   - Submit reviews for bookings
   - View reviews on room details
   - Review moderation (admin)
   - Review pagination
   - Average rating calculation

TEAM MEMBER 3 (PIC):
1. Admin Module - Room & Amenity Management
   - Room type management (CRUD operations)
   - Room management (CRUD operations)
   - Amenity management
   - Room-amenity relationship management
   - Multiple images per room type
   - Service management
   - Package management

2. Room Catalog Module
   - Browse available rooms
   - AJAX-enabled search and filtering
   - View room details with amenities
   - Check room availability
   - Room image carousel
   - Pagination

TEAM MEMBER 4 (PIC):
1. Booking Module
   - Create bookings with date selection
   - Apply promotion codes
   - Multiple payment methods (Credit Card, PayPal, Bank Transfer)
   - Booking confirmation with QR code
   - View booking history
   - Download/view e-receipts
   - Cancel bookings
   - Booking status management
   - Email confirmation simulation
   - Package bookings

2. Contact & Home Module
   - Contact form with rate limiting
   - Newsletter subscription
   - Home page with featured rooms
   - Package browsing
   - Public pages (About, Blog, Help Center, etc.)

===========================================================================
FEATURES IMPLEMENTED
===========================================================================
Core Modules:
- Authentication & Authorization (Cookie-based)
- Hotel & Room Management
- Booking System
- Customer Profile Management
- Review System
- Contact System
- Package System

Additional Features:
1. Admin Dashboard with Charts (Chart.js)
2. Print-Friendly Booking Receipts
3. Math Captcha for Registration
4. Export Bookings to CSV
5. Password Reset Simulation
6. Email Verification Simulation
7. Email Confirmation Simulation (Booking)
8. Search & Filter for Hotels/Rooms (AJAX)
9. Responsive "Premium" UI Design
10. Favorites/Wishlist Feature
11. Loading Indicators (AJAX)
12. Toast Notifications
13. Image Preview Before Upload
14. Custom Error Pages (404, 403, 400)
15. Rate Limiting (Registration & Contact Forms)
16. Soft Delete Functionality (All Entities)
17. QR Code Generation (QRCoder)
18. Multiple Images per Room Type
19. Package System with Services
20. Promotion System with Validation
21. QR Code Check-in System (QRToken generation)
22. Automatic Booking Status Updates (check-in, check-out, no-show)
23. Comprehensive Code Documentation (XML comments throughout)
24. Hotel Category System (Budget, MidRange, Luxury)
25. Booking Source Tracking (Direct, OTA, Group, Phone, WalkIn)

===========================================================================
LATEST SCHEMA UPDATES (v3)
===========================================================================
- Booking table now includes: QRToken, CheckInTime, CheckOutTime, TransactionId
- Promotion usage tracking fields merged into Booking table:
  * PromotionPhoneNumberHash (encrypted)
  * PromotionCardIdentifier (hashed)
  * PromotionDeviceFingerprint
  * PromotionIpAddress
  * PromotionUsedAt
- Review table: Removed UserId field (reviews linked only to Booking)
- Hotel table: Added Category field (0=Budget, 1=MidRange, 2=Luxury)
- Booking table: Added Source field (0=Direct, 1=OTA, 2=Group, 3=Phone, 4=WalkIn)
- Users table: Added PreferredLanguage and Theme fields (required NOT NULL)

===========================================================================
SAMPLE DATA (v4 - PRESENTATION READY)
===========================================================================
The SQL script (03_CompleteNewDatabase.sql) now includes:
- 10 Hotels (each with at least 3 room types = 30+ room types total)
- 36 Users: 1 Admin + 10 Managers + 10 Staff + 15 Customers
- 30 Room Types (at least 3 per hotel for comprehensive testing)
- 300 Rooms (10 rooms per room type for availability testing)
- 15 Amenities (Free Wi-Fi, Air Conditioning, TV, etc.)
- 90 Room Images (3 images per room type)
- 12 Services (Airport Transfer, Breakfast, Spa, etc.)
- 10 Packages (with PackageItems linking room types and services)
- 10 Promotions (various discount codes with abuse prevention)
- 30+ Bookings with various statuses (Pending, Confirmed, Cancelled, CheckedIn, CheckedOut, NoShow)
  * Recent bookings (last 7 days) for revenue trend charts
  * Historical bookings (last month) for analytics
  * Future bookings (pending) for testing
  * Cancelled bookings for cancellation flow testing
- 20+ Reviews (linked to checked-out bookings with ratings 1-5)
- 10 Contact Messages (customer inquiries)
- 10 Newsletter Subscriptions (email subscriptions)

Hotel 1 (Budget Inn KL Sentral) has 3 room types:
  * Standard Single Room (RoomTypeId = 1, BasePrice = 79.99)
  * Deluxe Double Room (RoomTypeId = 2, BasePrice = 129.99)
  * Executive Suite (RoomTypeId = 3, BasePrice = 199.99)

All bookings include:
- Payment information (amount, method, status, transaction ID)
- Promotion usage tracking (phone hash, card identifier, device fingerprint, IP address)
- QR tokens for check-in
- Check-in/check-out timestamps
- Booking source tracking (Direct, OTA, Group, Phone, WalkIn)

===========================================================================
CODE DOCUMENTATION (PRESENTATION READY)
===========================================================================
All code files now include comprehensive presentation-ready documentation:

INLINE COMMENTS (4,198+ comments across 54 files):
- Controllers: 775+ inline comments explaining business logic, validation, security checks
- Services: 161+ inline comments explaining algorithms, validation processes, error handling
- Models: XML documentation comments for all properties and relationships
- Helpers: 25+ inline comments explaining utility functions and authentication flow
- Middleware: 17+ inline comments explaining security headers and configuration
- Program.cs: 107+ inline comments explaining application startup and configuration

COMMENT COVERAGE:
- BookingController.cs: 177 inline comments (booking creation, payment, promotion validation)
- AdminController.cs: 281 inline comments (dashboard, CRUD operations, statistics)
- SecurityController.cs: 107 inline comments (authentication, password reset, email verification)
- HomeController.cs: 68 inline comments (featured rooms, statistics, package display)
- RoomController.cs: 59 inline comments (room catalog, search, filtering)
- CustomerController.cs: 53 inline comments (profile management, password changes)
- PromotionValidationService.cs: 65 inline comments (validation rules, abuse prevention)
- BookingStatusUpdateService.cs: 19 inline comments (automatic status updates)
- EmailService.cs: 24 inline comments (SMTP configuration, email sending)
- EncryptionService.cs: 41 inline comments (AES encryption/decryption)
- DbInitializer.cs: 71 inline comments (database seeding logic)
- AddMissingRoomTypes.cs: 39 inline comments (room type creation)

COMMENT TYPES INCLUDED:
1. XML Documentation Comments (///) - Class summaries, method descriptions, parameters
2. Inline Comments - Step-by-step logic explanations, business rules, security checks
3. Section Headers - Organized code sections for easy navigation
4. Algorithm Explanations - Complex logic breakdowns
5. Security Documentation - Security feature explanations
6. Error Handling - Error handling strategies

This comprehensive documentation is essential for:
- Code maintenance and updates
- Team collaboration
- Presentation and demonstration (tutor questions)
- Future development
- Code review and quality assurance
