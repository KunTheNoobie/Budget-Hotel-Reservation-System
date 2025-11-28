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
This script provides comprehensive sample data for testing and demonstration:

- 15 Hotels (various locations across Malaysia including KL, Penang, Malacca, Johor Bahru, Langkawi, Ipoh, Kota Kinabalu, Kuching, Cameron Highlands, Kuantan, Terengganu, and more)
- 15 Users (1 Admin, 1 Manager, 1 Staff, 12 Customers with different verification statuses)
- 15 Amenities (Free Wi-Fi, Air Conditioning, Flat-Screen TV, Mini Fridge, Coffee Maker, Private Bathroom, Room Service, Safe, Balcony, City View, and more)
- 20 Room Types (Standard Single, Deluxe Double, Executive Suite, Family Room, Premier Twin, Studio Apartment, Ocean View Suite, Heritage Double, Sabah View, Riverside Deluxe, Highland Cozy, and more)
- 25 Rooms (across different room types with various statuses: Available, Occupied, UnderMaintenance, Cleaning)
- 30 Room Images (multiple images per room type for showcasing)
- 50 Room Type-Amenity relationships (linking amenities to room types)
- 15 Services (Airport Transfer, Breakfast Buffet, Late Checkout, Island Hopping Tour, Spa Treatment, Candlelight Dinner, City Tour, Car Rental, Laundry Service, Room Upgrade, Wi-Fi Premium, Pet Care, Concierge, Gym Access, Pool Access)
- 15 Packages (Kuala Lumpur City Explorer, Malacca Family Fun Package, Langkawi Beach Escape, Honeymoon Bliss, Business Traveler, Adventure Seeker, Ipoh Heritage Experience, Sabah Adventure Package, Sarawak Cultural Journey, Cameron Highlands Retreat, Weekend Getaway Special, Extended Stay Value, and more)
- 30 Package Items (linking room types and services to packages with quantities)
- 15 Promotions (various discount codes with different types: Percentage and FixedAmount, with different validation rules)
- 15 Bookings (various statuses: Pending, Confirmed, Cancelled, CheckedIn, CheckedOut, NoShow)
  - Note: Promotion usage tracking is stored directly in Booking table (not separate PromotionUsage table)
- 15 Reviews (ratings and comments from customers for completed bookings)
  - Note: Reviews are linked to Booking only (user info obtained from Booking.UserId)
- 12 Contact Messages (customer inquiries with read/unread status)
- 12 Newsletter Subscriptions (email subscriptions with active/inactive status)

All data is designed to provide a realistic testing environment with diverse scenarios.

TROUBLESHOOTING:
---------------
- If you get foreign key constraint errors, make sure you run the scripts in order
- If passwords don't work, run the application once to initialize admin accounts
- If you need to reset everything, run 01_ClearAllData.sql again, then 02_InsertSampleData.sql

