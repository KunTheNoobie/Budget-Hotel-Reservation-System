Database Sample Data Scripts
=============================

This folder contains SQL scripts to populate the database with comprehensive sample data.

FILES:
------
1. 03_CompleteNewDatabase.sql (RECOMMENDED - USE THIS ONE)
   - Complete fresh database setup with ALL recent changes
   - Includes HotelId field for Manager/Staff hotel assignment
   - Clears existing data and inserts fresh sample data
   - Each hotel has 1 Manager and 1 Staff assigned
   - Admin has no hotel assignment (can see all hotels)
   - Total: 1 Admin + 15 Managers + 15 Staff + 12 Customers = 43 users
   - Includes role-based access control setup

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

SAMPLE DATA INCLUDED:
--------------------
This script provides comprehensive sample data for testing and demonstration:

- 15 Hotels (various locations across Malaysia)
- 43 Users:
  * 1 Admin (no hotel assignment - sees all hotels)
  * 15 Managers (one assigned to each hotel)
  * 15 Staff (one assigned to each hotel)
  * 12 Customers (no hotel assignment)
- 15 Amenities (Free Wi-Fi, Air Conditioning, Flat-Screen TV, etc.)
- 20 Room Types (Standard Single, Deluxe Double, Executive Suite, etc.)
- 25 Rooms (across different room types with various statuses)
- 30 Room Images (multiple images per room type)
- 50 Room Type-Amenity relationships
- 15 Services (Airport Transfer, Breakfast Buffet, Spa Treatment, etc.)
- 15 Packages (various bundled deals)
- 30 Package Items (linking room types and services to packages)
- 15 Promotions (various discount codes)
- 15 Bookings (various statuses: Pending, Confirmed, Cancelled, CheckedIn, CheckedOut, NoShow)
- 15 Reviews (ratings and comments from customers)
- 12 Contact Messages (customer inquiries)
- 12 Newsletter Subscriptions (email subscriptions)

HOTEL ASSIGNMENTS:
------------------
Each of the 15 hotels has been assigned:
- 1 Manager (manager1@hotel.com through manager15@hotel.com)
- 1 Staff (staff1@hotel.com through staff15@hotel.com)

Example:
- Hotel 1 (Budget Inn KL Sentral): manager1@hotel.com, staff1@hotel.com
- Hotel 2 (Economy Stay Bukit Bintang): manager2@hotel.com, staff2@hotel.com
- Hotel 3 (Penang Budget Hotel): manager3@hotel.com, staff3@hotel.com
- ... and so on for all 15 hotels

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
