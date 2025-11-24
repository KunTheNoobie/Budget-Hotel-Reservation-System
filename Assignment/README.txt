BMIT2023 Web and Mobile Systems Assignment
Budget Hotel Reservation System

===========================================================================
LOGIN CREDENTIALS
===========================================================================

1. ADMIN ACCOUNT
   Email:    admin@hotel.com
   Password: Password123!
   Role:     Admin (Full Access)

2. MANAGER ACCOUNT
   Email:    manager@hotel.com
   Password: Password123!
   Role:     Manager (Manage Hotels, Rooms, Bookings)

3. STAFF ACCOUNT
   Email:    staff@hotel.com
   Password: Password123!
   Role:     Staff (Manage Bookings)

4. CUSTOMER ACCOUNT
   Email:    customer@example.com
   Password: Password123!
   Role:     Customer (Book Rooms, View Profile)

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

===========================================================================
FEATURES IMPLEMENTED
===========================================================================
Core Modules:
- Authentication & Authorization (Cookie-based)
- Hotel & Room Management
- Booking System
- Customer Profile Management

Additional Features:
1. Admin Dashboard with Charts (Chart.js)
2. Print-Friendly Booking Receipts
3. Math Captcha for Registration
4. Export Bookings to CSV
5. Password Reset Simulation
6. Email Verification Simulation
7. Search & Filter for Hotels/Rooms
8. Responsive "Premium" UI Design
