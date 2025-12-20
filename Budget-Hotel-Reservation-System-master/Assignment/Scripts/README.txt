Database Sample Data Scripts
=============================

This folder contains SQL scripts to populate the database with comprehensive sample data.

FILES:
------
1. 03_CompleteNewDatabase.sql (RECOMMENDED - USE THIS ONE - v4 PRESENTATION READY)
   - Complete fresh database setup with ALL recent schema changes
   - Includes comprehensive comments explaining all sections
   - Schema safety checks for all new fields:
     * Booking: QRToken, CheckInTime, CheckOutTime, TransactionId
     * Booking: Promotion usage tracking fields (PhoneNumberHash, CardIdentifier, DeviceFingerprint, IpAddress, UsedAt)
     * Hotel: Category field (Budget, MidRange, Luxury)
     * Booking: Source field (Direct, OTA, Group, Phone, WalkIn)
   - Removes UserId from Reviews table (reviews linked only to Booking)
   - Includes HotelId field for Manager/Staff hotel assignment
   - Clears existing data and inserts fresh sample data
   - Each hotel has 1 Manager and 1 Staff assigned
   - Admin has no hotel assignment (can see all hotels)
   - Total: 1 Admin + 10 Managers + 10 Staff + 15 Customers = 36 users
   - Includes role-based access control setup
   - Sample booking data includes various statuses and sources for testing
   - PRESENTATION READY: 10 hotels, 30+ room types, 300+ rooms, 10 packages, 30+ bookings, 20+ reviews

HOW TO USE:
-----------
STEP 1 - Create Database Schema (REQUIRED):
1. Open terminal/command prompt in the Assignment folder
2. Run: dotnet ef database update
   - This creates the database and all tables
   - Applies migration for HotelId field in Users table

STEP 2 - Populate Database with Sample Data:
1. Open SQL Server Management Studio (SSMS) or Azure Data Studio
2. Connect to your SQL Server instance (usually (localdb)\mssqllocaldb)
3. Open and execute: 03_CompleteNewDatabase.sql
   - This clears existing data and inserts fresh sample data
   - Assigns managers and staff to all hotels

IMPORTANT - USER PASSWORDS:
---------------------------
After running the SQL script, user passwords are set to placeholders and will NOT work for login.

To fix this:
1. Run the application once (F5 in Visual Studio or dotnet run)
2. The DbInitializer will automatically set proper BCrypt hashed passwords for admin accounts
3. For other users, use the application's password reset feature

Default Test Passwords (after application initializes):
- admin@hotel.com: Admin123!
- manager1@hotel.com through manager15@hotel.com: Manager123!
- staff1@hotel.com through staff15@hotel.com: Password123!
- All customer accounts: Use password reset feature

SAMPLE DATA INCLUDED (v4 - PRESENTATION READY):
------------------------------------------------
This script provides comprehensive, presentation-ready sample data:

- 10 Hotels (various locations across Malaysia with different categories)
  * Budget: Budget Inn KL Sentral, Penang Heritage Inn, Kuching Cultural Lodge
  * MidRange: Grand Plaza Hotel, Malacca Riverside Hotel, Johor Bahru City Hotel, etc.
  * Luxury: Langkawi Beach Resort, Kota Kinabalu Ocean View

- 36 Users:
  * 1 Admin (no hotel assignment - sees all hotels)
  * 10 Managers (one assigned to each hotel)
  * 10 Staff (one assigned to each hotel)
  * 15 Customers (no hotel assignment - many customers for comprehensive testing)

- 30 Room Types (at least 3 per hotel for comprehensive testing)
  * Hotel 1 (Budget Inn KL Sentral) has 3 room types:
    - Standard Single Room (RoomTypeId = 1, BasePrice = 79.99)
    - Deluxe Double Room (RoomTypeId = 2, BasePrice = 129.99)
    - Executive Suite (RoomTypeId = 3, BasePrice = 199.99)
  * Each hotel has unique room types (Standard, Deluxe, Suite variations)

- 300 Rooms (10 rooms per room type for comprehensive availability testing)
  * All rooms initially set to Available status
  * Unique room numbers (e.g., 101, 102, 103 for RoomType 1)

- 15 Amenities (Free Wi-Fi, Air Conditioning, Flat-Screen TV, Mini Fridge, etc.)
- 90 Room Images (3 images per room type: Main view, Bathroom, Amenities)
- 200+ Room Type-Amenity relationships (amenities linked to room types)
- 12 Services (Airport Transfer, Breakfast Buffet, Spa Treatment, City Tour, etc.)
- 10 Packages (Kuala Lumpur City Explorer, Romantic Getaway, Business Traveler, etc.)
  * Each package includes PackageItems linking room types and services
- 10 Promotions (WELCOME10, SUMMER20, WEEKEND15, etc. with abuse prevention)
- 30+ Bookings (various statuses: Pending, Confirmed, Cancelled, CheckedIn, CheckedOut, NoShow)
  * Recent bookings (last 7 days) for revenue trend charts
  * Historical bookings (last month) for analytics
  * Future bookings (pending) for testing
  * Cancelled bookings for cancellation flow testing
  * All bookings include payment information and promotion usage tracking
- 20+ Reviews (linked to checked-out bookings with ratings 1-5)
  * Reviews are linked only to Bookings (not directly to Users)
  * User information obtained from Booking.UserId
- 10 Contact Messages (customer inquiries with various subjects)
- 10 Newsletter Subscriptions (email subscriptions)

HOTEL ASSIGNMENTS:
------------------
Each of the 10 hotels has been assigned:
- 1 Manager (manager1@hotel.com through manager10@hotel.com)
- 1 Staff (staff1@hotel.com through staff10@hotel.com)

Example:
- Hotel 1 (Budget Inn KL Sentral): manager1@hotel.com, staff1@hotel.com
- Hotel 2 (Grand Plaza Hotel): manager2@hotel.com, staff2@hotel.com
- Hotel 3 (Penang Heritage Inn): manager3@hotel.com, staff3@hotel.com
- Hotel 4 (Malacca Riverside Hotel): manager4@hotel.com, staff4@hotel.com
- Hotel 5 (Johor Bahru City Hotel): manager5@hotel.com, staff5@hotel.com
- Hotel 6 (Langkawi Beach Resort): manager6@hotel.com, staff6@hotel.com
- Hotel 7 (Ipoh Heritage Boutique): manager7@hotel.com, staff7@hotel.com
- Hotel 8 (Kota Kinabalu Ocean View): manager8@hotel.com, staff8@hotel.com
- Hotel 9 (Kuching Cultural Lodge): manager9@hotel.com, staff9@hotel.com
- Hotel 10 (Cameron Highlands Retreat): manager10@hotel.com, staff10@hotel.com

Admin account (admin@hotel.com) has no hotel assignment and can see all hotels.

TROUBLESHOOTING:
---------------
- If you get "Database does not exist" error:
  → Run: dotnet ef database update (from Assignment folder)

- If you get "Invalid object name" errors:
  → Tables don't exist. Run: dotnet ef database update first

- If passwords don't work:
  → Run the application once - DbInitializer will set admin passwords

- If you need to reset everything:
  → Run 03_CompleteNewDatabase.sql again (it clears existing data first)
