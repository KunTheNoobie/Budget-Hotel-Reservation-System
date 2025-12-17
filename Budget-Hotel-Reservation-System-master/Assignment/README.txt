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

2. MANAGER ACCOUNTS (15 managers, one per hotel)
   Email:    manager1@hotel.com through manager15@hotel.com
   Password: Manager123! (same for all managers)
   Role:     Manager (Full Access scoped to assigned hotel)
   Note:     Each manager can only see/manage their assigned hotel
             Cannot create hotels or users
             Can create room types, rooms, packages, promotions for their hotel

3. STAFF ACCOUNTS (15 staff, one per hotel)
   Email:    staff1@hotel.com through staff15@hotel.com
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
