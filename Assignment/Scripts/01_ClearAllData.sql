-- ============================================
-- Script to Clear All Existing Data
-- Budget Hotel Reservation System
-- ============================================
-- WARNING: This script will DELETE ALL DATA from the database
-- Make sure you have a backup before running this script
-- ============================================

USE [BMIT2023_HotelReservation]
GO

-- Disable foreign key constraints temporarily
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"
GO

-- Delete data in reverse order of dependencies
-- Start with tables that have foreign keys pointing to them

-- 1. Delete PromotionUsage (references Promotion, Booking, User)
DELETE FROM PromotionUsages
GO

-- 2. Delete Reviews (references Booking, User)
DELETE FROM Reviews
GO

-- 3. Delete Bookings (references User, Room, Promotion)
DELETE FROM Bookings
GO

-- 4. Delete PackageItems (references Package, RoomType, Service)
DELETE FROM PackageItems
GO

-- 5. Delete Packages
DELETE FROM Packages
GO

-- 6. Delete Services
DELETE FROM Services
GO

-- 7. Delete RoomTypeAmenities (junction table)
DELETE FROM RoomTypeAmenities
GO

-- 8. Delete RoomImages (references RoomType)
DELETE FROM RoomImages
GO

-- 9. Delete Rooms (references RoomType)
DELETE FROM Rooms
GO

-- 10. Delete RoomTypes (references Hotel)
DELETE FROM RoomTypes
GO

-- 11. Delete Amenities
DELETE FROM Amenities
GO

-- 12. Delete ContactMessages
DELETE FROM ContactMessages
GO

-- 13. Delete Newsletters
DELETE FROM Newsletters
GO

-- 14. Delete SecurityTokens (references User)
DELETE FROM SecurityTokens
GO

-- 15. Delete LoginAttempts
DELETE FROM LoginAttempts
GO

-- 16. Delete SecurityLogs (references User)
DELETE FROM SecurityLogs
GO

-- 17. Delete Users (references nothing, but has foreign keys pointing to it)
DELETE FROM Users
GO

-- 18. Delete Hotels
DELETE FROM Hotels
GO

-- 19. Delete Promotions
DELETE FROM Promotions
GO

-- Re-enable foreign key constraints
EXEC sp_MSforeachtable "ALTER TABLE ? CHECK CONSTRAINT all"
GO

-- Reset identity columns (optional, but recommended for clean IDs)
DBCC CHECKIDENT ('PromotionUsages', RESEED, 0)
DBCC CHECKIDENT ('Reviews', RESEED, 0)
DBCC CHECKIDENT ('Bookings', RESEED, 0)
DBCC CHECKIDENT ('PackageItems', RESEED, 0)
DBCC CHECKIDENT ('Packages', RESEED, 0)
DBCC CHECKIDENT ('Services', RESEED, 0)
DBCC CHECKIDENT ('RoomImages', RESEED, 0)
DBCC CHECKIDENT ('Rooms', RESEED, 0)
DBCC CHECKIDENT ('RoomTypes', RESEED, 0)
DBCC CHECKIDENT ('Amenities', RESEED, 0)
DBCC CHECKIDENT ('ContactMessages', RESEED, 0)
DBCC CHECKIDENT ('Newsletters', RESEED, 0)
DBCC CHECKIDENT ('SecurityTokens', RESEED, 0)
DBCC CHECKIDENT ('LoginAttempts', RESEED, 0)
DBCC CHECKIDENT ('SecurityLogs', RESEED, 0)
DBCC CHECKIDENT ('Users', RESEED, 0)
DBCC CHECKIDENT ('Hotels', RESEED, 0)
DBCC CHECKIDENT ('Promotions', RESEED, 0)
GO

PRINT 'All data has been cleared successfully!'
PRINT 'You can now run the 02_InsertSampleData.sql script to populate the database.'
GO

