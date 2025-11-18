using Assignment.Models;
using Assignment.Services;
using System;
using System.Linq;

namespace Assignment.Models.Data
{
    public static class DbInitializer
    {
        public static void Initialize(HotelDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if the database is already seeded
            if (context.Users.Any())
            {
                return;
            }

            // Seeding Hotels
            var hotels = new Hotel[]
            {
                new Hotel
                {
                    Name = "Budget Inn KL Sentral",
                    Address = "123 Jalan Tun Sambanthan",
                    City = "Kuala Lumpur",
                    PostalCode = "50470",
                    Country = "Malaysia",
                    ContactNumber = "+60-3-2274-0101",
                    ContactEmail = "info@budgetinnkl.com",
                    Description = "A comfortable budget hotel in the heart of KL Sentral. Perfect for business and leisure travelers.",
                    Latitude = 3.1349,
                    Longitude = 101.6869
                },
                new Hotel
                {
                    Name = "Economy Stay Bukit Bintang",
                    Address = "456 Jalan Bukit Bintang",
                    City = "Kuala Lumpur",
                    PostalCode = "55100",
                    Country = "Malaysia",
                    ContactNumber = "+60-3-2142-0202",
                    ContactEmail = "contact@economystaybb.com",
                    Description = "Affordable accommodation with modern amenities in the vibrant Bukit Bintang area.",
                    Latitude = 3.1478,
                    Longitude = 101.7103
                },
                new Hotel
                {
                    Name = "Penang Budget Hotel",
                    Address = "789 Jalan Penang",
                    City = "George Town",
                    PostalCode = "10000",
                    Country = "Malaysia",
                    ContactNumber = "+60-4-261-1234",
                    ContactEmail = "info@penangbudget.com",
                    Description = "Budget-friendly hotel in the heart of George Town, close to heritage sites and local food.",
                    Latitude = 5.4141,
                    Longitude = 100.3288
                },
                new Hotel
                {
                    Name = "Malacca City Inn",
                    Address = "321 Jalan Hang Jebat",
                    City = "Malacca",
                    PostalCode = "75200",
                    Country = "Malaysia",
                    ContactNumber = "+60-6-281-5678",
                    ContactEmail = "contact@malaccacityinn.com",
                    Description = "Affordable stay in historic Malacca, walking distance to Jonker Street and cultural attractions.",
                    Latitude = 2.1940,
                    Longitude = 102.2491
                },
                new Hotel
                {
                    Name = "Johor Bahru Budget Stay",
                    Address = "654 Jalan Wong Ah Fook",
                    City = "Johor Bahru",
                    PostalCode = "80000",
                    Country = "Malaysia",
                    ContactNumber = "+60-7-222-9012",
                    ContactEmail = "info@jbbudgetstay.com",
                    Description = "Convenient budget accommodation near JB city center, perfect for shopping and dining.",
                    Latitude = 1.4927,
                    Longitude = 103.7414
                },
                new Hotel
                {
                    Name = "Langkawi Beach Budget Hotel",
                    Address = "147 Pantai Cenang",
                    City = "Langkawi",
                    PostalCode = "07000",
                    Country = "Malaysia",
                    ContactNumber = "+60-4-955-3456",
                    ContactEmail = "stay@langkawibudget.com",
                    Description = "Budget beachfront hotel in Langkawi with stunning sea views and easy access to attractions.",
                    Latitude = 6.2930,
                    Longitude = 99.7289
                }
            };
            context.Hotels.AddRange(hotels);
            context.SaveChanges();

            // Seeding Users with proper password hashing
            var users = new User[]
            {
                new User
                {
                    FullName = "Admin Hotel",
                    Email = "admin@hotel.com",
                    PasswordHash = PasswordService.HashPassword("Admin123!"),
                    Role = UserRole.Admin,
                    IsEmailVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Manager Smith",
                    Email = "manager@hotel.com",
                    PasswordHash = PasswordService.HashPassword("Manager123!"),
                    Role = UserRole.Manager,
                    IsEmailVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Ahmad Zulkifli",
                    Email = "ahmad@example.com",
                    PasswordHash = PasswordService.HashPassword("Ahmad123!"),
                    Role = UserRole.Customer,
                    IsEmailVerified = true,
                    IsActive = true,
                    PhoneNumber = "+60-12-345-6789",
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Siti Nurhaliza",
                    Email = "siti@example.com",
                    PasswordHash = PasswordService.HashPassword("Siti123!"),
                    Role = UserRole.Customer,
                    IsEmailVerified = true,
                    IsActive = true,
                    PhoneNumber = "+60-19-876-5432",
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Charlie Brown",
                    Email = "charlie@example.com",
                    PasswordHash = PasswordService.HashPassword("Charlie123!"),
                    Role = UserRole.Customer,
                    IsEmailVerified = false,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // Seeding RoomTypes
            var roomTypes = new RoomType[]
            {
                new RoomType
                {
                    Name = "Standard Single Room",
                    Description = "A cozy room for a single traveler with essential amenities. Perfect for solo travelers on a budget.",
                    Occupancy = 1,
                    BasePrice = 79.99m,
                    HotelId = hotels[0].HotelId
                },
                new RoomType
                {
                    Name = "Deluxe Double Room",
                    Description = "Spacious room with two double beds. Ideal for families or groups of up to 4 people.",
                    Occupancy = 4,
                    BasePrice = 129.99m,
                    HotelId = hotels[1].HotelId
                },
                new RoomType
                {
                    Name = "Executive Suite",
                    Description = "Luxurious suite with a king bed and city view. Includes premium amenities and extra space.",
                    Occupancy = 2,
                    BasePrice = 199.99m,
                    HotelId = hotels[2].HotelId
                },
                new RoomType
                {
                    Name = "Family Room",
                    Description = "Large family-friendly room with multiple beds and extra space for children.",
                    Occupancy = 6,
                    BasePrice = 159.99m,
                    HotelId = hotels[3].HotelId
                },
                new RoomType
                {
                    Name = "Premier Twin Room",
                    Description = "Bright twin room with modern furnishings, perfect for friends or business travelers.",
                    Occupancy = 2,
                    BasePrice = 109.99m,
                    HotelId = hotels[4].HotelId
                },
                new RoomType
                {
                    Name = "Studio Apartment",
                    Description = "Fully furnished studio with kitchenette ideal for long stays and remote work.",
                    Occupancy = 3,
                    BasePrice = 149.99m,
                    HotelId = hotels[1].HotelId
                },
                new RoomType
                {
                    Name = "Ocean View Suite",
                    Description = "Premium suite with panoramic ocean views, private balcony, and lounge area.",
                    Occupancy = 2,
                    BasePrice = 229.99m,
                    HotelId = hotels[5].HotelId
                }
            };
            context.RoomTypes.AddRange(roomTypes);
            context.SaveChanges();

            // Seeding Room Images
            var roomImages = new RoomImage[]
            {
                new RoomImage { RoomTypeId = roomTypes[0].RoomTypeId, ImageUrl = "https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?auto=format&fit=crop&w=1280&q=80", Caption = "Standard Single Room" },
                new RoomImage { RoomTypeId = roomTypes[1].RoomTypeId, ImageUrl = "https://images.unsplash.com/photo-1445019980597-93fa8acb246c?auto=format&fit=crop&w=1280&q=80", Caption = "Deluxe Double Room" },
                new RoomImage { RoomTypeId = roomTypes[2].RoomTypeId, ImageUrl = "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1280&q=80", Caption = "Executive Suite" },
                new RoomImage { RoomTypeId = roomTypes[3].RoomTypeId, ImageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1280&q=80", Caption = "Family Room" },
                new RoomImage { RoomTypeId = roomTypes[4].RoomTypeId, ImageUrl = "https://images.unsplash.com/photo-1466978913421-dad2ebd01d17?auto=format&fit=crop&w=1280&q=80", Caption = "Premier Twin Room" },
                new RoomImage { RoomTypeId = roomTypes[5].RoomTypeId, ImageUrl = "https://images.unsplash.com/photo-1523217582562-09d0def993a6?auto=format&fit=crop&w=1280&q=80", Caption = "Studio Apartment" },
                new RoomImage { RoomTypeId = roomTypes[6].RoomTypeId, ImageUrl = "https://images.unsplash.com/photo-1496417263034-38ec4f0b665a?auto=format&fit=crop&w=1280&q=80", Caption = "Ocean View Suite" }
            };
            context.RoomImages.AddRange(roomImages);
            context.SaveChanges();

            // Seeding Rooms
            var rooms = new Room[]
            {
                new Room{RoomNumber="101", RoomTypeId=roomTypes[0].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="102", RoomTypeId=roomTypes[0].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="103", RoomTypeId=roomTypes[0].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="104", RoomTypeId=roomTypes[0].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="105", RoomTypeId=roomTypes[0].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="201", RoomTypeId=roomTypes[1].RoomTypeId, Status=RoomStatus.UnderMaintenance},
                new Room{RoomNumber="202", RoomTypeId=roomTypes[1].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="203", RoomTypeId=roomTypes[1].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="204", RoomTypeId=roomTypes[1].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="205", RoomTypeId=roomTypes[1].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="301", RoomTypeId=roomTypes[2].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="302", RoomTypeId=roomTypes[2].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="303", RoomTypeId=roomTypes[2].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="304", RoomTypeId=roomTypes[2].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="401", RoomTypeId=roomTypes[3].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="402", RoomTypeId=roomTypes[3].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="403", RoomTypeId=roomTypes[3].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="404", RoomTypeId=roomTypes[3].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="501", RoomTypeId=roomTypes[4].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="502", RoomTypeId=roomTypes[4].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="601", RoomTypeId=roomTypes[5].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="602", RoomTypeId=roomTypes[5].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="701", RoomTypeId=roomTypes[6].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="702", RoomTypeId=roomTypes[6].RoomTypeId, Status=RoomStatus.Available}
            };
            context.Rooms.AddRange(rooms);
            context.SaveChanges();

            // Seeding Amenities and linking them to RoomTypes
            var amenities = new Amenity[]
            {
                new Amenity{Name="Free Wi-Fi"},
                new Amenity{Name="Air Conditioning"},
                new Amenity{Name="Flat-Screen TV"},
                new Amenity{Name="Mini Fridge"},
                new Amenity{Name="Coffee Maker"},
                new Amenity{Name="Private Bathroom"},
                new Amenity{Name="Room Service"},
                new Amenity{Name="Safe"},
                new Amenity{Name="Balcony"},
                new Amenity{Name="City View"}
            };
            context.Amenities.AddRange(amenities);
            context.SaveChanges();

            var roomTypeAmenities = new RoomTypeAmenity[]
            {
                // Standard Single Room amenities
                new RoomTypeAmenity{RoomTypeId=roomTypes[0].RoomTypeId, AmenityId=amenities[0].AmenityId}, // Wi-Fi
                new RoomTypeAmenity{RoomTypeId=roomTypes[0].RoomTypeId, AmenityId=amenities[1].AmenityId}, // AC
                new RoomTypeAmenity{RoomTypeId=roomTypes[0].RoomTypeId, AmenityId=amenities[2].AmenityId}, // TV
                new RoomTypeAmenity{RoomTypeId=roomTypes[0].RoomTypeId, AmenityId=amenities[5].AmenityId}, // Private Bathroom
                
                // Deluxe Double Room amenities
                new RoomTypeAmenity{RoomTypeId=roomTypes[1].RoomTypeId, AmenityId=amenities[0].AmenityId}, // Wi-Fi
                new RoomTypeAmenity{RoomTypeId=roomTypes[1].RoomTypeId, AmenityId=amenities[1].AmenityId}, // AC
                new RoomTypeAmenity{RoomTypeId=roomTypes[1].RoomTypeId, AmenityId=amenities[2].AmenityId}, // TV
                new RoomTypeAmenity{RoomTypeId=roomTypes[1].RoomTypeId, AmenityId=amenities[3].AmenityId}, // Mini Fridge
                new RoomTypeAmenity{RoomTypeId=roomTypes[1].RoomTypeId, AmenityId=amenities[4].AmenityId}, // Coffee Maker
                new RoomTypeAmenity{RoomTypeId=roomTypes[1].RoomTypeId, AmenityId=amenities[5].AmenityId}, // Private Bathroom
                
                // Executive Suite amenities (all)
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[0].AmenityId},
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[1].AmenityId},
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[2].AmenityId},
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[3].AmenityId},
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[4].AmenityId},
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[5].AmenityId},
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[6].AmenityId}, // Room Service
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[7].AmenityId}, // Safe
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[8].AmenityId}, // Balcony
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[9].AmenityId}, // City View
                
                // Family Room amenities
                new RoomTypeAmenity{RoomTypeId=roomTypes[3].RoomTypeId, AmenityId=amenities[0].AmenityId},
                new RoomTypeAmenity{RoomTypeId=roomTypes[3].RoomTypeId, AmenityId=amenities[1].AmenityId},
                new RoomTypeAmenity{RoomTypeId=roomTypes[3].RoomTypeId, AmenityId=amenities[2].AmenityId},
                new RoomTypeAmenity{RoomTypeId=roomTypes[3].RoomTypeId, AmenityId=amenities[3].AmenityId},
                new RoomTypeAmenity{RoomTypeId=roomTypes[3].RoomTypeId, AmenityId=amenities[5].AmenityId}
            };
            context.RoomTypeAmenities.AddRange(roomTypeAmenities);
            context.SaveChanges();

            // Seeding Services
            var services = new Service[]
            {
                new Service { Name = "Airport Transfer", Description = "Private airport pick-up and drop-off service within Klang Valley.", Price = 120m },
                new Service { Name = "Breakfast Buffet", Description = "Authentic Malaysian breakfast spread for two guests.", Price = 60m },
                new Service { Name = "Late Checkout", Description = "Extend your checkout time to 4.00 PM.", Price = 80m },
                new Service { Name = "Island Hopping Tour", Description = "Half-day Langkawi tour with hotel transfer.", Price = 250m }
            };
            context.Services.AddRange(services);
            context.SaveChanges();

            // Seeding Packages
            var packages = new Package[]
            {
                new Package
                {
                    Name = "Kuala Lumpur City Explorer",
                    Description = "2-night stay with breakfast and airport transfer in KL Sentral.",
                    TotalPrice = 329.99m,
                    IsActive = true
                },
                new Package
                {
                    Name = "Family Fun Bukit Bintang",
                    Description = "Spacious family room with breakfast for four and late checkout.",
                    TotalPrice = 549.99m,
                    IsActive = true
                },
                new Package
                {
                    Name = "Langkawi Beach Escape",
                    Description = "Ocean-view suite with breakfast and island hopping tour.",
                    TotalPrice = 789.99m,
                    IsActive = true
                }
            };
            context.Packages.AddRange(packages);
            context.SaveChanges();

            var packageItems = new PackageItem[]
            {
                new PackageItem { PackageId = packages[0].PackageId, RoomTypeId = roomTypes[0].RoomTypeId, Quantity = 2 },
                new PackageItem { PackageId = packages[0].PackageId, ServiceId = services[0].ServiceId, Quantity = 1 },
                new PackageItem { PackageId = packages[0].PackageId, ServiceId = services[1].ServiceId, Quantity = 2 },

                new PackageItem { PackageId = packages[1].PackageId, RoomTypeId = roomTypes[3].RoomTypeId, Quantity = 2 },
                new PackageItem { PackageId = packages[1].PackageId, ServiceId = services[1].ServiceId, Quantity = 4 },
                new PackageItem { PackageId = packages[1].PackageId, ServiceId = services[2].ServiceId, Quantity = 1 },

                new PackageItem { PackageId = packages[2].PackageId, RoomTypeId = roomTypes[6].RoomTypeId, Quantity = 2 },
                new PackageItem { PackageId = packages[2].PackageId, ServiceId = services[1].ServiceId, Quantity = 2 },
                new PackageItem { PackageId = packages[2].PackageId, ServiceId = services[3].ServiceId, Quantity = 2 }
            };
            context.PackageItems.AddRange(packageItems);
            context.SaveChanges();

            // Seeding Promotions
            var promotions = new Promotion[]
            {
                new Promotion
                {
                    Code = "WELCOME10",
                    Description = "Welcome discount - 10% off",
                    Type = DiscountType.Percentage,
                    Value = 10,
                    StartDate = DateTime.Now.AddDays(-30),
                    EndDate = DateTime.Now.AddDays(30),
                    IsActive = true
                },
                new Promotion
                {
                    Code = "SUMMER20",
                    Description = "Summer special - 20% off",
                    Type = DiscountType.Percentage,
                    Value = 20,
                    StartDate = DateTime.Now.AddDays(-10),
                    EndDate = DateTime.Now.AddDays(50),
                    IsActive = true
                },
                new Promotion
                {
                    Code = "FIXED50",
                    Description = "Fixed RM50 discount",
                    Type = DiscountType.FixedAmount,
                    Value = 50,
                    StartDate = DateTime.Now.AddDays(-5),
                    EndDate = DateTime.Now.AddDays(25),
                    IsActive = true
                }
            };
            context.Promotions.AddRange(promotions);
            context.SaveChanges();

            // Seeding Bookings
            var bookings = new Booking[]
            {
                new Booking
                {
                    UserId = users.Single(u => u.Email == "ahmad@example.com").UserId,
                    RoomId = rooms.Single(r => r.RoomNumber == "101").RoomId,
                    CheckInDate = DateTime.Now.AddDays(10),
                    CheckOutDate = DateTime.Now.AddDays(13),
                    TotalPrice = 239.97m,
                    Status = BookingStatus.Confirmed,
                    BookingDate = DateTime.Now.AddDays(-5),
                    PromotionId = promotions[0].PromotionId
                },
                new Booking
                {
                    UserId = users.Single(u => u.Email == "siti@example.com").UserId,
                    RoomId = rooms.Single(r => r.RoomNumber == "202").RoomId,
                    CheckInDate = DateTime.Now.AddDays(15),
                    CheckOutDate = DateTime.Now.AddDays(18),
                    TotalPrice = 389.97m,
                    Status = BookingStatus.Pending,
                    BookingDate = DateTime.Now.AddDays(-2)
                }
            };
            context.Bookings.AddRange(bookings);
            context.SaveChanges();

            // Seeding Payments
            var payments = new Payment[]
            {
                new Payment
                {
                    BookingId = bookings[0].BookingId,
                    Amount = bookings[0].TotalPrice,
                    PaymentMethod = PaymentMethod.CreditCard,
                    Status = PaymentStatus.Completed,
                    TransactionId = "TXN-" + DateTime.Now.Ticks,
                    PaymentDate = DateTime.Now.AddDays(-5)
                }
            };
            context.Payments.AddRange(payments);
            context.SaveChanges();

            // Seeding Reviews
            var reviews = new Review[]
            {
                new Review
                {
                    BookingId = bookings[0].BookingId,
                    UserId = bookings[0].UserId,
                    Rating = 5,
                    Comment = "Excellent stay! The room was clean, comfortable, and the staff was very helpful. Great value for money!",
                    ReviewDate = DateTime.Now.AddDays(-3)
                },
                new Review
                {
                    BookingId = bookings[1].BookingId,
                    UserId = bookings[1].UserId,
                    Rating = 4,
                    Comment = "Good experience overall. The room was spacious and well-maintained. Would stay again.",
                    ReviewDate = DateTime.Now.AddDays(-1)
                }
            };
            context.Reviews.AddRange(reviews);
            context.SaveChanges();

            // Add more sample bookings with reviews for better review counts
            var additionalBookings = new Booking[]
            {
                new Booking
                {
                    UserId = users.Single(u => u.Email == "ahmad@example.com").UserId,
                    RoomId = rooms.Single(r => r.RoomNumber == "102").RoomId,
                    CheckInDate = DateTime.Now.AddDays(-30),
                    CheckOutDate = DateTime.Now.AddDays(-27),
                    TotalPrice = 239.97m,
                    Status = BookingStatus.CheckedOut,
                    BookingDate = DateTime.Now.AddDays(-35)
                },
                new Booking
                {
                    UserId = users.Single(u => u.Email == "siti@example.com").UserId,
                    RoomId = rooms.Single(r => r.RoomNumber == "203").RoomId,
                    CheckInDate = DateTime.Now.AddDays(-20),
                    CheckOutDate = DateTime.Now.AddDays(-17),
                    TotalPrice = 389.97m,
                    Status = BookingStatus.CheckedOut,
                    BookingDate = DateTime.Now.AddDays(-25)
                },
                new Booking
                {
                    UserId = users.Single(u => u.Email == "ahmad@example.com").UserId,
                    RoomId = rooms.Single(r => r.RoomNumber == "301").RoomId,
                    CheckInDate = DateTime.Now.AddDays(-15),
                    CheckOutDate = DateTime.Now.AddDays(-12),
                    TotalPrice = 599.97m,
                    Status = BookingStatus.CheckedOut,
                    BookingDate = DateTime.Now.AddDays(-20)
                }
            };
            context.Bookings.AddRange(additionalBookings);
            context.SaveChanges();

            var additionalReviews = new Review[]
            {
                new Review
                {
                    BookingId = additionalBookings[0].BookingId,
                    UserId = additionalBookings[0].UserId,
                    Rating = 5,
                    Comment = "Perfect location and great service. Highly recommended!",
                    ReviewDate = DateTime.Now.AddDays(-25)
                },
                new Review
                {
                    BookingId = additionalBookings[1].BookingId,
                    UserId = additionalBookings[1].UserId,
                    Rating = 4,
                    Comment = "Comfortable room with all necessary amenities. Good value.",
                    ReviewDate = DateTime.Now.AddDays(-15)
                },
                new Review
                {
                    BookingId = additionalBookings[2].BookingId,
                    UserId = additionalBookings[2].UserId,
                    Rating = 5,
                    Comment = "Amazing experience! The suite was luxurious and the view was spectacular.",
                    ReviewDate = DateTime.Now.AddDays(-10)
                }
            };
            context.Reviews.AddRange(additionalReviews);
            context.SaveChanges();
        }
    }
}