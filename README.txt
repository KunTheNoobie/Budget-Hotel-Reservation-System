BMIT2023 Web and Mobile Systems Assignment
Budget Hotel Reservation System
==========================================

PROJECT INFORMATION
-------------------
Project Title: Budget Hotel Reservation System
Technology Stack: ASP.NET Core MVC, Entity Framework Core, SQL Server Express, .NET 9
Database: SQL Server Express (file-based)

LOGIN CREDENTIALS
-----------------

ADMIN ACCOUNTS:
---------------
Email: admin@hotel.com
Password: Admin123!
Role: Admin
Access: Full system access including user management, hotel management, room management, and booking management

Email: manager@hotel.com
Password: Manager123!
Role: Manager
Access: Full system access (same as Admin)

HOW TO ACCESS ADMIN PAGE:
--------------------------
1. Login with admin or manager account (admin@hotel.com or manager@hotel.com)
2. After login, you will see an "Admin" dropdown menu in the navigation bar
3. Click on "Admin" dropdown to access:
   - Users (User Management)
   - Hotels (Hotel Management)
   - Rooms (Room Management)
   - Bookings (Booking Management)
4. Alternatively, you can directly access: /Admin/Index for the admin dashboard
5. Admin pages are protected - only Admin, Manager, and Staff roles can access them

CUSTOMER ACCOUNTS:
------------------
Email: ahmad@example.com
Password: Ahmad123!
Role: Customer
Status: Email Verified, Active
Note: Has existing bookings for testing

Email: siti@example.com
Password: Siti123!
Role: Customer
Status: Email Verified, Active
Note: Has pending bookings for testing

Email: charlie@example.com
Password: Charlie123!
Role: Customer
Status: Email NOT Verified, Active
Note: Use this account to test email verification flow

SYSTEM FEATURES
---------------

CORE MODULES:
1. Security Module
   - User registration with email verification
   - Login/Logout with cookie-based authentication
   - Password reset functionality
   - Login blocking after 3 failed attempts (15-minute lockout)
   - Password encryption using BCrypt

2. Admin Module
   - User management (Create, Read, Update, Delete)
   - Hotel management (CRUD operations)
   - Room management (CRUD operations)
   - Booking management (View, Update status)
   - Dashboard with statistics

3. Customer Module
   - Profile management
   - View and edit personal information
   - Change password

4. Room Catalog Module
   - Browse available rooms
   - Search and filter rooms (AJAX-enabled)
   - View room details with amenities
   - Check room availability

5. Booking Module
   - Create bookings with date selection
   - Apply promotion codes
   - Payment processing
   - Booking confirmation with QR code
   - View booking history
   - Cancel bookings
   - Download e-receipt

ADDITIONAL FEATURES:
1. Password Encryption - All passwords are hashed using BCrypt
2. Email Verification - Token-based email verification system
3. Password Reset - Secure token-based password reset
4. Login Blocking - Temporary account lockout after 3 failed attempts
5. E-Receipt View - View receipt as a page (can be printed) instead of downloading
6. QR Code Generation - QR codes for booking confirmations
7. AJAX Search/Filter - Real-time room search and filtering
8. Multiple Photos Support - Room types can have multiple images
9. Role-Based Authorization - Different access levels for Admin, Manager, Staff, and Customer
10. Responsive Design - Mobile-friendly Bootstrap UI
11. Professional Payment Forms - Credit card, PayPal, and Bank Transfer with proper validation
12. Auto-Generated Transaction IDs - Transaction IDs are automatically generated based on payment method
13. Price Breakdown Display - Shows base price, discount, and final price in booking and payment pages
14. Admin Account Protection - Main admin account (admin@hotel.com) cannot be deleted

DATABASE SEED DATA
-----------------
The system comes pre-seeded with:
- 2 Hotels (both in Kuala Lumpur, Malaysia)
- 5 Users (1 Admin, 1 Manager, 3 Customers)
- 4 Room Types (Standard Single, Deluxe Double, Executive Suite, Family Room)
- 10 Rooms across different types
- 10 Amenities
- 3 Active Promotions
- 2 Sample Bookings (1 confirmed, 1 pending)

LOCATION INFORMATION:
- All hotels are located in Malaysia (Kuala Lumpur)
- Phone numbers use Malaysian format (+60)
- Currency is Malaysian Ringgit (RM)

INSTALLATION & SETUP
--------------------
1. Ensure you have .NET 9 SDK installed
2. Ensure SQL Server Express is installed
3. Open the solution in Visual Studio 2022 or later
4. Restore NuGet packages
5. Update the connection string in appsettings.json if needed
6. Run the application - the database will be created automatically with seed data

IMPORTANT NOTES
---------------
- The system uses cookie-based authentication (NOT ASP.NET Core Identity)
- All passwords are encrypted using BCrypt
- Email verification tokens are generated but email sending is not implemented (tokens are shown in the UI for testing)
- Password reset tokens are also shown in the UI for testing purposes
- The database is file-based SQL Server Express
- All data annotations are used for validation (not Fluent API except where necessary)
- Currency is displayed in RM (Malaysian Ringgit)

TESTING RECOMMENDATIONS
-----------------------
1. Test login with different user roles
2. Test registration and email verification flow
3. Test password reset functionality
4. Test login blocking (try wrong password 3 times)
5. Test room search and filtering with AJAX
6. Test booking creation and payment
7. Test QR code generation and receipt view (View Receipt button)
8. Test admin functions (user/hotel/room management)
9. Test authorization (try accessing admin pages as customer)

SUPPORT
-------
For any issues or questions, please refer to the project documentation or contact the development team.

