-- ============================================
-- Script to Clear All Existing Data
-- Budget Hotel Reservation System
-- ============================================
-- WARNING: This script will DELETE ALL DATA from the database
-- Make sure you have a backup before running this script
-- ============================================
-- 
-- NOTE: If your database has a different name, update the database name below
-- or modify this script to use your actual database name.
-- ============================================

-- Check if database exists and switch to it
IF DB_ID('BMIT2023_HotelReservation') IS NOT NULL
BEGIN
    USE [BMIT2023_HotelReservation]
    SET NOEXEC OFF
    PRINT 'Using database: BMIT2023_HotelReservation'
    PRINT ''
END
ELSE
BEGIN
    PRINT 'ERROR: Could not find database ''BMIT2023_HotelReservation''.'
    PRINT ''
    PRINT 'Available options:'
    PRINT '1. Create the database by running the application (it will be created automatically)'
    PRINT '2. Or manually create the database using: Assignment/Scripts/00_CreateDatabase.sql'
    PRINT '3. Or run: dotnet ef database update (from the Assignment folder)'
    PRINT ''
    PRINT 'After creating the database, run this script again.'
    PRINT ''
    PRINT 'Script execution stopped.'
    SET NOEXEC ON
END
GO

-- Disable foreign key constraints temporarily (only if tables exist)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews')
BEGIN
    EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"
END
GO

-- Delete data in reverse order of dependencies
-- Start with tables that have foreign keys pointing to them

-- Note: PromotionUsage table has been removed - usage tracking now stored in Booking table
-- Note: FavoriteRoomTypes table has been removed - feature discontinued

-- 1. Delete Reviews (references Booking)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews')
BEGIN
    DELETE FROM Reviews
    PRINT 'Deleted data from Reviews table'
END
GO

-- 3. Delete Bookings (references User, Room, Promotion)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Bookings')
BEGIN
    DELETE FROM Bookings
    PRINT 'Deleted data from Bookings table'
END
GO

-- 4. Delete PackageItems (references Package, RoomType, Service)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PackageItems')
BEGIN
    DELETE FROM PackageItems
    PRINT 'Deleted data from PackageItems table'
END
GO

-- 5. Delete Packages
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Packages')
BEGIN
    DELETE FROM Packages
    PRINT 'Deleted data from Packages table'
END
GO

-- 6. Delete Services
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Services')
BEGIN
    DELETE FROM Services
    PRINT 'Deleted data from Services table'
END
GO

-- 7. Delete RoomTypeAmenities (junction table)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomTypeAmenities')
BEGIN
    DELETE FROM RoomTypeAmenities
    PRINT 'Deleted data from RoomTypeAmenities table'
END
GO

-- 8. Delete RoomImages (references RoomType)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomImages')
BEGIN
    DELETE FROM RoomImages
    PRINT 'Deleted data from RoomImages table'
END
GO

-- 9. Delete Rooms (references RoomType)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Rooms')
BEGIN
    DELETE FROM Rooms
    PRINT 'Deleted data from Rooms table'
END
GO

-- 10. Delete RoomTypes (references Hotel)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomTypes')
BEGIN
    DELETE FROM RoomTypes
    PRINT 'Deleted data from RoomTypes table'
END
GO

-- 11. Delete Amenities
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Amenities')
BEGIN
    DELETE FROM Amenities
    PRINT 'Deleted data from Amenities table'
END
GO

-- 12. Delete ContactMessages
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ContactMessages')
BEGIN
    DELETE FROM ContactMessages
    PRINT 'Deleted data from ContactMessages table'
END
GO

-- 13. Delete Newsletters
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Newsletters')
BEGIN
    DELETE FROM Newsletters
    PRINT 'Deleted data from Newsletters table'
END
GO

-- 14. Delete SecurityTokens (references User)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SecurityTokens')
BEGIN
    DELETE FROM SecurityTokens
    PRINT 'Deleted data from SecurityTokens table'
END
GO

-- 15. Delete LoginAttempts
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LoginAttempts')
BEGIN
    DELETE FROM LoginAttempts
    PRINT 'Deleted data from LoginAttempts table'
END
GO

-- 16. Delete SecurityLogs (references User)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SecurityLogs')
BEGIN
    DELETE FROM SecurityLogs
    PRINT 'Deleted data from SecurityLogs table'
END
GO

-- 17. Delete Users (references nothing, but has foreign keys pointing to it)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    DELETE FROM Users
    PRINT 'Deleted data from Users table'
END
GO

-- 18. Delete Hotels
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Hotels')
BEGIN
    DELETE FROM Hotels
    PRINT 'Deleted data from Hotels table'
END
GO

-- 19. Delete Promotions
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Promotions')
BEGIN
    DELETE FROM Promotions
    PRINT 'Deleted data from Promotions table'
END
GO

-- Re-enable foreign key constraints (only if tables exist)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews')
BEGIN
    EXEC sp_MSforeachtable "ALTER TABLE ? CHECK CONSTRAINT all"
END
GO

-- Reset identity columns (optional, but recommended for clean IDs)
-- Note: PromotionUsages table removed - usage tracking now in Booking table
-- Note: FavoriteRoomTypes table removed - feature discontinued
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews')
BEGIN
    DBCC CHECKIDENT ('Reviews', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Bookings')
BEGIN
    DBCC CHECKIDENT ('Bookings', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PackageItems')
BEGIN
    DBCC CHECKIDENT ('PackageItems', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Packages')
BEGIN
    DBCC CHECKIDENT ('Packages', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Services')
BEGIN
    DBCC CHECKIDENT ('Services', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomImages')
BEGIN
    DBCC CHECKIDENT ('RoomImages', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Rooms')
BEGIN
    DBCC CHECKIDENT ('Rooms', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomTypes')
BEGIN
    DBCC CHECKIDENT ('RoomTypes', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Amenities')
BEGIN
    DBCC CHECKIDENT ('Amenities', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ContactMessages')
BEGIN
    DBCC CHECKIDENT ('ContactMessages', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Newsletters')
BEGIN
    DBCC CHECKIDENT ('Newsletters', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SecurityTokens')
BEGIN
    DBCC CHECKIDENT ('SecurityTokens', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LoginAttempts')
BEGIN
    DBCC CHECKIDENT ('LoginAttempts', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SecurityLogs')
BEGIN
    DBCC CHECKIDENT ('SecurityLogs', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    DBCC CHECKIDENT ('Users', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Hotels')
BEGIN
    DBCC CHECKIDENT ('Hotels', RESEED, 0)
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Promotions')
BEGIN
    DBCC CHECKIDENT ('Promotions', RESEED, 0)
END
GO

-- Only print success message if we're in the correct database context
IF DB_NAME() = 'BMIT2023_HotelReservation'
BEGIN
    PRINT ''
    PRINT 'All data has been cleared successfully!'
    PRINT 'You can now run the 02_InsertSampleData.sql script to populate the database.'
END
GO

-- Re-enable execution in case it was disabled
SET NOEXEC OFF
GO

