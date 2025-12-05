-- ============================================
-- Complete New Database Script
-- Budget Hotel Reservation System
-- Updated with Role-Based Access Control
-- ============================================
-- This script populates a fresh database with sample data
-- INCLUDING the new HotelId field for Manager/Staff hotel assignment
-- 
-- ⚠️ IMPORTANT: You MUST run migrations FIRST! ⚠️
-- 
-- BEFORE running this script:
-- 1. Open terminal in the Assignment folder
-- 2. Run: dotnet ef database update
-- 3. This will create the database and all tables
-- 
-- THEN run this script to populate with sample data
-- 
-- This script will:
-- 1. Check if database and tables exist
-- 2. Clear all existing data (if tables exist)
-- 3. Reset identity seeds to start from 1
-- 4. Insert fresh sample data
-- 5. Assign hotels to Manager and Staff users (each hotel has 1 Manager + 1 Staff)
-- 
-- ROLE-BASED ACCESS CONTROL:
-- - Admin: Can see all hotels, create hotels/users, full access
-- - Manager: Can only see/manage their assigned hotel, can create room types/rooms/packages/promotions
-- - Staff: Can only see/manage their assigned hotel, limited to bookings and viewing
-- - Customer: Can only book hotels and manage their own bookings
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
    PRINT 'Please create the database first by running:'
    PRINT '  dotnet ef database update'
    PRINT ''
    PRINT 'Script execution stopped.'
    SET NOEXEC ON
END
GO

-- Check if required tables exist (only if database exists)
IF DB_ID('BMIT2023_HotelReservation') IS NOT NULL
BEGIN
    USE [BMIT2023_HotelReservation]
    
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Hotels' AND schema_id = SCHEMA_ID('dbo'))
       OR NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('dbo'))
       OR NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bookings' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        PRINT 'ERROR: Required tables do not exist in the database.'
        PRINT ''
        PRINT 'To create the tables, run:'
        PRINT '  dotnet ef database update'
        PRINT ''
        PRINT 'Script execution stopped.'
        SET NOEXEC ON
    END
    ELSE
    BEGIN
        PRINT 'Database and tables found. Proceeding with data insertion...'
        PRINT ''
    END
END
GO

-- ============================================
-- CLEAR ALL EXISTING DATA (in correct order)
-- ============================================
-- Only clear if tables exist
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Clearing existing data...'
    
    -- Delete in reverse order of dependencies
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews') DELETE FROM Reviews
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Bookings') DELETE FROM Bookings
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PackageItems') DELETE FROM PackageItems
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Packages') DELETE FROM Packages
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomTypeAmenities') DELETE FROM RoomTypeAmenities
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomImages') DELETE FROM RoomImages
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Rooms') DELETE FROM Rooms
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomTypes') DELETE FROM RoomTypes
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Services') DELETE FROM Services
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Amenities') DELETE FROM Amenities
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ContactMessages') DELETE FROM ContactMessages
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Newsletters') DELETE FROM Newsletters
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Promotions') DELETE FROM Promotions
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SecurityLogs') DELETE FROM SecurityLogs
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LoginAttempts') DELETE FROM LoginAttempts
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SecurityTokens') DELETE FROM SecurityTokens
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users') DELETE FROM Users
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Hotels') DELETE FROM Hotels
    
    -- Reset identity seeds to start from 1
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
    
    PRINT 'All existing data cleared and identity seeds reset.'
    PRINT ''
END
ELSE
BEGIN
    PRINT 'No existing data to clear (tables are empty or don''t exist).'
    PRINT ''
END
GO

-- ============================================
-- 1. INSERT HOTELS (15 hotels)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Hotels' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Hotels...'
    INSERT INTO Hotels (Name, Address, City, PostalCode, Country, ContactNumber, ContactEmail, Description, ImageUrl, IsDeleted, DeletedAt)
VALUES
('Budget Inn KL Sentral', '123 Jalan Tun Sambanthan', 'Kuala Lumpur', '50470', 'Malaysia', '+60-3-2274-0101', 'info@budgetinnkl.com', 'A comfortable budget hotel in the heart of KL Sentral. Perfect for business and leisure travelers.', NULL, 0, NULL),
('Economy Stay Bukit Bintang', '456 Jalan Bukit Bintang', 'Kuala Lumpur', '55100', 'Malaysia', '+60-3-2142-0202', 'contact@economystaybb.com', 'Affordable accommodation with modern amenities in the vibrant Bukit Bintang area.', NULL, 0, NULL),
('Penang Budget Hotel', '789 Jalan Penang', 'George Town', '10000', 'Malaysia', '+60-4-261-1234', 'info@penangbudget.com', 'Budget-friendly hotel in the heart of George Town, close to heritage sites and local food.', NULL, 0, NULL),
('Malacca City Inn', '321 Jalan Hang Jebat', 'Malacca', '75200', 'Malaysia', '+60-6-281-5678', 'contact@malaccacityinn.com', 'Affordable stay in historic Malacca, walking distance to Jonker Street and cultural attractions.', NULL, 0, NULL),
('Johor Bahru Budget Stay', '654 Jalan Wong Ah Fook', 'Johor Bahru', '80000', 'Malaysia', '+60-7-222-9012', 'info@jbbudgetstay.com', 'Convenient budget accommodation near JB city center, perfect for shopping and dining.', NULL, 0, NULL),
('Langkawi Beach Budget Hotel', '147 Pantai Cenang', 'Langkawi', '07000', 'Malaysia', '+60-4-955-3456', 'stay@langkawibudget.com', 'Budget beachfront hotel in Langkawi with stunning sea views and easy access to attractions.', NULL, 0, NULL),
('Ipoh Heritage Budget Inn', '258 Jalan Sultan Yussuf', 'Ipoh', '30000', 'Malaysia', '+60-5-241-7890', 'info@ipohheritage.com', 'Charming budget inn in the heart of Ipoh''s old town, surrounded by famous food stalls and heritage buildings.', NULL, 0, NULL),
('Kota Kinabalu Budget Hotel', '369 Jalan Gaya', 'Kota Kinabalu', '88000', 'Malaysia', '+60-88-234-5678', 'contact@kkbudget.com', 'Affordable accommodation in KK city center, close to waterfront and shopping areas.', NULL, 0, NULL),
('Kuching Riverside Budget Stay', '741 Jalan Main Bazaar', 'Kuching', '93000', 'Malaysia', '+60-82-456-7890', 'stay@kuchingbudget.com', 'Budget-friendly hotel along the Sarawak River, perfect for exploring Kuching''s cultural heritage.', NULL, 0, NULL),
('Cameron Highlands Budget Lodge', '852 Jalan Persiaran Camellia', 'Cameron Highlands', '39000', 'Malaysia', '+60-5-491-2345', 'info@cameronbudget.com', 'Cozy budget lodge in the cool highlands, surrounded by tea plantations and strawberry farms.', NULL, 0, NULL),
('Kuantan Beachfront Inn', '159 Jalan Teluk Cempedak', 'Kuantan', '25050', 'Malaysia', '+60-9-512-3456', 'info@kuantanbeach.com', 'Budget hotel with beach access in Kuantan, perfect for family vacations.', NULL, 0, NULL),
('Melaka Heritage Hotel', '963 Jalan Hang Tuah', 'Malacca', '75200', 'Malaysia', '+60-6-283-7890', 'heritage@melakahotel.com', 'Historic budget hotel in the UNESCO World Heritage city of Malacca.', NULL, 0, NULL),
('Terengganu Coastal Stay', '357 Jalan Sultan Zainal Abidin', 'Kuala Terengganu', '20000', 'Malaysia', '+60-9-622-1234', 'stay@terengganu.com', 'Budget accommodation near Terengganu beaches and cultural sites.', NULL, 0, NULL),
('Perak Budget Hotel', '741 Jalan Sultan Idris Shah', 'Ipoh', '30000', 'Malaysia', '+60-5-243-5678', 'info@perakbudget.com', 'Comfortable budget hotel in Ipoh city center with easy access to local attractions.', NULL, 0, NULL),
('Sabah Mountain View Inn', '852 Jalan Lintas', 'Kota Kinabalu', '88000', 'Malaysia', '+60-88-245-9012', 'mountain@kkhotel.com', 'Budget hotel with mountain views in Kota Kinabalu, close to Mount Kinabalu.', NULL, 0, NULL)
    PRINT 'Hotels inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: Hotels table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 2. INSERT USERS (43 users: 1 Admin + 15 Managers + 15 Staff + 12 Customers)
-- IMPORTANT: Each hotel has 1 Manager and 1 Staff assigned
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Users...'
    -- Note: PasswordHash is a placeholder. The application's DbInitializer will set proper passwords.
    -- Default passwords after app initialization:
    --   admin@hotel.com: Admin123!
    --   manager1@hotel.com through manager15@hotel.com: Manager123!
    --   staff1@hotel.com through staff15@hotel.com: Password123!
    INSERT INTO Users (Email, FullName, PasswordHash, PhoneNumber, Role, IsEmailVerified, IsActive, CreatedAt, ProfilePictureUrl, Bio, PreferredLanguage, Theme, HotelId, IsDeleted, DeletedAt)
VALUES
-- Admin (no hotel assignment - can see all hotels)
('admin@hotel.com', 'Admin Hotel', 'PLACEHOLDER_PASSWORD_HASH', NULL, 0, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', NULL, 0, NULL),
-- Managers (one for each hotel, HotelId 1-15)
('manager1@hotel.com', 'Manager Smith', 'PLACEHOLDER_PASSWORD_HASH', '+60-3-2274-0101', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 1, 0, NULL),
('manager2@hotel.com', 'Manager Lee', 'PLACEHOLDER_PASSWORD_HASH', '+60-3-2142-0202', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 2, 0, NULL),
('manager3@hotel.com', 'Manager Tan', 'PLACEHOLDER_PASSWORD_HASH', '+60-4-261-1234', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 3, 0, NULL),
('manager4@hotel.com', 'Manager Wong', 'PLACEHOLDER_PASSWORD_HASH', '+60-6-281-5678', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 4, 0, NULL),
('manager5@hotel.com', 'Manager Chen', 'PLACEHOLDER_PASSWORD_HASH', '+60-7-222-9012', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 5, 0, NULL),
('manager6@hotel.com', 'Manager Lim', 'PLACEHOLDER_PASSWORD_HASH', '+60-4-955-3456', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 6, 0, NULL),
('manager7@hotel.com', 'Manager Ahmad', 'PLACEHOLDER_PASSWORD_HASH', '+60-5-241-7890', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 7, 0, NULL),
('manager8@hotel.com', 'Manager Hassan', 'PLACEHOLDER_PASSWORD_HASH', '+60-88-234-5678', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 8, 0, NULL),
('manager9@hotel.com', 'Manager Ibrahim', 'PLACEHOLDER_PASSWORD_HASH', '+60-82-456-7890', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 9, 0, NULL),
('manager10@hotel.com', 'Manager Abdullah', 'PLACEHOLDER_PASSWORD_HASH', '+60-5-491-2345', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 10, 0, NULL),
('manager11@hotel.com', 'Manager Rahman', 'PLACEHOLDER_PASSWORD_HASH', '+60-9-512-3456', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 11, 0, NULL),
('manager12@hotel.com', 'Manager Ali', 'PLACEHOLDER_PASSWORD_HASH', '+60-6-283-7890', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 12, 0, NULL),
('manager13@hotel.com', 'Manager Bakar', 'PLACEHOLDER_PASSWORD_HASH', '+60-9-622-1234', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 13, 0, NULL),
('manager14@hotel.com', 'Manager Yusuf', 'PLACEHOLDER_PASSWORD_HASH', '+60-5-243-5678', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 14, 0, NULL),
('manager15@hotel.com', 'Manager Zainal', 'PLACEHOLDER_PASSWORD_HASH', '+60-88-245-9012', 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 15, 0, NULL),
-- Staff (one for each hotel, HotelId 1-15)
('staff1@hotel.com', 'Staff Johnson', 'PLACEHOLDER_PASSWORD_HASH', '+60-3-2274-0102', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 1, 0, NULL),
('staff2@hotel.com', 'Staff Williams', 'PLACEHOLDER_PASSWORD_HASH', '+60-3-2142-0203', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 2, 0, NULL),
('staff3@hotel.com', 'Staff Brown', 'PLACEHOLDER_PASSWORD_HASH', '+60-4-261-1235', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 3, 0, NULL),
('staff4@hotel.com', 'Staff Davis', 'PLACEHOLDER_PASSWORD_HASH', '+60-6-281-5679', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 4, 0, NULL),
('staff5@hotel.com', 'Staff Miller', 'PLACEHOLDER_PASSWORD_HASH', '+60-7-222-9013', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 5, 0, NULL),
('staff6@hotel.com', 'Staff Wilson', 'PLACEHOLDER_PASSWORD_HASH', '+60-4-955-3457', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 6, 0, NULL),
('staff7@hotel.com', 'Staff Moore', 'PLACEHOLDER_PASSWORD_HASH', '+60-5-241-7891', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 7, 0, NULL),
('staff8@hotel.com', 'Staff Taylor', 'PLACEHOLDER_PASSWORD_HASH', '+60-88-234-5679', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 8, 0, NULL),
('staff9@hotel.com', 'Staff Anderson', 'PLACEHOLDER_PASSWORD_HASH', '+60-82-456-7891', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 9, 0, NULL),
('staff10@hotel.com', 'Staff Thomas', 'PLACEHOLDER_PASSWORD_HASH', '+60-5-491-2346', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 10, 0, NULL),
('staff11@hotel.com', 'Staff Jackson', 'PLACEHOLDER_PASSWORD_HASH', '+60-9-512-3457', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 11, 0, NULL),
('staff12@hotel.com', 'Staff White', 'PLACEHOLDER_PASSWORD_HASH', '+60-6-283-7891', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 12, 0, NULL),
('staff13@hotel.com', 'Staff Harris', 'PLACEHOLDER_PASSWORD_HASH', '+60-9-622-1235', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 13, 0, NULL),
('staff14@hotel.com', 'Staff Martin', 'PLACEHOLDER_PASSWORD_HASH', '+60-5-243-5679', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 14, 0, NULL),
('staff15@hotel.com', 'Staff Thompson', 'PLACEHOLDER_PASSWORD_HASH', '+60-88-245-9013', 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 15, 0, NULL),
-- Customers (no hotel assignment)
('ahmad@example.com', 'Ahmad Zulkifli', 'PLACEHOLDER_PASSWORD_HASH', '+60-12-345-6789', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', NULL, 0, NULL),
('siti@example.com', 'Siti Nurhaliza', 'PLACEHOLDER_PASSWORD_HASH', '+60-19-876-5432', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', NULL, 0, NULL),
('charlie@example.com', 'Charlie Brown', 'PLACEHOLDER_PASSWORD_HASH', NULL, 3, 0, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', NULL, 0, NULL),
('sarah@example.com', 'Sarah Tan', 'PLACEHOLDER_PASSWORD_HASH', '+60-16-123-4567', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', NULL, 0, NULL),
('ali@example.com', 'Mohammad Ali', 'PLACEHOLDER_PASSWORD_HASH', '+60-17-234-5678', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', NULL, 0, NULL),
('lisa@example.com', 'Lisa Wong', 'PLACEHOLDER_PASSWORD_HASH', '+60-18-345-6789', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', NULL, 0, NULL),
('david@example.com', 'David Lee', 'PLACEHOLDER_PASSWORD_HASH', '+60-19-456-7890', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', NULL, 0, NULL),
('nurul@example.com', 'Nurul Aisyah', 'PLACEHOLDER_PASSWORD_HASH', '+60-13-567-8901', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', NULL, 0, NULL),
('james@example.com', 'James Lim', 'PLACEHOLDER_PASSWORD_HASH', '+60-14-678-9012', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', NULL, 0, NULL),
('fatimah@example.com', 'Fatimah Zahra', 'PLACEHOLDER_PASSWORD_HASH', '+60-15-789-0123', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', NULL, 0, NULL),
('kevin@example.com', 'Kevin Chen', 'PLACEHOLDER_PASSWORD_HASH', '+60-11-890-1234', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', NULL, 0, NULL),
('amira@example.com', 'Amira Hassan', 'PLACEHOLDER_PASSWORD_HASH', '+60-10-901-2345', 3, 0, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', NULL, 0, NULL)
    PRINT 'Users inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: Users table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 3. INSERT AMENITIES (15 amenities)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Amenities' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Amenities...'
    INSERT INTO Amenities (Name, ImageUrl, IsDeleted, DeletedAt)
VALUES
('Free Wi-Fi', 'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=100&h=100&fit=crop', 0, NULL),
('Air Conditioning', 'https://images.unsplash.com/photo-1621905251918-48116d1b5b0a?w=100&h=100&fit=crop', 0, NULL),
('Flat-Screen TV', 'https://images.unsplash.com/photo-1593359677879-a4bb92f829d1?w=100&h=100&fit=crop', 0, NULL),
('Mini Fridge', 'https://images.unsplash.com/photo-1571171637578-41bc2dd41cd2?w=100&h=100&fit=crop', 0, NULL),
('Coffee Maker', 'https://images.unsplash.com/photo-1517487881594-2787fef5ebf7?w=100&h=100&fit=crop', 0, NULL),
('Private Bathroom', 'https://images.unsplash.com/photo-1631889993954-2b0e5b6c6a6b?w=100&h=100&fit=crop', 0, NULL),
('Room Service', 'https://images.unsplash.com/photo-1555396273-367ea4eb4db5?w=100&h=100&fit=crop', 0, NULL),
('Safe', 'https://images.unsplash.com/photo-1586075010923-2dd4570fb338?w=100&h=100&fit=crop', 0, NULL),
('Balcony', 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=100&h=100&fit=crop', 0, NULL),
('City View', 'https://images.unsplash.com/photo-1514565131-fce0801e5785?w=100&h=100&fit=crop', 0, NULL),
('Ocean View', 'https://images.unsplash.com/photo-1505142468610-359e7d316be0?w=100&h=100&fit=crop', 0, NULL),
('Swimming Pool', 'https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=100&h=100&fit=crop', 0, NULL),
('Gym Access', 'https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=100&h=100&fit=crop', 0, NULL),
('Parking', 'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=100&h=100&fit=crop', 0, NULL),
('Laundry Service', 'https://images.unsplash.com/photo-1586075010923-2dd4570fb338?w=100&h=100&fit=crop', 0, NULL)
    PRINT 'Amenities inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: Amenities table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 4. INSERT ROOM TYPES (20 room types)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomTypes' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Room Types...'
    INSERT INTO RoomTypes (Name, Description, Occupancy, BasePrice, HotelId, IsDeleted, DeletedAt)
VALUES
('Standard Single Room', 'A cozy room for a single traveler with essential amenities. Perfect for solo travelers on a budget.', 1, 79.99, 1, 0, NULL),
('Deluxe Double Room', 'Spacious room with two double beds. Ideal for families or groups of up to 4 people.', 4, 129.99, 2, 0, NULL),
('Executive Suite', 'Luxurious suite with a king bed and city view. Includes premium amenities and extra space.', 2, 199.99, 3, 0, NULL),
('Family Room', 'Large family-friendly room with multiple beds and extra space for children.', 6, 159.99, 4, 0, NULL),
('Premier Twin Room', 'Bright twin room with modern furnishings, perfect for friends or business travelers.', 2, 109.99, 5, 0, NULL),
('Studio Apartment', 'Fully furnished studio with kitchenette ideal for long stays and remote work.', 3, 149.99, 2, 0, NULL),
('Ocean View Suite', 'Premium suite with panoramic ocean views, private balcony, and lounge area.', 2, 229.99, 6, 0, NULL),
('Heritage Double Room', 'Charming room with colonial-style furnishings in the heart of historic Ipoh.', 2, 99.99, 7, 0, NULL),
('Sabah View Room', 'Comfortable room with mountain or sea view, perfect for exploring Kota Kinabalu.', 2, 119.99, 8, 0, NULL),
('Riverside Deluxe', 'Spacious room overlooking the Sarawak River with modern amenities.', 3, 139.99, 9, 0, NULL),
('Highland Cozy Room', 'Warm and comfortable room perfect for the cool highland climate.', 2, 89.99, 10, 0, NULL),
('Beachfront Standard', 'Budget room with direct beach access and sea breeze.', 2, 149.99, 11, 0, NULL),
('Historic Suite', 'Elegant suite in a heritage building with period furnishings.', 2, 179.99, 12, 0, NULL),
('Coastal View Room', 'Room with stunning coastal views and modern amenities.', 2, 129.99, 13, 0, NULL),
('City Center Standard', 'Convenient room in the heart of the city with easy access to attractions.', 2, 99.99, 14, 0, NULL),
('Mountain View Deluxe', 'Spacious room with breathtaking mountain views.', 2, 139.99, 15, 0, NULL),
('Budget Single', 'Basic single room with essential amenities for budget-conscious travelers.', 1, 69.99, 1, 0, NULL),
('Triple Room', 'Room with three single beds, perfect for groups of three.', 3, 119.99, 2, 0, NULL),
('Quad Room', 'Room with four single beds, ideal for families or groups.', 4, 139.99, 3, 0, NULL),
('Penthouse Suite', 'Luxurious top-floor suite with panoramic views and premium amenities.', 4, 299.99, 4, 0, NULL)
    PRINT 'Room Types inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: RoomTypes table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 5. INSERT ROOMS (25 rooms)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Rooms' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Rooms...'
    INSERT INTO Rooms (RoomNumber, RoomTypeId, Status, IsDeleted, DeletedAt)
VALUES
('101', 1, 0, 0, NULL), ('102', 1, 0, 0, NULL), ('103', 1, 0, 0, NULL),
('201', 2, 0, 0, NULL), ('202', 2, 0, 0, NULL), ('203', 2, 0, 0, NULL),
('301', 3, 0, 0, NULL), ('302', 3, 0, 0, NULL),
('401', 4, 0, 0, NULL), ('402', 4, 0, 0, NULL),
('501', 5, 0, 0, NULL), ('502', 5, 0, 0, NULL),
('601', 6, 0, 0, NULL), ('602', 6, 0, 0, NULL),
('701', 7, 0, 0, NULL), ('702', 7, 0, 0, NULL),
('801', 8, 0, 0, NULL), ('802', 8, 0, 0, NULL),
('901', 9, 0, 0, NULL), ('902', 9, 0, 0, NULL),
('1001', 10, 0, 0, NULL), ('1002', 10, 0, 0, NULL),
('1101', 11, 0, 0, NULL), ('1102', 11, 0, 0, NULL),
('1201', 12, 0, 0, NULL)
    PRINT 'Rooms inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: Rooms table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 6. INSERT ROOM IMAGES (30 images)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomImages' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Room Images...'
    INSERT INTO RoomImages (RoomTypeId, ImageUrl, Caption, IsDeleted, DeletedAt)
VALUES
(1, 'https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?w=800&h=600&fit=crop', 'Standard Single Room', 0, NULL),
(1, 'https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=800&h=600&fit=crop', 'Standard Single Room View', 0, NULL),
(2, 'https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=800&h=600&fit=crop', 'Deluxe Double Room', 0, NULL),
(2, 'https://images.unsplash.com/photo-1466978913421-dad2ebd01d17?w=800&h=600&fit=crop', 'Deluxe Double Room Interior', 0, NULL),
(3, 'https://images.unsplash.com/photo-1505691938895-1758d7feb511?w=800&h=600&fit=crop', 'Executive Suite', 0, NULL),
(3, 'https://images.unsplash.com/photo-1523217582562-09d0def993a6?w=800&h=600&fit=crop', 'Executive Suite Living Area', 0, NULL),
(4, 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800&h=600&fit=crop', 'Family Room', 0, NULL),
(4, 'https://images.unsplash.com/photo-1496417263034-38ec4f0b665a?w=800&h=600&fit=crop', 'Family Room Beds', 0, NULL),
(5, 'https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?w=800&h=600&fit=crop', 'Premier Twin Room', 0, NULL),
(6, 'https://images.unsplash.com/photo-1523217582562-09d0def993a6?w=800&h=600&fit=crop', 'Studio Apartment', 0, NULL),
(7, 'https://images.unsplash.com/photo-1496417263034-38ec4f0b665a?w=800&h=600&fit=crop', 'Ocean View Suite', 0, NULL),
(7, 'https://images.unsplash.com/photo-1505142468610-359e7d316be0?w=800&h=600&fit=crop', 'Ocean View Suite Balcony', 0, NULL),
(8, 'https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?w=800&h=600&fit=crop', 'Heritage Double Room', 0, NULL),
(9, 'https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=800&h=600&fit=crop', 'Sabah View Room', 0, NULL),
(10, 'https://images.unsplash.com/photo-1505691938895-1758d7feb511?w=800&h=600&fit=crop', 'Riverside Deluxe', 0, NULL),
(11, 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800&h=600&fit=crop', 'Highland Cozy Room', 0, NULL),
(12, 'https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?w=800&h=600&fit=crop', 'Beachfront Standard', 0, NULL),
(13, 'https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=800&h=600&fit=crop', 'Historic Suite', 0, NULL),
(14, 'https://images.unsplash.com/photo-1505691938895-1758d7feb511?w=800&h=600&fit=crop', 'Coastal View Room', 0, NULL),
(15, 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800&h=600&fit=crop', 'City Center Standard', 0, NULL),
(16, 'https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?w=800&h=600&fit=crop', 'Mountain View Deluxe', 0, NULL),
(17, 'https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=800&h=600&fit=crop', 'Budget Single', 0, NULL),
(18, 'https://images.unsplash.com/photo-1505691938895-1758d7feb511?w=800&h=600&fit=crop', 'Triple Room', 0, NULL),
(19, 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800&h=600&fit=crop', 'Quad Room', 0, NULL),
(20, 'https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?w=800&h=600&fit=crop', 'Penthouse Suite', 0, NULL),
(1, 'https://images.unsplash.com/photo-1466978913421-dad2ebd01d17?w=800&h=600&fit=crop', 'Standard Single Room Bathroom', 0, NULL),
(2, 'https://images.unsplash.com/photo-1523217582562-09d0def993a6?w=800&h=600&fit=crop', 'Deluxe Double Room View', 0, NULL),
(3, 'https://images.unsplash.com/photo-1496417263034-38ec4f0b665a?w=800&h=600&fit=crop', 'Executive Suite Bedroom', 0, NULL),
(7, 'https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=800&h=600&fit=crop', 'Ocean View Suite Interior', 0, NULL),
(10, 'https://images.unsplash.com/photo-1505142468610-359e7d316be0?w=800&h=600&fit=crop', 'Riverside Deluxe View', 0, NULL)
    PRINT 'Room Images inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: RoomImages table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 7. INSERT ROOM TYPE-AMENITY RELATIONSHIPS (50 relationships)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomTypeAmenities' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Room Type-Amenity Relationships...'
    INSERT INTO RoomTypeAmenities (RoomTypeId, AmenityId)
VALUES
-- Standard Single Room (RoomType 1)
(1, 1), (1, 2), (1, 3), (1, 5), (1, 6),
-- Deluxe Double Room (RoomType 2)
(2, 1), (2, 2), (2, 3), (2, 4), (2, 5), (2, 6), (2, 7), (2, 8),
-- Executive Suite (RoomType 3)
(3, 1), (3, 2), (3, 3), (3, 4), (3, 5), (3, 6), (3, 7), (3, 8), (3, 9), (3, 10),
-- Family Room (RoomType 4)
(4, 1), (4, 2), (4, 3), (4, 4), (4, 5), (4, 6), (4, 7),
-- Premier Twin Room (RoomType 5)
(5, 1), (5, 2), (5, 3), (5, 5), (5, 6), (5, 8),
-- Studio Apartment (RoomType 6)
(6, 1), (6, 2), (6, 3), (6, 4), (6, 5), (6, 6), (6, 8),
-- Ocean View Suite (RoomType 7)
(7, 1), (7, 2), (7, 3), (7, 4), (7, 5), (7, 6), (7, 7), (7, 8), (7, 9), (7, 11),
-- Heritage Double Room (RoomType 8)
(8, 1), (8, 2), (8, 3), (8, 5), (8, 6),
-- Sabah View Room (RoomType 9)
(9, 1), (9, 2), (9, 3), (9, 5), (9, 6), (9, 10),
-- Riverside Deluxe (RoomType 10)
(10, 1), (10, 2), (10, 3), (10, 4), (10, 5), (10, 6), (10, 7), (10, 8),
-- Highland Cozy Room (RoomType 11)
(11, 1), (11, 2), (11, 3), (11, 5), (11, 6),
-- Beachfront Standard (RoomType 12)
(12, 1), (12, 2), (12, 3), (12, 5), (12, 6), (12, 9), (12, 11),
-- Historic Suite (RoomType 13)
(13, 1), (13, 2), (13, 3), (13, 4), (13, 5), (13, 6), (13, 7), (13, 8),
-- Coastal View Room (RoomType 14)
(14, 1), (14, 2), (14, 3), (14, 5), (14, 6), (14, 11),
-- City Center Standard (RoomType 15)
(15, 1), (15, 2), (15, 3), (15, 5), (15, 6), (15, 10),
-- Mountain View Deluxe (RoomType 16)
(16, 1), (16, 2), (16, 3), (16, 4), (16, 5), (16, 6), (16, 7), (16, 8), (16, 10),
-- Budget Single (RoomType 17)
(17, 1), (17, 2), (17, 3), (17, 6),
-- Triple Room (RoomType 18)
(18, 1), (18, 2), (18, 3), (18, 4), (18, 5), (18, 6),
-- Quad Room (RoomType 19)
(19, 1), (19, 2), (19, 3), (19, 4), (19, 5), (19, 6), (19, 7),
-- Penthouse Suite (RoomType 20)
(20, 1), (20, 2), (20, 3), (20, 4), (20, 5), (20, 6), (20, 7), (20, 8), (20, 9), (20, 10), (20, 12), (20, 13)
    PRINT 'Room Type-Amenity Relationships inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: RoomTypeAmenities table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 8. INSERT SERVICES (15 services)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Services' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Services...'
    INSERT INTO Services (Name, Description, Price, IsDeleted, DeletedAt)
VALUES
('Airport Transfer', 'Comfortable airport pickup and drop-off service', 50.00, 0, NULL),
('Breakfast Buffet', 'Delicious breakfast buffet with local and international options', 25.00, 0, NULL),
('Laundry Service', 'Professional laundry and dry cleaning service', 30.00, 0, NULL),
('Room Service', '24/7 room service with a variety of food and beverage options', 0.00, 0, NULL),
('Spa Treatment', 'Relaxing spa and massage services', 150.00, 0, NULL),
('Car Rental', 'Convenient car rental service for exploring the area', 80.00, 0, NULL),
('Tour Guide', 'Professional tour guide for local attractions', 100.00, 0, NULL),
('Gym Access', 'Full access to hotel fitness center', 20.00, 0, NULL),
('Swimming Pool Access', 'Access to hotel swimming pool and facilities', 15.00, 0, NULL),
('Wi-Fi Premium', 'High-speed premium Wi-Fi connection', 10.00, 0, NULL),
('Late Check-out', 'Extended check-out time until 2 PM', 50.00, 0, NULL),
('Early Check-in', 'Early check-in from 10 AM', 50.00, 0, NULL),
('Babysitting Service', 'Professional babysitting service for families', 40.00, 0, NULL),
('Business Center Access', 'Access to business center with printing and meeting facilities', 25.00, 0, NULL),
('Pet Accommodation', 'Pet-friendly accommodation with pet care services', 35.00, 0, NULL)
    PRINT 'Services inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: Services table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 9. INSERT PACKAGES (15 packages)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Packages' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Packages...'
    INSERT INTO Packages (Name, Description, TotalPrice, IsActive, IsDeleted, DeletedAt)
VALUES
('Romantic Getaway', 'Perfect package for couples with romantic dinner and spa treatment', 499.99, 1, 0, NULL),
('Family Fun Package', 'Family-friendly package with breakfast and activities for kids', 599.99, 1, 0, NULL),
('Business Traveler', 'Everything a business traveler needs including breakfast and Wi-Fi', 349.99, 1, 0, NULL),
('Beach Paradise', 'Beachfront stay with water sports and sunset dinner', 699.99, 1, 0, NULL),
('Heritage Explorer', 'Explore historic sites with guided tours and local experiences', 449.99, 1, 0, NULL),
('Adventure Seeker', 'Mountain and nature activities with equipment rental', 549.99, 1, 0, NULL),
('Wellness Retreat', 'Spa treatments, yoga sessions, and healthy meals', 799.99, 1, 0, NULL),
('Shopping Spree', 'Shopping district stay with shopping vouchers', 399.99, 1, 0, NULL),
('Foodie Delight', 'Culinary experience with food tours and cooking classes', 479.99, 1, 0, NULL),
('Weekend Escape', 'Weekend package with breakfast and late check-out', 299.99, 1, 0, NULL),
('Luxury Experience', 'Premium suite with all-inclusive services', 999.99, 1, 0, NULL),
('Budget Saver', 'Affordable package with essential amenities', 199.99, 1, 0, NULL),
('Extended Stay', 'Long-term stay package with discounts and services', 1299.99, 1, 0, NULL),
('Honeymoon Special', 'Luxurious honeymoon package with romantic amenities', 899.99, 1, 0, NULL),
('Group Package', 'Special rates for group bookings with meeting facilities', 2499.99, 1, 0, NULL)
    PRINT 'Packages inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: Packages table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 10. INSERT PACKAGE ITEMS (30 items linking packages to room types and services)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PackageItems' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Package Items...'
    INSERT INTO PackageItems (PackageId, RoomTypeId, ServiceId, Quantity, IsDeleted, DeletedAt)
VALUES
-- Romantic Getaway Package (Package 1)
(1, 3, 4, 1, 0, NULL), -- Executive Suite + Room Service
(1, NULL, 5, 1, 0, NULL), -- Spa Treatment
-- Family Fun Package (Package 2)
(2, 4, 2, 1, 0, NULL), -- Family Room + Breakfast
(2, NULL, 13, 1, 0, NULL), -- Babysitting Service
-- Business Traveler Package (Package 3)
(3, 5, 2, 1, 0, NULL), -- Premier Twin Room + Breakfast
(3, NULL, 10, 1, 0, NULL), -- Wi-Fi Premium
-- Beach Paradise Package (Package 4)
(4, 7, NULL, 1, 0, NULL), -- Ocean View Suite
(4, NULL, 9, 1, 0, NULL), -- Swimming Pool Access
-- Heritage Explorer Package (Package 5)
(5, 8, 7, 1, 0, NULL), -- Heritage Double Room + Tour Guide
-- Adventure Seeker Package (Package 6)
(6, 9, 6, 1, 0, NULL), -- Sabah View Room + Car Rental
-- Wellness Retreat Package (Package 7)
(7, 3, 5, 2, 0, NULL), -- Executive Suite + 2 Spa Treatments
-- Shopping Spree Package (Package 8)
(8, 2, NULL, 1, 0, NULL), -- Deluxe Double Room
-- Foodie Delight Package (Package 9)
(9, 8, NULL, 1, 0, NULL), -- Heritage Double Room
-- Weekend Escape Package (Package 10)
(10, 1, 2, 1, 0, NULL), -- Standard Single Room + Breakfast
(10, NULL, 11, 1, 0, NULL), -- Late Check-out
-- Luxury Experience Package (Package 11)
(11, 20, 4, 1, 0, NULL), -- Penthouse Suite + Room Service
(11, NULL, 5, 1, 0, NULL), -- Spa Treatment
(11, NULL, 2, 1, 0, NULL), -- Breakfast
-- Budget Saver Package (Package 12)
(12, 17, NULL, 1, 0, NULL), -- Budget Single
-- Extended Stay Package (Package 13)
(13, 6, 2, 1, 0, NULL), -- Studio Apartment + Breakfast
(13, NULL, 10, 1, 0, NULL), -- Wi-Fi Premium
-- Honeymoon Special Package (Package 14)
(14, 7, 4, 1, 0, NULL), -- Ocean View Suite + Room Service
(14, NULL, 5, 1, 0, NULL), -- Spa Treatment
-- Group Package (Package 15)
(15, 2, NULL, 3, 0, NULL), -- 3x Deluxe Double Room
(15, NULL, 14, 1, 0, NULL) -- Business Center Access
    PRINT 'Package Items inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: PackageItems table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 11. INSERT PROMOTIONS (15 promotions)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Promotions' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Promotions...'
    INSERT INTO Promotions (Code, Description, Type, Value, StartDate, EndDate, IsActive, LimitPerPhoneNumber, LimitPerPaymentCard, LimitPerDevice, LimitPerUserAccount, MaxUsesPerLimit, MinimumNights, MinimumAmount, MaxTotalUses, IsDeleted, DeletedAt)
VALUES
('WELCOME10', 'Welcome discount - 10% off your first booking', 0, 10, DATEADD(day, -20, GETDATE()), DATEADD(day, 100, GETDATE()), 1, 1, 1, 1, 1, 1, NULL, NULL, 100, 0, NULL),
('SAVE15', 'Save 15% on all bookings', 0, 15, DATEADD(day, -15, GETDATE()), DATEADD(day, 75, GETDATE()), 1, 1, 1, 1, 1, 1, NULL, NULL, 200, 0, NULL),
('SUMMER20', 'Summer special - 20% off', 0, 20, DATEADD(day, -10, GETDATE()), DATEADD(day, 50, GETDATE()), 1, 1, 0, 0, 1, 1, NULL, NULL, NULL, 0, NULL),
('FIXED50', 'Fixed RM50 discount', 1, 50, DATEADD(day, -5, GETDATE()), DATEADD(day, 25, GETDATE()), 1, 1, 1, 0, 1, 1, NULL, NULL, NULL, 0, NULL),
('EARLYBIRD', 'Book early and save 15%', 0, 15, GETDATE(), DATEADD(day, 60, GETDATE()), 1, 1, 0, 0, 1, 1, NULL, NULL, NULL, 0, NULL),
('LONGSTAY', 'Stay 5 nights or more get RM100 off', 1, 100, GETDATE(), DATEADD(day, 90, GETDATE()), 1, 1, 0, 0, 1, 1, 5, NULL, NULL, 0, NULL),
('WEEKEND15', 'Weekend special - 15% off', 0, 15, GETDATE(), DATEADD(day, 45, GETDATE()), 1, 1, 0, 0, 1, 1, NULL, NULL, NULL, 0, NULL),
('NEWYEAR25', 'New Year special - 25% off', 0, 25, GETDATE(), DATEADD(day, 30, GETDATE()), 1, 1, 0, 0, 1, 1, NULL, NULL, NULL, 0, NULL),
('FIXED100', 'Fixed RM100 discount for bookings over RM500', 1, 100, GETDATE(), DATEADD(day, 60, GETDATE()), 1, 1, 0, 0, 1, 1, NULL, 500, NULL, 0, NULL),
('STUDENT10', 'Student discount - 10% off', 0, 10, GETDATE(), DATEADD(day, 90, GETDATE()), 1, 1, 0, 0, 1, 1, NULL, NULL, NULL, 0, NULL),
('SENIOR15', 'Senior citizen discount - 15% off', 0, 15, GETDATE(), DATEADD(day, 90, GETDATE()), 1, 1, 0, 0, 1, 1, NULL, NULL, NULL, 0, NULL),
('GROUP20', 'Group booking discount - 20% off for 3+ rooms', 0, 20, GETDATE(), DATEADD(day, 120, GETDATE()), 1, 1, 0, 0, 1, 1, NULL, NULL, NULL, 0, NULL),
('LOYALTY5', 'Loyalty member discount - 5% off', 0, 5, GETDATE(), DATEADD(day, 365, GETDATE()), 1, 1, 0, 0, 1, 10, NULL, NULL, NULL, 0, NULL),
('FAMILY30', 'Family package discount - 30% off for family rooms', 0, 30, GETDATE(), DATEADD(day, 75, GETDATE()), 1, 1, 0, 0, 1, 2, NULL, NULL, NULL, 0, NULL),
('MIDWEEK20', 'Midweek special - 20% off for Monday-Thursday bookings', 0, 20, GETDATE(), DATEADD(day, 100, GETDATE()), 1, 1, 0, 0, 1, 1, NULL, NULL, NULL, 0, NULL)
    PRINT 'Promotions inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: Promotions table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 12. INSERT BOOKINGS (15 bookings)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Bookings' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Bookings...'
    INSERT INTO Bookings (UserId, RoomId, CheckInDate, CheckOutDate, BookingDate, TotalPrice, Status, PromotionId, PaymentAmount, PaymentMethod, PaymentStatus, TransactionId, PaymentDate, CancellationDate, CancellationReason, RefundAmount, IsDeleted, DeletedAt)
VALUES
-- Confirmed bookings (Status = 1)
(4, 1, DATEADD(day, 10, GETDATE()), DATEADD(day, 13, GETDATE()), DATEADD(day, -5, GETDATE()), 239.97, 1, 1, 239.97, 0, 1, 'CC-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR), DATEADD(day, -5, GETDATE()), NULL, NULL, NULL, 0, NULL),
(5, 4, DATEADD(day, 15, GETDATE()), DATEADD(day, 18, GETDATE()), DATEADD(day, -2, GETDATE()), 389.97, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, 0, NULL),
-- CheckedOut bookings (Status = 4)
(7, 7, DATEADD(day, -30, GETDATE()), DATEADD(day, -27, GETDATE()), DATEADD(day, -35, GETDATE()), 599.97, 4, NULL, 599.97, 1, 1, 'PP-' + FORMAT(DATEADD(day, -30, GETDATE()), 'yyyyMMdd') + '-' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR), DATEADD(day, -30, GETDATE()), NULL, NULL, NULL, 0, NULL),
(8, 9, DATEADD(day, -20, GETDATE()), DATEADD(day, -17, GETDATE()), DATEADD(day, -25, GETDATE()), 479.97, 4, NULL, 479.97, 2, 1, 'BT-' + FORMAT(DATEADD(day, -20, GETDATE()), 'yyyyMMdd') + '-' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR), DATEADD(day, -20, GETDATE()), NULL, NULL, NULL, 0, NULL),
(9, 11, DATEADD(day, -15, GETDATE()), DATEADD(day, -12, GETDATE()), DATEADD(day, -20, GETDATE()), 329.97, 4, 2, 329.97, 0, 1, 'CC-' + FORMAT(DATEADD(day, -15, GETDATE()), 'yyyyMMdd') + '-' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR), DATEADD(day, -15, GETDATE()), NULL, NULL, NULL, 0, NULL),
(10, 13, DATEADD(day, -40, GETDATE()), DATEADD(day, -37, GETDATE()), DATEADD(day, -45, GETDATE()), 449.97, 4, NULL, 449.97, 1, 1, 'PP-' + FORMAT(DATEADD(day, -40, GETDATE()), 'yyyyMMdd') + '-' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR), DATEADD(day, -40, GETDATE()), NULL, NULL, NULL, 0, NULL),
(11, 15, DATEADD(day, -50, GETDATE()), DATEADD(day, -47, GETDATE()), DATEADD(day, -55, GETDATE()), 689.97, 4, 3, 689.97, 0, 1, 'CC-' + FORMAT(DATEADD(day, -50, GETDATE()), 'yyyyMMdd') + '-' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR), DATEADD(day, -50, GETDATE()), NULL, NULL, NULL, 0, NULL),
(12, 17, DATEADD(day, -60, GETDATE()), DATEADD(day, -57, GETDATE()), DATEADD(day, -65, GETDATE()), 299.97, 4, NULL, 299.97, 2, 1, 'BT-' + FORMAT(DATEADD(day, -60, GETDATE()), 'yyyyMMdd') + '-' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR), DATEADD(day, -60, GETDATE()), NULL, NULL, NULL, 0, NULL),
(13, 19, DATEADD(day, -70, GETDATE()), DATEADD(day, -67, GETDATE()), DATEADD(day, -75, GETDATE()), 359.97, 4, 4, 359.97, 0, 1, 'CC-' + FORMAT(DATEADD(day, -70, GETDATE()), 'yyyyMMdd') + '-' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR), DATEADD(day, -70, GETDATE()), NULL, NULL, NULL, 0, NULL),
-- Pending bookings (Status = 0)
(4, 2, DATEADD(day, 20, GETDATE()), DATEADD(day, 23, GETDATE()), GETDATE(), 239.97, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, 0, NULL),
(5, 5, DATEADD(day, 25, GETDATE()), DATEADD(day, 28, GETDATE()), DATEADD(day, 1, GETDATE()), 329.97, 1, 5, 329.97, 1, 1, 'PP-' + FORMAT(DATEADD(day, 1, GETDATE()), 'yyyyMMdd') + '-' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR), DATEADD(day, 1, GETDATE()), NULL, NULL, NULL, 0, NULL),
(7, 8, DATEADD(day, 30, GETDATE()), DATEADD(day, 33, GETDATE()), DATEADD(day, 2, GETDATE()), 389.97, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, 0, NULL),
(8, 10, DATEADD(day, 35, GETDATE()), DATEADD(day, 38, GETDATE()), DATEADD(day, 3, GETDATE()), 419.97, 1, 6, 419.97, 0, 1, 'CC-' + FORMAT(DATEADD(day, 3, GETDATE()), 'yyyyMMdd') + '-' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR), DATEADD(day, 3, GETDATE()), NULL, NULL, NULL, 0, NULL),
(9, 12, DATEADD(day, 40, GETDATE()), DATEADD(day, 43, GETDATE()), DATEADD(day, 4, GETDATE()), 449.97, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, 0, NULL),
(10, 14, DATEADD(day, 45, GETDATE()), DATEADD(day, 48, GETDATE()), DATEADD(day, 5, GETDATE()), 459.97, 1, 7, 459.97, 2, 1, 'BT-' + FORMAT(DATEADD(day, 5, GETDATE()), 'yyyyMMdd') + '-' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR), DATEADD(day, 5, GETDATE()), NULL, NULL, NULL, 0, NULL)
    PRINT 'Bookings inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: Bookings table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 13. INSERT REVIEWS (15 reviews)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Reviews...'
    INSERT INTO Reviews (BookingId, Rating, Comment, ReviewDate, IsDeleted, DeletedAt)
VALUES
(1, 5, 'Excellent stay! The room was clean, comfortable, and the staff was very helpful. Great value for money!', DATEADD(day, -3, GETDATE()), 0, NULL),
(2, 4, 'Good experience overall. The room was spacious and well-maintained. Would stay again.', DATEADD(day, -1, GETDATE()), 0, NULL),
(3, 5, 'Perfect location and great service. Highly recommended!', DATEADD(day, -25, GETDATE()), 0, NULL),
(4, 4, 'Comfortable room with all necessary amenities. Good value.', DATEADD(day, -15, GETDATE()), 0, NULL),
(5, 5, 'Amazing experience! The suite was luxurious and the view was spectacular.', DATEADD(day, -10, GETDATE()), 0, NULL),
(6, 4, 'Great value for money. The location is perfect, close to everything. Room was clean and comfortable.', DATEADD(day, -35, GETDATE()), 0, NULL),
(7, 5, 'Best budget hotel experience I''ve had! The family room was spacious, perfect for our needs. Kids loved it and we''ll be back for sure. Highly recommend!', DATEADD(day, -45, GETDATE()), 0, NULL),
(8, 4, 'Clean, modern, and well-maintained. The twin room was perfect for our business trip. Good amenities and friendly staff. Would stay again.', DATEADD(day, -55, GETDATE()), 0, NULL),
(9, 5, 'Stunning ocean view! The suite exceeded all expectations. Beautiful balcony, comfortable bed, and excellent service. Worth every ringgit. Perfect for a romantic getaway!', DATEADD(day, -65, GETDATE()), 0, NULL),
(11, 5, 'Absolutely fantastic! The staff went above and beyond to make our stay memorable. The room was spotless and the breakfast was delicious. Will definitely return!', DATEADD(day, -2, GETDATE()), 0, NULL),
(13, 4, 'Great hotel with excellent location. Room was clean and had all the amenities we needed. Good value for money.', DATEADD(day, -1, GETDATE()), 0, NULL),
(15, 5, 'Outstanding service and beautiful room. The view was amazing and the staff was very accommodating. Highly recommend this hotel!', GETDATE(), 0, NULL),
(10, 4, 'Very satisfied with our stay. The room was clean, the bed was comfortable, and the location was convenient. Good value for the price.', DATEADD(day, -40, GETDATE()), 0, NULL),
(12, 5, 'Exceptional service! The staff was friendly and helpful. The room exceeded our expectations. Will definitely book again!', DATEADD(day, -50, GETDATE()), 0, NULL),
(14, 4, 'Nice hotel with good amenities. The breakfast was decent and the room was well-maintained. Would recommend for budget travelers.', DATEADD(day, -5, GETDATE()), 0, NULL)
    PRINT 'Reviews inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: Reviews table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 14. INSERT CONTACT MESSAGES (15 messages)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ContactMessages' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Contact Messages...'
    INSERT INTO ContactMessages (Name, Email, Subject, Message, SentAt, IsRead, IsDeleted, DeletedAt)
VALUES
('John Smith', 'john.smith@example.com', 'Inquiry about group bookings', 'Hello, I''m planning a group booking for 20 people next month. Could you please provide more information about group rates?', DATEADD(day, -10, GETDATE()), 1, 0, NULL),
('Sarah Johnson', 'sarah.j@example.com', 'Special dietary requirements', 'Do your hotels accommodate guests with special dietary requirements? I have a severe nut allergy.', DATEADD(day, -5, GETDATE()), 0, 0, NULL),
('Michael Chen', 'michael.chen@example.com', 'Early check-in availability', 'I have an early morning flight arrival. Is early check-in available and what are the charges?', DATEADD(day, -2, GETDATE()), 0, 0, NULL),
('Emily Wong', 'emily.wong@example.com', 'Pet-friendly rooms', 'Do you have pet-friendly rooms available? I will be traveling with my small dog.', DATEADD(day, -8, GETDATE()), 1, 0, NULL),
('Robert Tan', 'robert.tan@example.com', 'Accessibility features', 'I use a wheelchair. Do your rooms have accessibility features?', DATEADD(day, -6, GETDATE()), 1, 0, NULL),
('Lisa Lim', 'lisa.lim@example.com', 'Cancellation policy', 'What is your cancellation policy? I may need to change my booking dates.', DATEADD(day, -4, GETDATE()), 0, 0, NULL),
('David Kumar', 'david.kumar@example.com', 'Parking availability', 'Is parking available at your hotel? What are the parking rates?', DATEADD(day, -7, GETDATE()), 1, 0, NULL),
('Nurul Aisyah', 'nurul.aisyah@example.com', 'Wi-Fi speed', 'What is the Wi-Fi speed in your rooms? I need reliable internet for work.', DATEADD(day, -3, GETDATE()), 0, 0, NULL),
('James Lee', 'james.lee@example.com', 'Breakfast options', 'What breakfast options are available? Do you serve halal food?', DATEADD(day, -9, GETDATE()), 1, 0, NULL),
('Fatimah Zahra', 'fatimah.z@example.com', 'Room with view', 'Do you have rooms with city view? I would like to book one for my anniversary.', DATEADD(day, -1, GETDATE()), 0, 0, NULL),
('Kevin Chen', 'kevin.chen@example.com', 'Long-term stay discount', 'I plan to stay for 2 weeks. Do you offer discounts for long-term stays?', DATEADD(day, -12, GETDATE()), 1, 0, NULL),
('Amira Hassan', 'amira.hassan@example.com', 'Gift voucher', 'Do you sell gift vouchers? I would like to purchase one as a gift.', GETDATE(), 0, 0, NULL),
('Thomas Anderson', 'thomas.a@example.com', 'Conference room availability', 'Do you have conference or meeting rooms available for business events?', DATEADD(day, -11, GETDATE()), 1, 0, NULL),
('Maria Garcia', 'maria.g@example.com', 'Luggage storage', 'Can I store my luggage at the hotel before check-in or after check-out?', DATEADD(day, -13, GETDATE()), 0, 0, NULL),
('Ahmed Ibrahim', 'ahmed.i@example.com', 'Prayer room facilities', 'Do you have prayer room facilities for Muslim guests?', DATEADD(day, -14, GETDATE()), 1, 0, NULL)
    PRINT 'Contact Messages inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: ContactMessages table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- 15. INSERT NEWSLETTERS (15 subscriptions)
-- ============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Newsletters' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Inserting Newsletter Subscriptions...'
    INSERT INTO Newsletters (Email, SubscribedAt, IsActive, IsDeleted, DeletedAt)
VALUES
('newsletter1@example.com', DATEADD(day, -30, GETDATE()), 1, 0, NULL),
('newsletter2@example.com', DATEADD(day, -20, GETDATE()), 1, 0, NULL),
('newsletter3@example.com', DATEADD(day, -10, GETDATE()), 1, 0, NULL),
('subscriber1@example.com', DATEADD(day, -25, GETDATE()), 1, 0, NULL),
('subscriber2@example.com', DATEADD(day, -15, GETDATE()), 1, 0, NULL),
('subscriber3@example.com', DATEADD(day, -5, GETDATE()), 1, 0, NULL),
('member1@example.com', DATEADD(day, -40, GETDATE()), 1, 0, NULL),
('member2@example.com', DATEADD(day, -35, GETDATE()), 0, 0, NULL),
('member3@example.com', DATEADD(day, -28, GETDATE()), 1, 0, NULL),
('email1@example.com', DATEADD(day, -22, GETDATE()), 1, 0, NULL),
('email2@example.com', DATEADD(day, -18, GETDATE()), 1, 0, NULL),
('email3@example.com', DATEADD(day, -12, GETDATE()), 1, 0, NULL),
('user1@example.com', DATEADD(day, -8, GETDATE()), 1, 0, NULL),
('user2@example.com', DATEADD(day, -3, GETDATE()), 1, 0, NULL),
('user3@example.com', GETDATE(), 1, 0, NULL)
    PRINT 'Newsletter Subscriptions inserted successfully.'
END
ELSE
BEGIN
    PRINT 'ERROR: Newsletters table does not exist. Please run: dotnet ef database update'
END
GO

-- ============================================
-- VERIFICATION AND SUMMARY
-- ============================================
-- Only show summary if tables exist
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Hotels' AND schema_id = SCHEMA_ID('dbo'))
   AND EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('dbo'))
   AND EXISTS (SELECT * FROM sys.tables WHERE name = 'Bookings' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
PRINT ''
PRINT '============================================'
PRINT 'DATABASE SETUP COMPLETE!'
PRINT '============================================'
PRINT ''

-- Use variables for counts to avoid subquery errors in PRINT
DECLARE @HotelCount INT
DECLARE @UserCount INT
DECLARE @RoomTypeCount INT
DECLARE @RoomCount INT
DECLARE @BookingCount INT
DECLARE @ReviewCount INT
DECLARE @PromotionCount INT
DECLARE @PackageCount INT

SELECT @HotelCount = COUNT(*) FROM Hotels
SELECT @UserCount = COUNT(*) FROM Users
SELECT @RoomTypeCount = COUNT(*) FROM RoomTypes
SELECT @RoomCount = COUNT(*) FROM Rooms
SELECT @BookingCount = COUNT(*) FROM Bookings
SELECT @ReviewCount = COUNT(*) FROM Reviews
SELECT @PromotionCount = COUNT(*) FROM Promotions
SELECT @PackageCount = COUNT(*) FROM Packages

PRINT 'Summary of inserted data:'
PRINT '  - Hotels: ' + CAST(@HotelCount AS VARCHAR) + ' hotels'
PRINT '  - Users: ' + CAST(@UserCount AS VARCHAR) + ' users'
PRINT '    * Admin: 1 (no hotel assignment - can see all hotels)'
PRINT '    * Managers: 15 (one assigned to each hotel)'
PRINT '    * Staff: 15 (one assigned to each hotel)'
PRINT '    * Customers: 12'
PRINT '  - Room Types: ' + CAST(@RoomTypeCount AS VARCHAR) + ' room types'
PRINT '  - Rooms: ' + CAST(@RoomCount AS VARCHAR) + ' rooms'
PRINT '  - Bookings: ' + CAST(@BookingCount AS VARCHAR) + ' bookings'
PRINT '  - Reviews: ' + CAST(@ReviewCount AS VARCHAR) + ' reviews'
PRINT '  - Promotions: ' + CAST(@PromotionCount AS VARCHAR) + ' promotions'
PRINT '  - Packages: ' + CAST(@PackageCount AS VARCHAR) + ' packages'
PRINT ''
PRINT '============================================'
PRINT 'IMPORTANT: Password Setup Required'
PRINT '============================================'
PRINT 'User passwords are set to PLACEHOLDER_PASSWORD_HASH.'
PRINT 'After running this script, you MUST:'
PRINT ''
PRINT '1. Run the application once - DbInitializer will set admin passwords:'
PRINT '   - admin@hotel.com: Admin123!'
PRINT '   - manager@hotel.com: Manager123!'
PRINT '   - staff@hotel.com: Password123!'
PRINT ''
PRINT '2. OR use password reset feature for customer accounts'
PRINT ''
PRINT '============================================'
PRINT 'Hotel Assignments:'
PRINT '============================================'
PRINT 'Each hotel has been assigned:'
PRINT '  - 1 Manager (manager1@hotel.com through manager15@hotel.com)'
PRINT '  - 1 Staff (staff1@hotel.com through staff15@hotel.com)'
PRINT ''
PRINT 'Example assignments:'
PRINT '  - Hotel 1 (Budget Inn KL Sentral): manager1@hotel.com, staff1@hotel.com'
PRINT '  - Hotel 2 (Economy Stay Bukit Bintang): manager2@hotel.com, staff2@hotel.com'
PRINT '  - Hotel 3 (Penang Budget Hotel): manager3@hotel.com, staff3@hotel.com'
PRINT '  - ... and so on for all 15 hotels'
PRINT ''
PRINT 'Admin (admin@hotel.com):'
PRINT '  No hotel assignment (can see all hotels)'
PRINT ''
PRINT 'Default Passwords (after app initialization):'
PRINT '  - admin@hotel.com: Admin123!'
PRINT '  - manager1@hotel.com through manager15@hotel.com: Manager123!'
PRINT '  - staff1@hotel.com through staff15@hotel.com: Password123!'
PRINT ''
PRINT '============================================'
PRINT 'Script completed successfully!'
PRINT '============================================'
END
ELSE
BEGIN
    PRINT ''
    PRINT '============================================'
    PRINT 'ERROR: Could not generate summary'
    PRINT '============================================'
    PRINT 'Some tables are missing. Please run:'
    PRINT '  dotnet ef database update'
    PRINT ''
END
GO
