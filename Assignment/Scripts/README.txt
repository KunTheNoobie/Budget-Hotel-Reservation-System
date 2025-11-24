Database Sample Data Scripts
=============================

This folder contains SQL scripts to populate the database with comprehensive sample data.

FILES:
------
1. 01_ClearAllData.sql
   - Deletes ALL existing data from the database
   - Resets identity columns
   - WARNING: This is irreversible! Backup your data first if needed

2. 02_InsertSampleData.sql
   - Inserts 10+ sample records for each table
   - Includes: Hotels, Users, Amenities, Room Types, Rooms, Services, Packages, Bookings, Reviews, etc.

HOW TO USE:
-----------
1. Open SQL Server Management Studio (SSMS) or Azure Data Studio
2. Connect to your SQL Server instance
3. Select your database: BMIT2023_HotelReservation
4. Run 01_ClearAllData.sql first (to clear existing data)
5. Run 02_InsertSampleData.sql (to insert sample data)

IMPORTANT - USER PASSWORDS:
---------------------------
After running the SQL scripts, user passwords are set to placeholders and will NOT work for login.

To fix this, you have two options:

OPTION 1 (Recommended):
- Run the application once
- The DbInitializer will automatically set proper BCrypt hashed passwords for admin accounts
- For other users, use the application's password reset feature

OPTION 2:
- Use the application's password reset feature for each user account
- Or manually update passwords in the admin panel

Default Test Passwords (after application initializes):
- admin@hotel.com: Admin123!
- manager@hotel.com: Manager123!
- staff@hotel.com: Password123!
- All customer accounts: Use password reset feature

SAMPLE DATA INCLUDED:
--------------------
- 15 Hotels (various locations in Malaysia)
- 15 Users (1 Admin, 1 Manager, 1 Staff, 12 Customers)
- 15 Amenities (with images)
- 20 Room Types (various categories)
- 25 Rooms (across different room types)
- 30 Room Images
- 50 Room Type-Amenity relationships
- 15 Services
- 15 Packages
- 30 Package Items
- 12 Promotions
- 15 Bookings (various statuses)
- 12 Reviews
- 12 Contact Messages
- 12 Newsletter Subscriptions

TROUBLESHOOTING:
---------------
- If you get foreign key constraint errors, make sure you run the scripts in order
- If passwords don't work, run the application once to initialize admin accounts
- If you need to reset everything, run 01_ClearAllData.sql again, then 02_InsertSampleData.sql

