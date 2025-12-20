-- ====================================================================================
-- COMPREHENSIVE HOTEL DATABASE SCRIPT v4 - PRESENTATION READY
-- ====================================================================================
-- This script creates a complete, presentation-ready database for the Budget Hotel Reservation System.
-- 
-- DATA OVERVIEW:
-- - 10 Hotels (each with at least 3 room types = 30+ room types total)
-- - 10+ Users per role type (Admin, Managers, Staff, Customers)
-- - 10+ Amenities
-- - 10+ Services
-- - 10+ Packages (with PackageItems)
-- - 10+ Promotions
-- - 10+ Bookings (various statuses for demonstration)
-- - 10+ Reviews (linked to completed bookings)
-- - 10+ ContactMessages
-- - 10+ Newsletters
-- - RoomTypeAmenities (linking amenities to room types)
-- - RoomImages (images for room types)
--
-- SCHEMA UPDATES INCLUDED:
-- - Booking table: QRToken, CheckInTime, CheckOutTime, TransactionId
-- - Booking table: Promotion usage tracking fields
-- - Hotel table: Category field (0=Budget, 1=MidRange, 2=Luxury)
-- - Booking table: Source field (0=Direct, 1=OTA, 2=Group, 3=Phone, 4=WalkIn)
-- - Review table: Removed UserId (Reviews now linked only to Booking)
--
-- IMPORTANT NOTES:
-- - All tables use soft delete pattern (IsDeleted flag + DeletedAt timestamp)
-- - Payment information is merged into Booking table
-- - Promotion usage tracking is stored directly in Booking table
-- - Reviews are linked only to Booking
-- ====================================================================================

-- Check if database exists
IF DB_ID('BMIT2023_HotelReservation') IS NOT NULL
BEGIN
    PRINT 'Using database: BMIT2023_HotelReservation'
END
ELSE
BEGIN
    RAISERROR('Database BMIT2023_HotelReservation not found. Run migrations first!', 16, 1)
    RETURN
END
GO

USE [BMIT2023_HotelReservation]
GO

-- ============================================
-- 1. CLEANUP & RESET
-- ============================================
PRINT 'Clearing existing data...'

-- Disable constraints
EXEC sp_msforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

-- Delete data in correct order (respecting foreign keys)
DELETE FROM Reviews
DELETE FROM Bookings
DELETE FROM PackageItems
DELETE FROM Packages
DELETE FROM RoomTypeAmenities
DELETE FROM RoomImages
DELETE FROM Rooms
DELETE FROM RoomTypes
DELETE FROM Services
DELETE FROM Amenities
DELETE FROM ContactMessages
DELETE FROM Newsletters
DELETE FROM Promotions
DELETE FROM SecurityLogs
DELETE FROM LoginAttempts
DELETE FROM SecurityTokens
DELETE FROM Users
DELETE FROM Hotels

-- Reset identity columns
DBCC CHECKIDENT ('Hotels', RESEED, 0)
DBCC CHECKIDENT ('Users', RESEED, 0)
DBCC CHECKIDENT ('Amenities', RESEED, 0)
DBCC CHECKIDENT ('RoomTypes', RESEED, 0)
DBCC CHECKIDENT ('Rooms', RESEED, 0)
DBCC CHECKIDENT ('RoomImages', RESEED, 0)
DBCC CHECKIDENT ('Services', RESEED, 0)
DBCC CHECKIDENT ('Packages', RESEED, 0)
DBCC CHECKIDENT ('PackageItems', RESEED, 0)
DBCC CHECKIDENT ('Promotions', RESEED, 0)
DBCC CHECKIDENT ('Bookings', RESEED, 0)
DBCC CHECKIDENT ('Reviews', RESEED, 0)
DBCC CHECKIDENT ('ContactMessages', RESEED, 0)
DBCC CHECKIDENT ('Newsletters', RESEED, 0)

-- Re-enable constraints
EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"

PRINT 'Cleanup Complete.'
GO

-- ============================================
-- 2. ENSURE SCHEMA (Safety Check)
-- ============================================
-- Ensure Hotel.Category column exists (0=Budget, 1=MidRange, 2=Luxury)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Hotels') AND name = 'Category')
BEGIN
    ALTER TABLE Hotels ADD Category int NOT NULL DEFAULT 0;
    PRINT 'Added Category column to Hotels table.'
END

-- Ensure Booking.Source column exists (0=Direct, 1=OTA, 2=Group, 3=Phone, 4=WalkIn)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'Source')
BEGIN
    ALTER TABLE Bookings ADD Source int NOT NULL DEFAULT 0;
    PRINT 'Added Source column to Bookings table.'
END

-- Ensure Booking.QRToken column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'QRToken')
BEGIN
    ALTER TABLE Bookings ADD QRToken uniqueidentifier NULL;
    PRINT 'Added QRToken column to Bookings table.'
END

-- Ensure Booking.CheckInTime column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'CheckInTime')
BEGIN
    ALTER TABLE Bookings ADD CheckInTime datetime2 NULL;
    PRINT 'Added CheckInTime column to Bookings table.'
END

-- Ensure Booking.CheckOutTime column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'CheckOutTime')
BEGIN
    ALTER TABLE Bookings ADD CheckOutTime datetime2 NULL;
    PRINT 'Added CheckOutTime column to Bookings table.'
END

-- Ensure Booking.TransactionId column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'TransactionId')
BEGIN
    ALTER TABLE Bookings ADD TransactionId nvarchar(255) NULL;
    PRINT 'Added TransactionId column to Bookings table.'
END

-- Ensure Booking.PromotionPhoneNumberHash column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'PromotionPhoneNumberHash')
BEGIN
    ALTER TABLE Bookings ADD PromotionPhoneNumberHash nvarchar(255) NULL;
    PRINT 'Added PromotionPhoneNumberHash column to Bookings table.'
END

-- Ensure Booking.PromotionCardIdentifier column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'PromotionCardIdentifier')
BEGIN
    ALTER TABLE Bookings ADD PromotionCardIdentifier nvarchar(100) NULL;
    PRINT 'Added PromotionCardIdentifier column to Bookings table.'
END

-- Ensure Booking.PromotionDeviceFingerprint column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'PromotionDeviceFingerprint')
BEGIN
    ALTER TABLE Bookings ADD PromotionDeviceFingerprint nvarchar(100) NULL;
    PRINT 'Added PromotionDeviceFingerprint column to Bookings table.'
END

-- Ensure Booking.PromotionIpAddress column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'PromotionIpAddress')
BEGIN
    ALTER TABLE Bookings ADD PromotionIpAddress nvarchar(50) NULL;
    PRINT 'Added PromotionIpAddress column to Bookings table.'
END

-- Ensure Booking.PromotionUsedAt column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'PromotionUsedAt')
BEGIN
    ALTER TABLE Bookings ADD PromotionUsedAt datetime2 NULL;
    PRINT 'Added PromotionUsedAt column to Bookings table.'
END

-- Ensure Review table does NOT have UserId
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Reviews') AND name = 'UserId')
BEGIN
    ALTER TABLE Reviews DROP COLUMN UserId;
    PRINT 'Removed UserId column from Reviews table.'
END
GO

-- ============================================
-- 3. INSERT INFRASTRUCTURE DATA
-- ============================================

-- A. HOTELS (10 Hotels)
-- Category: 0=Budget, 1=MidRange, 2=Luxury
PRINT 'Inserting 10 Hotels...'
INSERT INTO Hotels (Name, Address, City, PostalCode, Country, ContactNumber, ContactEmail, Description, ImageUrl, Category, IsDeleted, DeletedAt) VALUES
('Budget Inn KL Sentral', '123 Jalan Tun Sambanthan', 'Kuala Lumpur', '50470', 'Malaysia', '+60-3-2274-0101', 'info@budgetinnkl.com', 'A comfortable budget hotel in the heart of KL Sentral. Perfect for business and leisure travelers.', '/images/hotels/kl-sentral.jpg', 0, 0, NULL),
('Grand Plaza Hotel', '456 Jalan Bukit Bintang', 'Kuala Lumpur', '55100', 'Malaysia', '+60-3-2142-0202', 'contact@grandplaza.com', 'Mid-range hotel with modern amenities in vibrant Bukit Bintang area.', '/images/hotels/bukit-bintang.jpg', 1, 0, NULL),
('Penang Heritage Inn', '789 Jalan Penang', 'George Town', '10000', 'Malaysia', '+60-4-261-1234', 'info@penangheritage.com', 'Charming heritage hotel close to UNESCO World Heritage sites and local food.', '/images/hotels/penang.jpg', 0, 0, NULL),
('Malacca Riverside Hotel', '321 Jalan Hang Jebat', 'Malacca', '75200', 'Malaysia', '+60-6-281-5678', 'contact@malaccariverside.com', 'Beautiful hotel along the Malacca River, walking distance to Jonker Street.', '/images/hotels/malacca.jpg', 1, 0, NULL),
('Johor Bahru City Hotel', '654 Jalan Wong Ah Fook', 'Johor Bahru', '80000', 'Malaysia', '+60-7-222-9012', 'info@jbhotel.com', 'Convenient mid-range hotel near JB city center and shopping malls.', '/images/hotels/jb.jpg', 1, 0, NULL),
('Langkawi Beach Resort', '147 Pantai Cenang', 'Langkawi', '07000', 'Malaysia', '+60-4-955-3456', 'stay@langkawiresort.com', 'Luxury beachfront resort with stunning sea views and world-class amenities.', '/images/hotels/langkawi.jpg', 2, 0, NULL),
('Ipoh Heritage Boutique', '258 Jalan Sultan Yussuf', 'Ipoh', '30000', 'Malaysia', '+60-5-241-7890', 'info@ipohboutique.com', 'Charming boutique hotel in Ipoh old town with colonial architecture.', '/images/hotels/ipoh.jpg', 1, 0, NULL),
('Kota Kinabalu Ocean View', '369 Jalan Gaya', 'Kota Kinabalu', '88000', 'Malaysia', '+60-88-234-5678', 'contact@kkhotel.com', 'Luxury hotel with ocean views, close to waterfront and shopping areas.', '/images/hotels/kk.jpg', 2, 0, NULL),
('Kuching Cultural Lodge', '741 Jalan Main Bazaar', 'Kuching', '93000', 'Malaysia', '+60-82-456-7890', 'stay@kuchinglodge.com', 'Budget-friendly lodge along the Sarawak River, near cultural attractions.', '/images/hotels/kuching.jpg', 0, 0, NULL),
('Cameron Highlands Retreat', '852 Jalan Persiaran Camellia', 'Cameron Highlands', '39000', 'Malaysia', '+60-5-491-2345', 'info@cameronretreat.com', 'Cozy mid-range retreat in the cool highlands, perfect for nature lovers.', '/images/hotels/cameron.jpg', 1, 0, NULL);
GO

-- B. USERS (10+ per role type)
-- Role: 0=Admin, 1=Manager, 2=Staff, 3=Customer
PRINT 'Inserting Users...'

-- 1 Admin user
INSERT INTO Users (Email, FullName, PasswordHash, Role, IsEmailVerified, IsActive, CreatedAt, HotelId, PreferredLanguage, Theme, IsDeleted) VALUES
('admin@hotel.com', 'System Administrator', 'PLACEHOLDER_HASH', 0, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0);

-- 10 Manager users (one per hotel)
INSERT INTO Users (Email, FullName, PasswordHash, Role, IsEmailVerified, IsActive, CreatedAt, HotelId, PreferredLanguage, Theme, IsDeleted)
SELECT 'manager' + CAST(HotelId AS VARCHAR) + '@hotel.com', 'Manager ' + Name, 'PLACEHOLDER_HASH', 1, 1, 1, GETDATE(), HotelId, 'en-US', 'Default', 0 FROM Hotels;

-- 10 Staff users (one per hotel)
INSERT INTO Users (Email, FullName, PasswordHash, Role, IsEmailVerified, IsActive, CreatedAt, HotelId, PreferredLanguage, Theme, IsDeleted)
SELECT 'staff' + CAST(HotelId AS VARCHAR) + '@hotel.com', 'Staff ' + Name, 'PLACEHOLDER_HASH', 2, 1, 1, GETDATE(), HotelId, 'en-US', 'Default', 0 FROM Hotels;

-- 15 Customer users
INSERT INTO Users (Email, FullName, PasswordHash, Role, IsEmailVerified, IsActive, CreatedAt, HotelId, PreferredLanguage, Theme, IsDeleted) VALUES
('ahmad@example.com', 'Ahmad Zulkifli', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('siti@example.com', 'Siti Nurhaliza', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('charlie@example.com', 'Charlie Brown', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('sarah@example.com', 'Sarah Tan', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('ali@example.com', 'Mohammad Ali', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('lisa@example.com', 'Lisa Wong', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('david@example.com', 'David Lee', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('nurul@example.com', 'Nurul Aisyah', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('james@example.com', 'James Lim', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('fatimah@example.com', 'Fatimah Zahra', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('kevin@example.com', 'Kevin Chen', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('amira@example.com', 'Amira Hassan', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('john@example.com', 'John Smith', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('maria@example.com', 'Maria Garcia', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0),
('robert@example.com', 'Robert Johnson', 'PLACEHOLDER_HASH', 3, 1, 1, GETDATE(), NULL, 'en-US', 'Default', 0);
GO

-- C. AMENITIES (10+ Amenities)
PRINT 'Inserting Amenities...'
INSERT INTO Amenities (Name, ImageUrl, IsDeleted) VALUES
('Free Wi-Fi', '/images/amenities/wifi.png', 0),
('Air Conditioning', '/images/amenities/ac.png', 0),
('Flat-Screen TV', '/images/amenities/tv.png', 0),
('Mini Fridge', '/images/amenities/fridge.png', 0),
('Coffee Maker', '/images/amenities/coffee.png', 0),
('Private Bathroom', '/images/amenities/bathroom.png', 0),
('Room Service', '/images/amenities/roomservice.png', 0),
('Safe', '/images/amenities/safe.png', 0),
('Balcony', '/images/amenities/balcony.png', 0),
('City View', '/images/amenities/cityview.png', 0),
('Ocean View', '/images/amenities/oceanview.png', 0),
('Swimming Pool', '/images/amenities/pool.png', 0),
('Gym Access', '/images/amenities/gym.png', 0),
('Parking', '/images/amenities/parking.png', 0),
('Laundry Service', '/images/amenities/laundry.png', 0);
GO

-- D. ROOM TYPES (30+ Room Types - at least 3 per hotel)
-- Each hotel gets at least 3 room types
PRINT 'Inserting Room Types (at least 3 per hotel)...'
INSERT INTO RoomTypes (Name, Description, Occupancy, BasePrice, HotelId, IsDeleted) VALUES
-- Hotel 1: Budget Inn KL Sentral (3 room types)
('Standard Single Room', 'Cozy single room perfect for solo travelers. Includes free Wi-Fi and air conditioning.', 1, 79.99, 1, 0),
('Deluxe Double Room', 'Spacious double room with modern amenities. Ideal for couples or business travelers.', 2, 129.99, 1, 0),
('Executive Suite', 'Luxurious suite with separate living area and city view. Perfect for extended stays.', 2, 199.99, 1, 0),
-- Hotel 2: Grand Plaza Hotel (3 room types)
('Superior Room', 'Comfortable room with city views and premium amenities.', 2, 149.99, 2, 0),
('Deluxe Room', 'Spacious room with modern furnishings and excellent service.', 2, 189.99, 2, 0),
('Junior Suite', 'Elegant suite with separate living area and premium amenities.', 3, 249.99, 2, 0),
-- Hotel 3: Penang Heritage Inn (3 room types)
('Heritage Single Room', 'Charming single room with colonial-style furnishings.', 1, 89.99, 3, 0),
('Heritage Double Room', 'Comfortable double room featuring heritage architecture.', 2, 139.99, 3, 0),
('Heritage Suite', 'Luxurious suite with period furniture and modern amenities.', 2, 219.99, 3, 0),
-- Hotel 4: Malacca Riverside Hotel (3 room types)
('Riverside Standard', 'Comfortable room with river views.', 2, 119.99, 4, 0),
('Riverside Deluxe', 'Spacious room with balcony overlooking the river.', 2, 169.99, 4, 0),
('Riverside Suite', 'Premium suite with panoramic river and city views.', 3, 239.99, 4, 0),
-- Hotel 5: Johor Bahru City Hotel (3 room types)
('City View Room', 'Modern room with city skyline views.', 2, 109.99, 5, 0),
('Deluxe City Room', 'Spacious room with premium city views and amenities.', 2, 159.99, 5, 0),
('Executive City Suite', 'Luxurious suite with stunning city views.', 3, 229.99, 5, 0),
-- Hotel 6: Langkawi Beach Resort (3 room types)
('Beachfront Standard', 'Direct beach access with ocean views.', 2, 299.99, 6, 0),
('Beachfront Deluxe', 'Spacious beachfront room with private balcony.', 2, 399.99, 6, 0),
('Beachfront Villa', 'Luxury villa with private pool and beach access.', 4, 599.99, 6, 0),
-- Hotel 7: Ipoh Heritage Boutique (3 room types)
('Boutique Single', 'Charming single room with heritage charm.', 1, 99.99, 7, 0),
('Boutique Double', 'Elegant double room with period details.', 2, 149.99, 7, 0),
('Boutique Suite', 'Luxurious suite with heritage architecture.', 2, 229.99, 7, 0),
-- Hotel 8: Kota Kinabalu Ocean View (3 room types)
('Ocean View Standard', 'Room with stunning ocean views.', 2, 249.99, 8, 0),
('Ocean View Deluxe', 'Spacious room with premium ocean views.', 2, 329.99, 8, 0),
('Ocean View Penthouse', 'Luxury penthouse with panoramic ocean views.', 4, 499.99, 8, 0),
-- Hotel 9: Kuching Cultural Lodge (3 room types)
('Lodge Standard Room', 'Comfortable budget room with basic amenities.', 2, 69.99, 9, 0),
('Lodge Deluxe Room', 'Spacious room with river views.', 2, 99.99, 9, 0),
('Lodge Family Room', 'Large room perfect for families.', 4, 139.99, 9, 0),
-- Hotel 10: Cameron Highlands Retreat (3 room types)
('Highland Standard', 'Cozy room with mountain views.', 2, 119.99, 10, 0),
('Highland Deluxe', 'Spacious room with premium mountain views.', 2, 169.99, 10, 0),
('Highland Suite', 'Luxury suite with fireplace and mountain views.', 3, 249.99, 10, 0);
GO

-- E. ROOMS (10+ rooms per room type = 300+ rooms total)
-- Status: 0=Available, 1=Occupied, 2=UnderMaintenance, 3=Cleaning
PRINT 'Inserting Rooms (10+ per room type)...'
-- Generate rooms for each room type (10 rooms per room type)
DECLARE @RoomTypeId INT = 1;
DECLARE @RoomCounter INT;
WHILE @RoomTypeId <= 30
BEGIN
    SET @RoomCounter = 1;
    WHILE @RoomCounter <= 10
    BEGIN
        INSERT INTO Rooms (RoomNumber, RoomTypeId, Status, IsDeleted)
        VALUES (CAST(@RoomTypeId AS VARCHAR) + RIGHT('00' + CAST(@RoomCounter AS VARCHAR), 2), @RoomTypeId, 0, 0);
        SET @RoomCounter = @RoomCounter + 1;
    END
    SET @RoomTypeId = @RoomTypeId + 1;
END
GO

-- F. ROOM TYPE AMENITIES (Link amenities to room types)
PRINT 'Inserting Room Type Amenities...'
-- Link common amenities to all room types
INSERT INTO RoomTypeAmenities (RoomTypeId, AmenityId)
SELECT rt.RoomTypeId, a.AmenityId
FROM RoomTypes rt
CROSS JOIN Amenities a
WHERE a.Name IN ('Free Wi-Fi', 'Air Conditioning', 'Private Bathroom', 'Flat-Screen TV')
AND rt.RoomTypeId <= 30;

-- Add premium amenities to deluxe/suite rooms
INSERT INTO RoomTypeAmenities (RoomTypeId, AmenityId)
SELECT rt.RoomTypeId, a.AmenityId
FROM RoomTypes rt
CROSS JOIN Amenities a
WHERE (rt.Name LIKE '%Suite%' OR rt.Name LIKE '%Deluxe%' OR rt.Name LIKE '%Executive%' OR rt.Name LIKE '%Villa%' OR rt.Name LIKE '%Penthouse%')
AND a.Name IN ('Mini Fridge', 'Coffee Maker', 'Safe', 'Balcony', 'Room Service');

-- Add view amenities based on room type names
INSERT INTO RoomTypeAmenities (RoomTypeId, AmenityId)
SELECT rt.RoomTypeId, a.AmenityId
FROM RoomTypes rt
CROSS JOIN Amenities a
WHERE (rt.Name LIKE '%City View%' OR rt.Name LIKE '%City%') AND a.Name = 'City View';

INSERT INTO RoomTypeAmenities (RoomTypeId, AmenityId)
SELECT rt.RoomTypeId, a.AmenityId
FROM RoomTypes rt
CROSS JOIN Amenities a
WHERE (rt.Name LIKE '%Ocean%' OR rt.Name LIKE '%Beach%') AND a.Name = 'Ocean View';
GO

-- G. ROOM IMAGES (Multiple images per room type)
PRINT 'Inserting Room Images...'
-- Add 3 images per room type
DECLARE @RTId INT = 1;
WHILE @RTId <= 30
BEGIN
    INSERT INTO RoomImages (RoomTypeId, ImageUrl, Caption, IsDeleted) VALUES
    (@RTId, '/images/rooms/room' + CAST(@RTId AS VARCHAR) + '_1.jpg', 'Main view', 0),
    (@RTId, '/images/rooms/room' + CAST(@RTId AS VARCHAR) + '_2.jpg', 'Bathroom', 0),
    (@RTId, '/images/rooms/room' + CAST(@RTId AS VARCHAR) + '_3.jpg', 'Amenities', 0);
    SET @RTId = @RTId + 1;
END
GO

-- H. SERVICES (10+ Services)
PRINT 'Inserting Services...'
INSERT INTO Services (Name, Description, Price, IsDeleted, DeletedAt) VALUES
('Airport Transfer', 'Comfortable pickup and drop-off service to/from airport', 50.00, 0, NULL),
('Breakfast Buffet', 'International breakfast buffet with local and Western options', 25.00, 0, NULL),
('Laundry Service', 'Professional dry cleaning and laundry service', 30.00, 0, NULL),
('Room Service', '24/7 room service with extensive menu', 0.00, 0, NULL),
('Spa Treatment', 'Relaxing massage and spa treatment', 150.00, 0, NULL),
('City Tour', 'Guided city tour with professional guide', 80.00, 0, NULL),
('Car Rental', 'Daily car rental with insurance included', 120.00, 0, NULL),
('Late Checkout', 'Extended checkout time until 2 PM', 30.00, 0, NULL),
('Wi-Fi Premium', 'High-speed premium Wi-Fi access', 15.00, 0, NULL),
('Minibar Refill', 'Complimentary minibar refill service', 40.00, 0, NULL),
('Gym Access', 'Full access to fitness center and gym facilities', 20.00, 0, NULL),
('Swimming Pool Access', 'Access to swimming pool and poolside facilities', 25.00, 0, NULL);
GO

-- I. PACKAGES (10+ Packages)
PRINT 'Inserting Packages...'
INSERT INTO Packages (Name, Description, TotalPrice, ImageUrl, IsActive, IsDeleted, DeletedAt) VALUES
('Kuala Lumpur City Explorer', '3 nights accommodation + city tour + breakfast buffet. Perfect for exploring KL.', 599.99, '/images/packages/kl-explorer.jpg', 1, 0, NULL),
('Romantic Getaway', '2 nights in deluxe room + spa treatment + romantic dinner. Ideal for couples.', 799.99, '/images/packages/romantic.jpg', 1, 0, NULL),
('Business Traveler', '5 nights accommodation + airport transfer + late checkout + Wi-Fi premium.', 899.99, '/images/packages/business.jpg', 1, 0, NULL),
('Family Fun Package', '3 nights family room + breakfast + city tour + swimming pool access.', 1099.99, '/images/packages/family.jpg', 1, 0, NULL),
('Beach Paradise', '4 nights beachfront room + breakfast + spa treatment + car rental.', 1999.99, '/images/packages/beach.jpg', 1, 0, NULL),
('Heritage Experience', '3 nights heritage room + guided heritage tour + breakfast buffet.', 749.99, '/images/packages/heritage.jpg', 1, 0, NULL),
('Weekend Escape', '2 nights accommodation + breakfast + late checkout. Perfect weekend getaway.', 399.99, '/images/packages/weekend.jpg', 1, 0, NULL),
('Luxury Indulgence', '3 nights suite + spa treatment + premium services + airport transfer.', 2499.99, '/images/packages/luxury.jpg', 1, 0, NULL),
('Adventure Package', '3 nights accommodation + car rental + city tour + gym access.', 899.99, '/images/packages/adventure.jpg', 1, 0, NULL),
('Extended Stay', '7 nights accommodation + breakfast + laundry service + late checkout.', 1299.99, '/images/packages/extended.jpg', 1, 0, NULL);
GO

-- J. PACKAGE ITEMS (Link services and room types to packages)
-- PackageItem uses RoomTypeId (for room types) and ServiceId (for services)
-- One must be NULL and the other must have a value
PRINT 'Inserting Package Items...'
-- Package 1: Kuala Lumpur City Explorer
INSERT INTO PackageItems (PackageId, RoomTypeId, ServiceId, Quantity, IsDeleted) VALUES
(1, 1, NULL, 3, 0), -- 3 nights Standard Single Room (RoomTypeId=1)
(1, NULL, 6, 1, 0), -- City Tour (ServiceId=6)
(1, NULL, 2, 3, 0); -- Breakfast Buffet (ServiceId=2, 3 days)

-- Package 2: Romantic Getaway
INSERT INTO PackageItems (PackageId, RoomTypeId, ServiceId, Quantity, IsDeleted) VALUES
(2, 2, NULL, 2, 0), -- 2 nights Deluxe Double Room (RoomTypeId=2)
(2, NULL, 5, 1, 0), -- Spa Treatment (ServiceId=5)
(2, NULL, 2, 2, 0); -- Breakfast Buffet (ServiceId=2, 2 days)

-- Package 3: Business Traveler
INSERT INTO PackageItems (PackageId, RoomTypeId, ServiceId, Quantity, IsDeleted) VALUES
(3, 1, NULL, 5, 0), -- 5 nights Standard Single Room (RoomTypeId=1)
(3, NULL, 1, 1, 0), -- Airport Transfer (ServiceId=1)
(3, NULL, 8, 1, 0), -- Late Checkout (ServiceId=8)
(3, NULL, 9, 5, 0); -- Wi-Fi Premium (ServiceId=9, 5 days)

-- Package 4: Family Fun Package
INSERT INTO PackageItems (PackageId, RoomTypeId, ServiceId, Quantity, IsDeleted) VALUES
(4, 4, NULL, 3, 0), -- 3 nights Family Room (RoomTypeId=4)
(4, NULL, 2, 3, 0), -- Breakfast Buffet (ServiceId=2, 3 days)
(4, NULL, 6, 1, 0), -- City Tour (ServiceId=6)
(4, NULL, 12, 3, 0); -- Swimming Pool Access (ServiceId=12, 3 days)

-- Package 5: Beach Paradise
INSERT INTO PackageItems (PackageId, RoomTypeId, ServiceId, Quantity, IsDeleted) VALUES
(5, 16, NULL, 4, 0), -- 4 nights Beachfront Standard (RoomTypeId=16)
(5, NULL, 2, 4, 0), -- Breakfast Buffet (ServiceId=2, 4 days)
(5, NULL, 5, 1, 0), -- Spa Treatment (ServiceId=5)
(5, NULL, 7, 4, 0); -- Car Rental (ServiceId=7, 4 days)

-- Package 6: Heritage Experience
INSERT INTO PackageItems (PackageId, RoomTypeId, ServiceId, Quantity, IsDeleted) VALUES
(6, 8, NULL, 3, 0), -- 3 nights Heritage Double Room (RoomTypeId=8)
(6, NULL, 6, 1, 0), -- City Tour (ServiceId=6, heritage tour)
(6, NULL, 2, 3, 0); -- Breakfast Buffet (ServiceId=2, 3 days)

-- Package 7: Weekend Escape
INSERT INTO PackageItems (PackageId, RoomTypeId, ServiceId, Quantity, IsDeleted) VALUES
(7, 1, NULL, 2, 0), -- 2 nights Standard Single Room (RoomTypeId=1)
(7, NULL, 2, 2, 0), -- Breakfast Buffet (ServiceId=2, 2 days)
(7, NULL, 8, 1, 0); -- Late Checkout (ServiceId=8)

-- Package 8: Luxury Indulgence
INSERT INTO PackageItems (PackageId, RoomTypeId, ServiceId, Quantity, IsDeleted) VALUES
(8, 3, NULL, 3, 0), -- 3 nights Executive Suite (RoomTypeId=3)
(8, NULL, 5, 1, 0), -- Spa Treatment (ServiceId=5)
(8, NULL, 1, 1, 0), -- Airport Transfer (ServiceId=1)
(8, NULL, 2, 3, 0); -- Breakfast Buffet (ServiceId=2, 3 days)

-- Package 9: Adventure Package
INSERT INTO PackageItems (PackageId, RoomTypeId, ServiceId, Quantity, IsDeleted) VALUES
(9, 1, NULL, 3, 0), -- 3 nights Standard Single Room (RoomTypeId=1)
(9, NULL, 7, 3, 0), -- Car Rental (ServiceId=7, 3 days)
(9, NULL, 6, 1, 0), -- City Tour (ServiceId=6)
(9, NULL, 11, 3, 0); -- Gym Access (ServiceId=11, 3 days)

-- Package 10: Extended Stay
INSERT INTO PackageItems (PackageId, RoomTypeId, ServiceId, Quantity, IsDeleted) VALUES
(10, 1, NULL, 7, 0), -- 7 nights Standard Single Room (RoomTypeId=1)
(10, NULL, 2, 7, 0), -- Breakfast Buffet (ServiceId=2, 7 days)
(10, NULL, 3, 2, 0), -- Laundry Service (ServiceId=3, 2 times)
(10, NULL, 8, 1, 0); -- Late Checkout (ServiceId=8)
GO

-- K. PROMOTIONS (10+ Promotions)
PRINT 'Inserting Promotions...'
-- Type: 0=Percentage, 1=FixedAmount
INSERT INTO Promotions (Code, Description, Type, Value, StartDate, EndDate, IsActive, LimitPerPhoneNumber, LimitPerPaymentCard, LimitPerDevice, LimitPerUserAccount, MaxUsesPerLimit, IsDeleted) VALUES
('WELCOME10', '10% off your first booking', 0, 10, DATEADD(day, -30, GETDATE()), DATEADD(day, 365, GETDATE()), 1, 1, 1, 1, 1, 1, 0),
('SUMMER20', '20% off summer bookings', 0, 20, DATEADD(day, -30, GETDATE()), DATEADD(day, 60, GETDATE()), 1, 1, 0, 0, 1, 1, 0),
('WEEKEND15', '15% off weekend stays', 0, 15, DATEADD(day, -10, GETDATE()), DATEADD(day, 90, GETDATE()), 1, 0, 0, 0, 1, 1, 0),
('FAMILY25', '25% off family bookings (4+ guests)', 0, 25, DATEADD(day, -20, GETDATE()), DATEADD(day, 120, GETDATE()), 1, 1, 1, 0, 1, 1, 0),
('SAVE50', 'RM50 off bookings over RM500', 1, 50, DATEADD(day, -15, GETDATE()), DATEADD(day, 60, GETDATE()), 1, 1, 1, 1, 1, 1, 0),
('LONGSTAY30', '30% off stays of 7+ nights', 0, 30, DATEADD(day, -10, GETDATE()), DATEADD(day, 180, GETDATE()), 1, 1, 0, 0, 1, 1, 0),
('NEWYEAR2024', 'RM100 off New Year bookings', 1, 100, DATEADD(day, -5, GETDATE()), DATEADD(day, 30, GETDATE()), 1, 1, 1, 1, 1, 1, 0),
('BUSINESS20', '20% off business bookings', 0, 20, DATEADD(day, -25, GETDATE()), DATEADD(day, 90, GETDATE()), 1, 0, 0, 0, 1, 1, 0),
('EARLYBIRD15', '15% off early bookings (30+ days advance)', 0, 15, DATEADD(day, -20, GETDATE()), DATEADD(day, 150, GETDATE()), 1, 1, 0, 0, 1, 1, 0),
('LOYALTY10', '10% off for returning customers', 0, 10, DATEADD(day, -30, GETDATE()), DATEADD(day, 365, GETDATE()), 1, 0, 0, 0, 1, 1, 0);
GO

-- ============================================
-- 4. INSERT BOOKING DATA (10+ Bookings)
-- ============================================
PRINT 'Inserting Bookings...'

-- Get User IDs dynamically
DECLARE @UserA INT = (SELECT UserId FROM Users WHERE Email = 'ahmad@example.com');
DECLARE @UserB INT = (SELECT UserId FROM Users WHERE Email = 'siti@example.com');
DECLARE @UserC INT = (SELECT UserId FROM Users WHERE Email = 'charlie@example.com');
DECLARE @UserD INT = (SELECT UserId FROM Users WHERE Email = 'sarah@example.com');
DECLARE @UserE INT = (SELECT UserId FROM Users WHERE Email = 'ali@example.com');
DECLARE @UserF INT = (SELECT UserId FROM Users WHERE Email = 'lisa@example.com');
DECLARE @UserG INT = (SELECT UserId FROM Users WHERE Email = 'david@example.com');
DECLARE @UserH INT = (SELECT UserId FROM Users WHERE Email = 'nurul@example.com');
DECLARE @UserI INT = (SELECT UserId FROM Users WHERE Email = 'james@example.com');
DECLARE @UserJ INT = (SELECT UserId FROM Users WHERE Email = 'fatimah@example.com');

-- Verify users exist
IF @UserA IS NULL OR @UserB IS NULL OR @UserC IS NULL OR @UserD IS NULL OR @UserE IS NULL OR @UserF IS NULL OR @UserG IS NULL OR @UserH IS NULL OR @UserI IS NULL OR @UserJ IS NULL
BEGIN
    PRINT 'ERROR: One or more required users are missing. Cannot proceed with booking inserts.';
    RETURN;
END

-- Get Room IDs (first room of each room type)
DECLARE @Room1 INT = 1;   -- Standard Single Room
DECLARE @Room2 INT = 11;  -- Deluxe Double Room
DECLARE @Room3 INT = 21;  -- Executive Suite
DECLARE @Room4 INT = 31;  -- Superior Room
DECLARE @Room5 INT = 41;  -- Deluxe Room

-- Status: 0=Pending, 1=Confirmed, 2=Cancelled, 3=CheckedIn, 4=CheckedOut, 5=NoShow
-- PaymentMethod: 0=CreditCard, 1=PayPal, 2=BankTransfer
-- PaymentStatus: 0=Pending, 1=Completed, 2=Failed, 3=Refunded
-- Source: 0=Direct, 1=OTA, 2=Group, 3=Phone, 4=WalkIn

-- Recent bookings (last 7 days) for revenue trends
INSERT INTO Bookings (UserId, RoomId, CheckInDate, CheckOutDate, BookingDate, TotalPrice, Status, PaymentAmount, PaymentMethod, PaymentStatus, PaymentDate, Source, QRToken, TransactionId, IsDeleted) VALUES
(@UserA, @Room1, DATEADD(day, -6, GETDATE()), DATEADD(day, -5, GETDATE()), DATEADD(day, -20, GETDATE()), 79.99, 4, 79.99, 0, 1, DATEADD(day, -6, GETDATE()), 0, NEWID(), 'TXN001', 0),
(@UserB, @Room2, DATEADD(day, -6, GETDATE()), DATEADD(day, -4, GETDATE()), DATEADD(day, -20, GETDATE()), 259.98, 4, 259.98, 1, 1, DATEADD(day, -6, GETDATE()), 0, NEWID(), 'TXN002', 0),
(@UserC, @Room3, DATEADD(day, -5, GETDATE()), DATEADD(day, -3, GETDATE()), DATEADD(day, -15, GETDATE()), 399.98, 4, 399.98, 0, 1, DATEADD(day, -5, GETDATE()), 0, NEWID(), 'TXN003', 0),
(@UserA, @Room1, DATEADD(day, -5, GETDATE()), DATEADD(day, -4, GETDATE()), DATEADD(day, -10, GETDATE()), 79.99, 4, 79.99, 2, 1, DATEADD(day, -5, GETDATE()), 0, NEWID(), 'TXN004', 0),
(@UserB, @Room2, DATEADD(day, -5, GETDATE()), DATEADD(day, -4, GETDATE()), DATEADD(day, -10, GETDATE()), 129.99, 4, 129.99, 0, 1, DATEADD(day, -5, GETDATE()), 0, NEWID(), 'TXN005', 0),
(@UserA, @Room1, DATEADD(day, -4, GETDATE()), DATEADD(day, -3, GETDATE()), DATEADD(day, -8, GETDATE()), 79.99, 4, 79.99, 0, 1, DATEADD(day, -4, GETDATE()), 0, NEWID(), 'TXN006', 0),
(@UserB, @Room2, DATEADD(day, -3, GETDATE()), DATEADD(day, -1, GETDATE()), DATEADD(day, -5, GETDATE()), 259.98, 3, 259.98, 1, 1, DATEADD(day, -3, GETDATE()), 0, NEWID(), 'TXN007', 0),
(@UserC, @Room2, DATEADD(day, -2, GETDATE()), GETDATE(), DATEADD(day, -4, GETDATE()), 259.98, 3, 259.98, 0, 1, DATEADD(day, -2, GETDATE()), 0, NEWID(), 'TXN008', 0),
(@UserA, @Room1, DATEADD(day, -2, GETDATE()), DATEADD(day, -1, GETDATE()), DATEADD(day, -4, GETDATE()), 79.99, 3, 79.99, 0, 1, DATEADD(day, -2, GETDATE()), 1, NEWID(), 'TXN009', 0),
(@UserB, @Room3, DATEADD(day, -1, GETDATE()), DATEADD(day, 1, GETDATE()), DATEADD(day, -3, GETDATE()), 399.98, 1, 399.98, 0, 1, DATEADD(day, -1, GETDATE()), 0, NEWID(), 'TXN010', 0),
(@UserC, @Room1, DATEADD(day, -1, GETDATE()), GETDATE(), DATEADD(day, -3, GETDATE()), 79.99, 1, 79.99, 2, 1, DATEADD(day, -1, GETDATE()), 0, NEWID(), 'TXN011', 0),
(@UserA, @Room2, DATEADD(day, -1, GETDATE()), DATEADD(day, 1, GETDATE()), DATEADD(day, -3, GETDATE()), 259.98, 1, 259.98, 0, 1, DATEADD(day, -1, GETDATE()), 0, NEWID(), 'TXN012', 0),
(@UserA, @Room3, GETDATE(), DATEADD(day, 2, GETDATE()), DATEADD(day, -1, GETDATE()), 399.98, 1, 399.98, 0, 1, GETDATE(), 0, NEWID(), 'TXN013', 0),
(@UserB, @Room2, GETDATE(), DATEADD(day, 2, GETDATE()), DATEADD(day, -1, GETDATE()), 259.98, 1, 259.98, 0, 1, GETDATE(), 0, NEWID(), 'TXN014', 0),
(@UserC, @Room2, GETDATE(), DATEADD(day, 2, GETDATE()), DATEADD(day, -1, GETDATE()), 259.98, 1, 259.98, 1, 1, GETDATE(), 0, NEWID(), 'TXN015', 0),
(@UserA, @Room1, GETDATE(), DATEADD(day, 1, GETDATE()), DATEADD(day, -1, GETDATE()), 79.99, 1, 79.99, 0, 1, GETDATE(), 0, NEWID(), 'TXN016', 0);

-- Pending bookings (future dates)
INSERT INTO Bookings (UserId, RoomId, CheckInDate, CheckOutDate, BookingDate, TotalPrice, Status, PaymentAmount, PaymentMethod, PaymentStatus, Source, IsDeleted) VALUES
(@UserD, @Room4, DATEADD(day, 10, GETDATE()), DATEADD(day, 14, GETDATE()), GETDATE(), 599.96, 0, NULL, 0, 0, 0, 0),
(@UserB, @Room3, DATEADD(day, 20, GETDATE()), DATEADD(day, 22, GETDATE()), GETDATE(), 399.98, 0, NULL, 0, 0, 1, 0),
(@UserC, @Room1, DATEADD(day, 5, GETDATE()), DATEADD(day, 8, GETDATE()), GETDATE(), 239.97, 0, NULL, 0, 0, 0, 0);

-- Cancelled bookings
INSERT INTO Bookings (UserId, RoomId, CheckInDate, CheckOutDate, BookingDate, TotalPrice, Status, PaymentAmount, PaymentMethod, PaymentStatus, CancellationDate, CancellationReason, Source, IsDeleted) VALUES
(@UserA, @Room2, DATEADD(day, -20, GETDATE()), DATEADD(day, -18, GETDATE()), DATEADD(day, -30, GETDATE()), 259.98, 2, 0, 0, 0, DATEADD(day, -25, GETDATE()), 'Change of plans', 0, 0),
(@UserD, @Room1, DATEADD(day, -10, GETDATE()), DATEADD(day, -8, GETDATE()), DATEADD(day, -15, GETDATE()), 159.98, 2, 0, 0, 0, DATEADD(day, -12, GETDATE()), 'Sick', 1, 0);

-- Historical bookings (checked out)
INSERT INTO Bookings (UserId, RoomId, CheckInDate, CheckOutDate, BookingDate, TotalPrice, Status, PaymentAmount, PaymentMethod, PaymentStatus, PaymentDate, Source, QRToken, TransactionId, IsDeleted) VALUES
(@UserB, @Room3, DATEADD(day, -30, GETDATE()), DATEADD(day, -28, GETDATE()), DATEADD(day, -40, GETDATE()), 399.98, 4, 399.98, 0, 1, DATEADD(day, -30, GETDATE()), 0, NEWID(), 'TXN017', 0),
(@UserC, @Room4, DATEADD(day, -35, GETDATE()), DATEADD(day, -32, GETDATE()), DATEADD(day, -45, GETDATE()), 449.97, 4, 449.97, 1, 1, DATEADD(day, -35, GETDATE()), 0, NEWID(), 'TXN018', 0),
(@UserA, @Room2, DATEADD(day, -40, GETDATE()), DATEADD(day, -38, GETDATE()), DATEADD(day, -50, GETDATE()), 259.98, 4, 259.98, 2, 1, DATEADD(day, -40, GETDATE()), 0, NEWID(), 'TXN019', 0),
(@UserE, @Room1, DATEADD(day, -25, GETDATE()), DATEADD(day, -23, GETDATE()), DATEADD(day, -35, GETDATE()), 159.98, 4, 159.98, 0, 1, DATEADD(day, -25, GETDATE()), 0, NEWID(), 'TXN020', 0),
(@UserF, @Room2, DATEADD(day, -20, GETDATE()), DATEADD(day, -18, GETDATE()), DATEADD(day, -30, GETDATE()), 259.98, 4, 259.98, 1, 1, DATEADD(day, -20, GETDATE()), 0, NEWID(), 'TXN021', 0),
(@UserG, @Room3, DATEADD(day, -15, GETDATE()), DATEADD(day, -13, GETDATE()), DATEADD(day, -25, GETDATE()), 399.98, 4, 399.98, 0, 1, DATEADD(day, -15, GETDATE()), 0, NEWID(), 'TXN022', 0),
(@UserH, @Room1, DATEADD(day, -10, GETDATE()), DATEADD(day, -8, GETDATE()), DATEADD(day, -20, GETDATE()), 159.98, 4, 159.98, 2, 1, DATEADD(day, -10, GETDATE()), 0, NEWID(), 'TXN023', 0),
(@UserI, @Room2, DATEADD(day, -28, GETDATE()), DATEADD(day, -26, GETDATE()), DATEADD(day, -38, GETDATE()), 259.98, 4, 259.98, 0, 1, DATEADD(day, -28, GETDATE()), 0, NEWID(), 'TXN024', 0),
(@UserJ, @Room4, DATEADD(day, -22, GETDATE()), DATEADD(day, -19, GETDATE()), DATEADD(day, -32, GETDATE()), 449.97, 4, 449.97, 1, 1, DATEADD(day, -22, GETDATE()), 0, NEWID(), 'TXN025', 0);
GO

-- ============================================
-- 5. INSERT REVIEWS (10+ Reviews)
-- ============================================
PRINT 'Inserting Reviews...'
-- Reviews are linked only to Bookings (not directly to Users)
-- Rating scale: 1=Poor, 2=Fair, 3=Good, 4=Very Good, 5=Excellent

-- Insert reviews for checked-out bookings
INSERT INTO Reviews (BookingId, Rating, Comment, ReviewDate, IsDeleted)
SELECT 
    BookingId,
    CASE 
        WHEN BookingId % 5 = 0 THEN 5  -- Excellent
        WHEN BookingId % 5 = 1 THEN 4  -- Very Good
        WHEN BookingId % 5 = 2 THEN 4  -- Very Good
        WHEN BookingId % 5 = 3 THEN 3  -- Good
        ELSE 5  -- Excellent (default)
    END AS Rating,
    CASE 
        WHEN BookingId % 5 = 0 THEN 'Absolutely amazing experience! The room was perfect and the service was outstanding. Highly recommend!'
        WHEN BookingId % 5 = 1 THEN 'Great stay! Clean room, comfortable bed, and excellent location. Will definitely come back.'
        WHEN BookingId % 5 = 2 THEN 'Very good hotel with friendly staff. The amenities were nice and the room was spacious.'
        WHEN BookingId % 5 = 3 THEN 'Good value for money. Room was clean and basic amenities were provided. Could use some improvements.'
        ELSE 'Perfect stay! Everything exceeded our expectations. The staff was helpful and the location was convenient.'
    END AS Comment,
    DATEADD(day, -ABS(CHECKSUM(NEWID()) % 5), GETDATE()) AS ReviewDate,
    0 AS IsDeleted
FROM Bookings 
WHERE Status = 4  -- Only checked-out bookings
AND BookingId NOT IN (SELECT BookingId FROM Reviews);  -- Don't duplicate existing reviews

DECLARE @ReviewCount INT = @@ROWCOUNT;
PRINT 'Inserted ' + CAST(@ReviewCount AS VARCHAR(10)) + ' reviews for checked-out bookings.';
GO

-- ============================================
-- 6. INSERT CONTACT MESSAGES (10+ Messages)
-- ============================================
PRINT 'Inserting Contact Messages...'
-- ContactMessage uses SentAt (not CreatedAt) and IsRead (defaults to false)
INSERT INTO ContactMessages (Name, Email, Subject, Message, SentAt, IsRead, IsDeleted) VALUES
('John Doe', 'john@example.com', 'Room Availability Inquiry', 'Hi, I would like to know about room availability for next month.', GETDATE(), 0, 0),
('Jane Smith', 'jane@example.com', 'Package Information', 'Can you provide more details about the Romantic Getaway package?', DATEADD(day, -1, GETDATE()), 0, 0),
('Mike Johnson', 'mike@example.com', 'Cancellation Policy', 'What is your cancellation policy for bookings?', DATEADD(day, -2, GETDATE()), 0, 0),
('Sarah Williams', 'sarah@example.com', 'Group Booking', 'I need to book 5 rooms for a group. Do you offer group discounts?', DATEADD(day, -3, GETDATE()), 0, 0),
('David Brown', 'david@example.com', 'Amenities Question', 'Does the hotel have a swimming pool and gym?', DATEADD(day, -4, GETDATE()), 0, 0),
('Emily Davis', 'emily@example.com', 'Check-in Time', 'What is the earliest check-in time?', DATEADD(day, -5, GETDATE()), 0, 0),
('Robert Wilson', 'robert@example.com', 'Parking Availability', 'Is parking available at the hotel?', DATEADD(day, -6, GETDATE()), 0, 0),
('Lisa Anderson', 'lisa@example.com', 'Wi-Fi Speed', 'What is the Wi-Fi speed in the rooms?', DATEADD(day, -7, GETDATE()), 0, 0),
('Michael Taylor', 'michael@example.com', 'Pet Policy', 'Are pets allowed in the hotel?', DATEADD(day, -8, GETDATE()), 0, 0),
('Olivia Martinez', 'olivia@example.com', 'Special Requests', 'Can I request a room with a view?', DATEADD(day, -9, GETDATE()), 0, 0);
GO

-- ============================================
-- 7. INSERT NEWSLETTER SUBSCRIPTIONS (10+ Subscriptions)
-- ============================================
PRINT 'Inserting Newsletter Subscriptions...'
-- Newsletter requires IsActive column (defaults to true for active subscriptions)
INSERT INTO Newsletters (Email, SubscribedAt, IsActive, IsDeleted) VALUES
('ahmad@example.com', DATEADD(day, -30, GETDATE()), 1, 0),
('siti@example.com', DATEADD(day, -25, GETDATE()), 1, 0),
('charlie@example.com', DATEADD(day, -20, GETDATE()), 1, 0),
('sarah@example.com', DATEADD(day, -15, GETDATE()), 1, 0),
('ali@example.com', DATEADD(day, -10, GETDATE()), 1, 0),
('lisa@example.com', DATEADD(day, -5, GETDATE()), 1, 0),
('david@example.com', DATEADD(day, -3, GETDATE()), 1, 0),
('nurul@example.com', DATEADD(day, -2, GETDATE()), 1, 0),
('james@example.com', DATEADD(day, -1, GETDATE()), 1, 0),
('fatimah@example.com', GETDATE(), 1, 0);
GO

PRINT '============================================'
PRINT 'DATABASE SETUP COMPLETE!'
PRINT '============================================'
PRINT 'Summary:'
PRINT '- 10 Hotels (each with at least 3 room types)'
PRINT '- 36 Users (1 Admin + 10 Managers + 10 Staff + 15 Customers)'
PRINT '- 15 Amenities'
PRINT '- 30 Room Types (3 per hotel)'
PRINT '- 300 Rooms (10 per room type)'
PRINT '- 12 Services'
PRINT '- 10 Packages (with PackageItems)'
PRINT '- 10 Promotions'
PRINT '- 30+ Bookings (various statuses)'
PRINT '- 20+ Reviews'
PRINT '- 10 Contact Messages'
PRINT '- 10 Newsletter Subscriptions'
PRINT '============================================'
GO
