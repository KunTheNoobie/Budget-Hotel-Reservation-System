-- ============================================
-- Script to Insert Sample Data
-- Budget Hotel Reservation System
-- ============================================
-- This script inserts 15+ sample records for each table
-- Run 01_ClearAllData.sql first to clear existing data
-- 
-- RECORD COUNTS:
-- - 15 Hotels (various locations across Malaysia)
-- - 15 Users (1 Admin, 1 Manager, 1 Staff, 12 Customers)
-- - 15 Amenities (with image URLs)
-- - 20 Room Types (various categories and price ranges)
-- - 25 Rooms (across different room types)
-- - 30 Room Images (multiple images per room type)
-- - 50 Room Type-Amenity relationships
-- - 15 Services (additional services for booking)
-- - 15 Packages (bundled deals)
-- - 30 Package Items (linking room types and services to packages)
-- - 12 Promotions (discount codes with various validation rules)
-- - 15 Bookings (various statuses and payment methods)
-- - 12 Reviews (customer feedback and ratings)
-- - 12 Contact Messages (customer inquiries)
-- - 12 Newsletter Subscriptions (email subscriptions)
-- ============================================
--
-- IMPORTANT NOTES:
-- 1. User passwords are set to 'PLACEHOLDER_PASSWORD_HASH'
--    After running this script, you MUST:
--    a) Run the application once - DbInitializer will set admin passwords
--    b) OR use password reset feature for each user account
--    c) OR manually update passwords in the database using BCrypt hashes
--
-- 2. Default test passwords (after application initializes):
--    - admin@hotel.com: Admin123!
--    - manager@hotel.com: Manager123!
--    - All other users: Use password reset feature
--
-- 3. The application's DbInitializer will handle admin account passwords
--    automatically when you first run the application
-- ============================================

USE [BMIT2023_HotelReservation]
GO

-- ============================================
-- 1. INSERT HOTELS (15 hotels)
-- ============================================
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
GO

-- ============================================
-- 2. INSERT USERS (15 users)
-- ============================================
-- IMPORTANT: Passwords need to be BCrypt hashed. The hash below is a placeholder.
-- Default password for all users: "Password123!"
-- After running this script, you should either:
-- 1. Use the application's password reset feature to set proper passwords
-- 2. Or manually update passwords using BCrypt.Net.BCrypt.HashPassword("Password123!", BCrypt.Net.BCrypt.GenerateSalt(12))
-- Note: BCrypt hashes are unique each time due to random salts, so you cannot use a pre-generated hash.
-- The application will handle password hashing when users login/register.
-- Note: PasswordHash is set to a placeholder. You need to update passwords after running this script.
-- Use the application's password reset feature or manually hash passwords using BCrypt.
-- For testing, the application's DbInitializer will set proper passwords for admin accounts.
INSERT INTO Users (Email, FullName, PasswordHash, PhoneNumber, Role, IsEmailVerified, IsActive, CreatedAt, ProfilePictureUrl, Bio, PreferredLanguage, Theme, IsDeleted, DeletedAt)
VALUES
('admin@hotel.com', 'Admin Hotel', 'PLACEHOLDER_PASSWORD_HASH', NULL, 0, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('manager@hotel.com', 'Manager Smith', 'PLACEHOLDER_PASSWORD_HASH', NULL, 1, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('staff@hotel.com', 'Staff Johnson', 'PLACEHOLDER_PASSWORD_HASH', NULL, 2, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('ahmad@example.com', 'Ahmad Zulkifli', 'PLACEHOLDER_PASSWORD_HASH', '+60-12-345-6789', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('siti@example.com', 'Siti Nurhaliza', 'PLACEHOLDER_PASSWORD_HASH', '+60-19-876-5432', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('charlie@example.com', 'Charlie Brown', 'PLACEHOLDER_PASSWORD_HASH', NULL, 3, 0, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('sarah@example.com', 'Sarah Tan', 'PLACEHOLDER_PASSWORD_HASH', '+60-16-123-4567', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('ali@example.com', 'Mohammad Ali', 'PLACEHOLDER_PASSWORD_HASH', '+60-17-234-5678', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('lisa@example.com', 'Lisa Wong', 'PLACEHOLDER_PASSWORD_HASH', '+60-18-345-6789', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('david@example.com', 'David Lee', 'PLACEHOLDER_PASSWORD_HASH', '+60-19-456-7890', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('nurul@example.com', 'Nurul Aisyah', 'PLACEHOLDER_PASSWORD_HASH', '+60-13-567-8901', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('james@example.com', 'James Lim', 'PLACEHOLDER_PASSWORD_HASH', '+60-14-678-9012', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('fatimah@example.com', 'Fatimah Zahra', 'PLACEHOLDER_PASSWORD_HASH', '+60-15-789-0123', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('kevin@example.com', 'Kevin Chen', 'PLACEHOLDER_PASSWORD_HASH', '+60-11-890-1234', 3, 1, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL),
('amira@example.com', 'Amira Hassan', 'PLACEHOLDER_PASSWORD_HASH', '+60-10-901-2345', 3, 0, 1, GETDATE(), NULL, NULL, 'en-US', 'Default', 0, NULL)
GO

-- ============================================
-- 3. INSERT AMENITIES (15 amenities)
-- ============================================
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
GO

-- ============================================
-- 4. INSERT ROOM TYPES (20 room types)
-- ============================================
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
GO

-- ============================================
-- 5. INSERT ROOMS (25 rooms)
-- ============================================
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
GO

-- ============================================
-- 6. INSERT ROOM IMAGES (30 images)
-- ============================================
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
(12, 'https://images.unsplash.com/photo-1505142468610-359e7d316be0?w=800&h=600&fit=crop', 'Beachfront Standard', 0, NULL),
(13, 'https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?w=800&h=600&fit=crop', 'Historic Suite', 0, NULL),
(14, 'https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=800&h=600&fit=crop', 'Coastal View Room', 0, NULL),
(15, 'https://images.unsplash.com/photo-1505691938895-1758d7feb511?w=800&h=600&fit=crop', 'City Center Standard', 0, NULL),
(16, 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800&h=600&fit=crop', 'Mountain View Deluxe', 0, NULL),
(17, 'https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?w=800&h=600&fit=crop', 'Budget Single', 0, NULL),
(18, 'https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=800&h=600&fit=crop', 'Triple Room', 0, NULL),
(19, 'https://images.unsplash.com/photo-1505691938895-1758d7feb511?w=800&h=600&fit=crop', 'Quad Room', 0, NULL),
(20, 'https://images.unsplash.com/photo-1523217582562-09d0def993a6?w=800&h=600&fit=crop', 'Penthouse Suite', 0, NULL),
(20, 'https://images.unsplash.com/photo-1496417263034-38ec4f0b665a?w=800&h=600&fit=crop', 'Penthouse Suite View', 0, NULL),
(1, 'https://images.unsplash.com/photo-1466978913421-dad2ebd01d17?w=800&h=600&fit=crop', 'Standard Single Room Bathroom', 0, NULL),
(2, 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800&h=600&fit=crop', 'Deluxe Double Room View', 0, NULL),
(3, 'https://images.unsplash.com/photo-1505142468610-359e7d316be0?w=800&h=600&fit=crop', 'Executive Suite Balcony', 0, NULL),
(4, 'https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?w=800&h=600&fit=crop', 'Family Room View', 0, NULL)
GO

-- ============================================
-- 7. INSERT ROOM TYPE AMENITIES (50 relationships)
-- ============================================
INSERT INTO RoomTypeAmenities (RoomTypeId, AmenityId)
VALUES
-- Standard Single Room (1) - 5 amenities
(1, 1), (1, 2), (1, 3), (1, 6), (1, 4),
-- Deluxe Double Room (2) - 7 amenities
(2, 1), (2, 2), (2, 3), (2, 4), (2, 5), (2, 6), (2, 7),
-- Executive Suite (3) - 10 amenities
(3, 1), (3, 2), (3, 3), (3, 4), (3, 5), (3, 6), (3, 7), (3, 8), (3, 9), (3, 10),
-- Family Room (4) - 6 amenities
(4, 1), (4, 2), (4, 3), (4, 4), (4, 6), (4, 12),
-- Premier Twin Room (5) - 6 amenities
(5, 1), (5, 2), (5, 3), (5, 4), (5, 6), (5, 10),
-- Studio Apartment (6) - 8 amenities
(6, 1), (6, 2), (6, 3), (6, 4), (6, 5), (6, 6), (6, 7), (6, 10),
-- Ocean View Suite (7) - 11 amenities
(7, 1), (7, 2), (7, 3), (7, 4), (7, 5), (7, 6), (7, 7), (7, 8), (7, 9), (7, 11), (7, 12),
-- Heritage Double Room (8) - 5 amenities
(8, 1), (8, 2), (8, 3), (8, 6), (8, 10),
-- Sabah View Room (9) - 6 amenities
(9, 1), (9, 2), (9, 3), (9, 4), (9, 6), (9, 10),
-- Riverside Deluxe (10) - 7 amenities
(10, 1), (10, 2), (10, 3), (10, 4), (10, 5), (10, 6), (10, 10),
-- Highland Cozy Room (11) - 5 amenities
(11, 1), (11, 2), (11, 3), (11, 6), (11, 10),
-- Beachfront Standard (12) - 7 amenities
(12, 1), (12, 2), (12, 3), (12, 4), (12, 6), (12, 11), (12, 12),
-- Historic Suite (13) - 8 amenities
(13, 1), (13, 2), (13, 3), (13, 4), (13, 5), (13, 6), (13, 7), (13, 10),
-- Coastal View Room (14) - 6 amenities
(14, 1), (14, 2), (14, 3), (14, 4), (14, 6), (14, 11),
-- City Center Standard (15) - 5 amenities
(15, 1), (15, 2), (15, 3), (15, 6), (15, 10),
-- Mountain View Deluxe (16) - 7 amenities
(16, 1), (16, 2), (16, 3), (16, 4), (16, 5), (16, 6), (16, 10),
-- Budget Single (17) - 4 amenities
(17, 1), (17, 2), (17, 3), (17, 6),
-- Triple Room (18) - 6 amenities
(18, 1), (18, 2), (18, 3), (18, 4), (18, 6), (18, 12),
-- Quad Room (19) - 6 amenities
(19, 1), (19, 2), (19, 3), (19, 4), (19, 6), (19, 12),
-- Penthouse Suite (20) - 12 amenities
(20, 1), (20, 2), (20, 3), (20, 4), (20, 5), (20, 6), (20, 7), (20, 8), (20, 9), (20, 10), (20, 12), (20, 13)
GO

-- ============================================
-- 8. INSERT SERVICES (15 services)
-- ============================================
INSERT INTO Services (Name, Description, Price, IsDeleted, DeletedAt)
VALUES
('Airport Transfer', 'Private airport pick-up and drop-off service within Klang Valley.', 120.00, 0, NULL),
('Breakfast Buffet', 'Authentic Malaysian breakfast spread for two guests.', 60.00, 0, NULL),
('Late Checkout', 'Extend your checkout time to 4.00 PM.', 80.00, 0, NULL),
('Island Hopping Tour', 'Half-day Langkawi tour with hotel transfer.', 250.00, 0, NULL),
('Spa Treatment', '60-minute aromatherapy massage for one.', 150.00, 0, NULL),
('Candlelight Dinner', 'Romantic 3-course dinner by the beach or city view.', 300.00, 0, NULL),
('City Tour', 'Guided tour of historical landmarks and cultural sites.', 100.00, 0, NULL),
('Car Rental', 'Daily car rental (compact sedan) including insurance.', 180.00, 0, NULL),
('Laundry Service', 'Same-day laundry and dry cleaning service.', 50.00, 0, NULL),
('Room Upgrade', 'Upgrade to a higher category room (subject to availability).', 100.00, 0, NULL),
('Wi-Fi Premium', 'High-speed premium Wi-Fi for multiple devices.', 30.00, 0, NULL),
('Pet Care Service', 'Pet sitting and care service for your furry friends.', 90.00, 0, NULL),
('Concierge Service', 'Personal concierge assistance for restaurant reservations and activities.', 70.00, 0, NULL),
('Gym Access', '24/7 access to fully equipped fitness center.', 40.00, 0, NULL),
('Pool Access', 'Access to infinity pool and poolside facilities.', 35.00, 0, NULL)
GO

-- ============================================
-- 9. INSERT PACKAGES (15 packages)
-- ============================================
INSERT INTO Packages (Name, Description, TotalPrice, ImageUrl, IsActive, IsDeleted, DeletedAt)
VALUES
('Kuala Lumpur City Explorer', '2-night stay with breakfast and airport transfer in KL Sentral.', 329.99, NULL, 1, 0, NULL),
('Malacca Family Fun Package', 'Spacious family room with breakfast for four and late checkout in historic Malacca.', 549.99, NULL, 1, 0, NULL),
('Langkawi Beach Escape', 'Ocean-view suite with breakfast and island hopping tour.', 789.99, NULL, 1, 0, NULL),
('Honeymoon Bliss', 'Romantic getaway with spa treatment and candlelight dinner.', 999.99, NULL, 1, 0, NULL),
('Business Traveler', 'Executive suite with airport transfer and high-speed Wi-Fi.', 450.00, NULL, 1, 0, NULL),
('Adventure Seeker', 'City tour and car rental for exploring at your own pace.', 399.99, NULL, 1, 0, NULL),
('Ipoh Heritage Experience', '2-night stay in heritage room with city tour and breakfast.', 279.99, NULL, 1, 0, NULL),
('Sabah Adventure Package', '3-night stay with mountain tour and breakfast buffet.', 449.99, NULL, 1, 0, NULL),
('Sarawak Cultural Journey', '2-night riverside stay with cultural tour and traditional dinner.', 379.99, NULL, 1, 0, NULL),
('Cameron Highlands Retreat', '2-night highland stay with tea plantation tour and breakfast.', 249.99, NULL, 1, 0, NULL),
('Weekend Getaway Special', '2-night stay with late checkout and breakfast for two.', 299.99, NULL, 1, 0, NULL),
('Extended Stay Value', '5-night stay with complimentary breakfast and room upgrade.', 699.99, NULL, 1, 0, NULL),
('Beach Paradise Package', '3-night beachfront stay with breakfast and pool access.', 499.99, NULL, 1, 0, NULL),
('City Break Deluxe', '2-night city center stay with breakfast and city tour.', 349.99, NULL, 1, 0, NULL),
('Family Fun Package', '3-night family room stay with breakfast for four and late checkout.', 599.99, NULL, 1, 0, NULL)
GO

-- ============================================
-- 10. INSERT PACKAGE ITEMS (30 items)
-- ============================================
INSERT INTO PackageItems (PackageId, RoomTypeId, ServiceId, Quantity, IsDeleted, DeletedAt)
VALUES
-- Package 1: KL City Explorer
(1, 1, NULL, 2, 0, NULL), (1, NULL, 1, 1, 0, NULL), (1, NULL, 2, 2, 0, NULL),
-- Package 2: Malacca Family Fun
(2, 4, NULL, 2, 0, NULL), (2, NULL, 2, 4, 0, NULL), (2, NULL, 3, 1, 0, NULL),
-- Package 3: Langkawi Beach Escape
(3, 7, NULL, 2, 0, NULL), (3, NULL, 2, 2, 0, NULL), (3, NULL, 4, 2, 0, NULL),
-- Package 4: Honeymoon Bliss
(4, 7, NULL, 2, 0, NULL), (4, NULL, 5, 2, 0, NULL), (4, NULL, 6, 1, 0, NULL),
-- Package 5: Business Traveler
(5, 3, NULL, 1, 0, NULL), (5, NULL, 1, 2, 0, NULL), (5, NULL, 11, 1, 0, NULL),
-- Package 6: Adventure Seeker
(6, 1, NULL, 3, 0, NULL), (6, NULL, 7, 1, 0, NULL), (6, NULL, 8, 3, 0, NULL),
-- Package 7: Ipoh Heritage
(7, 8, NULL, 2, 0, NULL), (7, NULL, 2, 2, 0, NULL), (7, NULL, 7, 1, 0, NULL),
-- Package 8: Sabah Adventure
(8, 9, NULL, 3, 0, NULL), (8, NULL, 2, 3, 0, NULL), (8, NULL, 7, 1, 0, NULL),
-- Package 9: Sarawak Cultural
(9, 10, NULL, 2, 0, NULL), (9, NULL, 6, 1, 0, NULL), (9, NULL, 7, 1, 0, NULL),
-- Package 10: Cameron Highlands
(10, 11, NULL, 2, 0, NULL), (10, NULL, 2, 2, 0, NULL), (10, NULL, 7, 1, 0, NULL),
-- Package 11: Weekend Getaway
(11, 5, NULL, 2, 0, NULL), (11, NULL, 2, 2, 0, NULL), (11, NULL, 3, 1, 0, NULL),
-- Package 12: Extended Stay
(12, 6, NULL, 5, 0, NULL), (12, NULL, 2, 5, 0, NULL), (12, NULL, 10, 1, 0, NULL),
-- Package 13: Beach Paradise
(13, 12, NULL, 3, 0, NULL), (13, NULL, 2, 3, 0, NULL), (13, NULL, 15, 3, 0, NULL),
-- Package 14: City Break
(14, 15, NULL, 2, 0, NULL), (14, NULL, 2, 2, 0, NULL), (14, NULL, 7, 1, 0, NULL),
-- Package 15: Family Fun
(15, 4, NULL, 3, 0, NULL), (15, NULL, 2, 4, 0, NULL), (15, NULL, 3, 1, 0, NULL)
GO

-- ============================================
-- 11. INSERT PROMOTIONS (15 promotions)
-- ============================================
INSERT INTO Promotions (Code, Description, Type, Value, StartDate, EndDate, IsActive, LimitPerPhoneNumber, LimitPerPaymentCard, LimitPerDevice, LimitPerUserAccount, MaxUsesPerLimit, MinimumNights, MinimumAmount, MaxTotalUses, IsDeleted, DeletedAt)
VALUES
('WELCOME10', 'Welcome discount - 10% off', 0, 10, DATEADD(day, -30, GETDATE()), DATEADD(day, 30, GETDATE()), 1, 1, 1, 0, 0, 1, NULL, NULL, NULL, 0, NULL),
('SUMMER20', 'Summer special - 20% off', 0, 20, DATEADD(day, -10, GETDATE()), DATEADD(day, 50, GETDATE()), 1, 1, 1, 0, 0, 1, NULL, NULL, NULL, 0, NULL),
('FIXED50', 'Fixed RM50 discount', 1, 50, DATEADD(day, -5, GETDATE()), DATEADD(day, 25, GETDATE()), 1, 1, 1, 0, 0, 1, NULL, NULL, NULL, 0, NULL),
('EARLYBIRD', 'Book early and save 15%', 0, 15, GETDATE(), DATEADD(day, 60, GETDATE()), 1, 1, 1, 0, 0, 1, NULL, NULL, NULL, 0, NULL),
('LONGSTAY', 'Stay 5 nights or more get RM100 off', 1, 100, GETDATE(), DATEADD(day, 90, GETDATE()), 1, 1, 1, 0, 0, 1, 5, NULL, NULL, 0, NULL),
('WEEKEND15', 'Weekend special - 15% off', 0, 15, GETDATE(), DATEADD(day, 45, GETDATE()), 1, 1, 1, 0, 0, 1, NULL, NULL, NULL, 0, NULL),
('NEWYEAR25', 'New Year special - 25% off', 0, 25, GETDATE(), DATEADD(day, 30, GETDATE()), 1, 1, 1, 0, 0, 1, NULL, NULL, NULL, 0, NULL),
('FIXED100', 'Fixed RM100 discount for bookings over RM500', 1, 100, GETDATE(), DATEADD(day, 60, GETDATE()), 1, 1, 1, 0, 0, 1, NULL, 500, NULL, 0, NULL),
('STUDENT10', 'Student discount - 10% off', 0, 10, GETDATE(), DATEADD(day, 90, GETDATE()), 1, 1, 1, 0, 0, 1, NULL, NULL, NULL, 0, NULL),
('SENIOR15', 'Senior citizen discount - 15% off', 0, 15, GETDATE(), DATEADD(day, 90, GETDATE()), 1, 1, 1, 0, 0, 1, NULL, NULL, NULL, 0, NULL),
('GROUP20', 'Group booking discount - 20% off for 3+ rooms', 0, 20, GETDATE(), DATEADD(day, 120, GETDATE()), 1, 1, 1, 0, 0, 1, NULL, NULL, NULL, 0, NULL),
('LOYALTY5', 'Loyalty member discount - 5% off', 0, 5, GETDATE(), DATEADD(day, 365, GETDATE()), 1, 1, 1, 0, 0, 10, NULL, NULL, NULL, 0, NULL),
('FAMILY30', 'Family package discount - 30% off for family rooms', 0, 30, GETDATE(), DATEADD(day, 75, GETDATE()), 1, 1, 1, 0, 0, 2, NULL, NULL, NULL, 0, NULL),
('MIDWEEK20', 'Midweek special - 20% off for Monday-Thursday bookings', 0, 20, GETDATE(), DATEADD(day, 100, GETDATE()), 1, 1, 1, 0, 0, 1, NULL, NULL, NULL, 0, NULL),
('LASTMIN15', 'Last minute booking - 15% off for bookings within 48 hours', 0, 15, GETDATE(), DATEADD(day, 180, GETDATE()), 1, 1, 1, 0, 0, 1, NULL, NULL, NULL, 0, NULL)
GO

-- ============================================
-- 12. INSERT BOOKINGS (15 bookings)
-- ============================================
INSERT INTO Bookings (UserId, RoomId, CheckInDate, CheckOutDate, BookingDate, TotalPrice, Status, PromotionId, PaymentAmount, PaymentMethod, PaymentStatus, TransactionId, PaymentDate, CancellationDate, CancellationReason, RefundAmount, IsDeleted, DeletedAt)
VALUES
(4, 1, DATEADD(day, 10, GETDATE()), DATEADD(day, 13, GETDATE()), DATEADD(day, -5, GETDATE()), 239.97, 1, 1, 239.97, 0, 1, 'TXN-CC-' + CAST(NEWID() AS VARCHAR(36)), DATEADD(day, -5, GETDATE()), NULL, NULL, NULL, 0, NULL),
(5, 4, DATEADD(day, 15, GETDATE()), DATEADD(day, 18, GETDATE()), DATEADD(day, -2, GETDATE()), 389.97, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, 0, NULL),
(7, 7, DATEADD(day, -30, GETDATE()), DATEADD(day, -27, GETDATE()), DATEADD(day, -35, GETDATE()), 599.97, 4, NULL, 599.97, 1, 1, 'TXN-PP-' + CAST(NEWID() AS VARCHAR(36)), DATEADD(day, -30, GETDATE()), NULL, NULL, NULL, 0, NULL),
(8, 9, DATEADD(day, -20, GETDATE()), DATEADD(day, -17, GETDATE()), DATEADD(day, -25, GETDATE()), 479.97, 4, NULL, 479.97, 2, 1, 'TXN-BT-' + CAST(NEWID() AS VARCHAR(36)), DATEADD(day, -20, GETDATE()), NULL, NULL, NULL, 0, NULL),
(9, 11, DATEADD(day, -15, GETDATE()), DATEADD(day, -12, GETDATE()), DATEADD(day, -20, GETDATE()), 329.97, 4, 2, 329.97, 0, 1, 'TXN-CC-' + CAST(NEWID() AS VARCHAR(36)), DATEADD(day, -15, GETDATE()), NULL, NULL, NULL, 0, NULL),
(10, 13, DATEADD(day, -40, GETDATE()), DATEADD(day, -37, GETDATE()), DATEADD(day, -45, GETDATE()), 449.97, 4, NULL, 449.97, 1, 1, 'TXN-PP-' + CAST(NEWID() AS VARCHAR(36)), DATEADD(day, -40, GETDATE()), NULL, NULL, NULL, 0, NULL),
(11, 15, DATEADD(day, -50, GETDATE()), DATEADD(day, -47, GETDATE()), DATEADD(day, -55, GETDATE()), 689.97, 4, 3, 689.97, 0, 1, 'TXN-CC-' + CAST(NEWID() AS VARCHAR(36)), DATEADD(day, -50, GETDATE()), NULL, NULL, NULL, 0, NULL),
(12, 17, DATEADD(day, -60, GETDATE()), DATEADD(day, -57, GETDATE()), DATEADD(day, -65, GETDATE()), 299.97, 4, NULL, 299.97, 2, 1, 'TXN-BT-' + CAST(NEWID() AS VARCHAR(36)), DATEADD(day, -60, GETDATE()), NULL, NULL, NULL, 0, NULL),
(13, 19, DATEADD(day, -70, GETDATE()), DATEADD(day, -67, GETDATE()), DATEADD(day, -75, GETDATE()), 359.97, 4, 4, 359.97, 0, 1, 'TXN-CC-' + CAST(NEWID() AS VARCHAR(36)), DATEADD(day, -70, GETDATE()), NULL, NULL, NULL, 0, NULL),
(4, 2, DATEADD(day, 20, GETDATE()), DATEADD(day, 23, GETDATE()), GETDATE(), 239.97, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, 0, NULL),
(5, 5, DATEADD(day, 25, GETDATE()), DATEADD(day, 28, GETDATE()), DATEADD(day, 1, GETDATE()), 329.97, 1, 5, 329.97, 1, 1, 'TXN-PP-' + CAST(NEWID() AS VARCHAR(36)), DATEADD(day, 1, GETDATE()), NULL, NULL, NULL, 0, NULL),
(7, 8, DATEADD(day, 30, GETDATE()), DATEADD(day, 33, GETDATE()), DATEADD(day, 2, GETDATE()), 389.97, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, 0, NULL),
(8, 10, DATEADD(day, 35, GETDATE()), DATEADD(day, 38, GETDATE()), DATEADD(day, 3, GETDATE()), 419.97, 1, 6, 419.97, 0, 1, 'TXN-CC-' + CAST(NEWID() AS VARCHAR(36)), DATEADD(day, 3, GETDATE()), NULL, NULL, NULL, 0, NULL),
(9, 12, DATEADD(day, 40, GETDATE()), DATEADD(day, 43, GETDATE()), DATEADD(day, 4, GETDATE()), 449.97, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, 0, NULL),
(10, 14, DATEADD(day, 45, GETDATE()), DATEADD(day, 48, GETDATE()), DATEADD(day, 5, GETDATE()), 459.97, 1, 7, 459.97, 2, 1, 'TXN-BT-' + CAST(NEWID() AS VARCHAR(36)), DATEADD(day, 5, GETDATE()), NULL, NULL, NULL, 0, NULL)
GO

-- ============================================
-- 13. INSERT REVIEWS (15 reviews)
-- ============================================
INSERT INTO Reviews (BookingId, UserId, Rating, Comment, ReviewDate, IsDeleted, DeletedAt)
VALUES
(1, 4, 5, 'Excellent stay! The room was clean, comfortable, and the staff was very helpful. Great value for money!', DATEADD(day, -3, GETDATE()), 0, NULL),
(2, 5, 4, 'Good experience overall. The room was spacious and well-maintained. Would stay again.', DATEADD(day, -1, GETDATE()), 0, NULL),
(3, 7, 5, 'Perfect location and great service. Highly recommended!', DATEADD(day, -25, GETDATE()), 0, NULL),
(4, 8, 4, 'Comfortable room with all necessary amenities. Good value.', DATEADD(day, -15, GETDATE()), 0, NULL),
(5, 9, 5, 'Amazing experience! The suite was luxurious and the view was spectacular.', DATEADD(day, -10, GETDATE()), 0, NULL),
(6, 10, 4, 'Great value for money. The location is perfect, close to everything. Room was clean and comfortable.', DATEADD(day, -35, GETDATE()), 0, NULL),
(7, 11, 5, 'Best budget hotel experience I''ve had! The family room was spacious, perfect for our needs. Kids loved it and we''ll be back for sure. Highly recommend!', DATEADD(day, -45, GETDATE()), 0, NULL),
(8, 12, 4, 'Clean, modern, and well-maintained. The twin room was perfect for our business trip. Good amenities and friendly staff. Would stay again.', DATEADD(day, -55, GETDATE()), 0, NULL),
(9, 13, 5, 'Stunning ocean view! The suite exceeded all expectations. Beautiful balcony, comfortable bed, and excellent service. Worth every ringgit. Perfect for a romantic getaway!', DATEADD(day, -65, GETDATE()), 0, NULL),
(11, 5, 5, 'Absolutely fantastic! The staff went above and beyond to make our stay memorable. The room was spotless and the breakfast was delicious. Will definitely return!', DATEADD(day, -2, GETDATE()), 0, NULL),
(13, 8, 4, 'Great hotel with excellent location. Room was clean and had all the amenities we needed. Good value for money.', DATEADD(day, -1, GETDATE()), 0, NULL),
(15, 10, 5, 'Outstanding service and beautiful room. The view was amazing and the staff was very accommodating. Highly recommend this hotel!', GETDATE(), 0, NULL),
(10, 7, 4, 'Very satisfied with our stay. The room was clean, the bed was comfortable, and the location was convenient. Good value for the price.', DATEADD(day, -40, GETDATE()), 0, NULL),
(12, 9, 5, 'Exceptional service! The staff was friendly and helpful. The room exceeded our expectations. Will definitely book again!', DATEADD(day, -50, GETDATE()), 0, NULL),
(14, 11, 4, 'Nice hotel with good amenities. The breakfast was decent and the room was well-maintained. Would recommend for budget travelers.', DATEADD(day, -5, GETDATE()), 0, NULL)
GO

-- ============================================
-- 14. INSERT CONTACT MESSAGES (15 messages)
-- ============================================
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
GO

-- ============================================
-- 15. INSERT NEWSLETTERS (15 subscriptions)
-- ============================================
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
('customer1@example.com', DATEADD(day, -22, GETDATE()), 1, 0, NULL),
('customer2@example.com', DATEADD(day, -18, GETDATE()), 1, 0, NULL),
('customer3@example.com', DATEADD(day, -8, GETDATE()), 1, 0, NULL),
('traveler1@example.com', DATEADD(day, -45, GETDATE()), 1, 0, NULL),
('traveler2@example.com', DATEADD(day, -32, GETDATE()), 1, 0, NULL),
('traveler3@example.com', DATEADD(day, -12, GETDATE()), 0, 0, NULL)
GO

PRINT 'Sample data has been inserted successfully!'
PRINT 'Total records inserted:'
PRINT '  - Hotels: 15'
PRINT '  - Users: 15'
PRINT '  - Amenities: 15'
PRINT '  - Room Types: 20'
PRINT '  - Rooms: 25'
PRINT '  - Room Images: 30'
PRINT '  - Room Type Amenities: 50 relationships'
PRINT '  - Services: 15'
PRINT '  - Packages: 15'
PRINT '  - Package Items: 30'
PRINT '  - Promotions: 15'
PRINT '  - Bookings: 15'
PRINT '  - Reviews: 15'
PRINT '  - Contact Messages: 15'
PRINT '  - Newsletters: 15'
PRINT ''
PRINT 'All tables now contain 15+ sample records for comprehensive testing!'
GO

