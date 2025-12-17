using Assignment.Models;
using Assignment.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Assignment.Models.Data
{
    /// <summary>
    /// Static class responsible for seeding the database with initial data.
    /// Creates sample hotels, users, rooms, amenities, packages, services, promotions, bookings, and reviews
    /// if they don't already exist in the database.
    /// This ensures the application has data to work with on first run.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Initializes the database by seeding it with initial data.
        /// Only seeds data if it doesn't already exist to avoid duplicates.
        /// </summary>
        /// <param name="context">The database context to seed.</param>
        public static void Initialize(HotelDbContext context)
        {
            // Don't use EnsureCreated() - let migrations handle database creation
            // context.Database.EnsureCreated();

            // Check if we need to seed - check each component independently
            // This allows partial seeding if some data already exists
            bool hasUsers = context.Users.Any();
            bool hasHotels = context.Hotels.Any();
            bool hasPackages = context.Packages.Any();
            bool hasServices = context.Services.Any();
            bool hasReviews = context.Reviews.Any();
            
            // If basic data exists but reviews don't, we'll seed reviews at the end
            bool shouldSkipMainSeeding = hasUsers && hasHotels && hasPackages && hasServices;
            
            // If everything exists including reviews, skip seeding entirely
            if (shouldSkipMainSeeding && hasReviews)
            {
                return;
            }

            // Only run main seeding if data doesn't exist
            if (!shouldSkipMainSeeding)
            {
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
                    Description = "A comfortable budget hotel in the heart of KL Sentral. Perfect for business and leisure travelers."
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
                    Description = "Affordable accommodation with modern amenities in the vibrant Bukit Bintang area."
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
                    Description = "Budget-friendly hotel in the heart of George Town, close to heritage sites and local food."
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
                    Description = "Affordable stay in historic Malacca, walking distance to Jonker Street and cultural attractions."
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
                    Description = "Convenient budget accommodation near JB city center, perfect for shopping and dining."
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
                    Description = "Budget beachfront hotel in Langkawi with stunning sea views and easy access to attractions."
                },
                new Hotel
                {
                    Name = "Ipoh Heritage Budget Inn",
                    Address = "258 Jalan Sultan Yussuf",
                    City = "Ipoh",
                    PostalCode = "30000",
                    Country = "Malaysia",
                    ContactNumber = "+60-5-241-7890",
                    ContactEmail = "info@ipohheritage.com",
                    Description = "Charming budget inn in the heart of Ipoh's old town, surrounded by famous food stalls and heritage buildings."
                },
                new Hotel
                {
                    Name = "Kota Kinabalu Budget Hotel",
                    Address = "369 Jalan Gaya",
                    City = "Kota Kinabalu",
                    PostalCode = "88000",
                    Country = "Malaysia",
                    ContactNumber = "+60-88-234-5678",
                    ContactEmail = "contact@kkbudget.com",
                    Description = "Affordable accommodation in KK city center, close to waterfront and shopping areas."
                },
                new Hotel
                {
                    Name = "Kuching Riverside Budget Stay",
                    Address = "741 Jalan Main Bazaar",
                    City = "Kuching",
                    PostalCode = "93000",
                    Country = "Malaysia",
                    ContactNumber = "+60-82-456-7890",
                    ContactEmail = "stay@kuchingbudget.com",
                    Description = "Budget-friendly hotel along the Sarawak River, perfect for exploring Kuching's cultural heritage."
                },
                new Hotel
                {
                    Name = "Cameron Highlands Budget Lodge",
                    Address = "852 Jalan Persiaran Camellia",
                    City = "Cameron Highlands",
                    PostalCode = "39000",
                    Country = "Malaysia",
                    ContactNumber = "+60-5-491-2345",
                    ContactEmail = "info@cameronbudget.com",
                    Description = "Cozy budget lodge in the cool highlands, surrounded by tea plantations and strawberry farms."
                }
            };
            
            // Only add hotels if they don't exist
            if (!hasHotels)
            {
                context.Hotels.AddRange(hotels);
                context.SaveChanges();
            }
            else
            {
                // Get existing hotels for reference
                hotels = context.Hotels.ToArray();
            }

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
                },
                new User
                {
                    FullName = "Sarah Tan",
                    Email = "sarah@example.com",
                    PasswordHash = PasswordService.HashPassword("Sarah123!"),
                    Role = UserRole.Customer,
                    IsEmailVerified = true,
                    IsActive = true,
                    PhoneNumber = "+60-16-123-4567",
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Mohammad Ali",
                    Email = "ali@example.com",
                    PasswordHash = PasswordService.HashPassword("Ali123!"),
                    Role = UserRole.Customer,
                    IsEmailVerified = true,
                    IsActive = true,
                    PhoneNumber = "+60-17-234-5678",
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Lisa Wong",
                    Email = "lisa@example.com",
                    PasswordHash = PasswordService.HashPassword("Lisa123!"),
                    Role = UserRole.Customer,
                    IsEmailVerified = true,
                    IsActive = true,
                    PhoneNumber = "+60-18-345-6789",
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "David Lee",
                    Email = "david@example.com",
                    PasswordHash = PasswordService.HashPassword("David123!"),
                    Role = UserRole.Customer,
                    IsEmailVerified = true,
                    IsActive = true,
                    PhoneNumber = "+60-19-456-7890",
                    CreatedAt = DateTime.Now
                }
            };
            
            // Only add users if they don't exist
            if (!hasUsers)
            {
                context.Users.AddRange(users);
                context.SaveChanges();
            }
            else
            {
                // Get existing users for reference
                users = context.Users.ToArray();
            }

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
                },
                new RoomType
                {
                    Name = "Heritage Double Room",
                    Description = "Charming room with colonial-style furnishings in the heart of historic Ipoh.",
                    Occupancy = 2,
                    BasePrice = 99.99m,
                    HotelId = hotels[6].HotelId
                },
                new RoomType
                {
                    Name = "Sabah View Room",
                    Description = "Comfortable room with mountain or sea view, perfect for exploring Kota Kinabalu.",
                    Occupancy = 2,
                    BasePrice = 119.99m,
                    HotelId = hotels[7].HotelId
                },
                new RoomType
                {
                    Name = "Riverside Deluxe",
                    Description = "Spacious room overlooking the Sarawak River with modern amenities.",
                    Occupancy = 3,
                    BasePrice = 139.99m,
                    HotelId = hotels[8].HotelId
                },
                new RoomType
                {
                    Name = "Highland Cozy Room",
                    Description = "Warm and comfortable room perfect for the cool highland climate.",
                    Occupancy = 2,
                    BasePrice = 89.99m,
                    HotelId = hotels[9].HotelId
                }
            };
            
            // Only add roomTypes if they don't exist
            if (!context.RoomTypes.Any())
            {
                context.RoomTypes.AddRange(roomTypes);
                context.SaveChanges();
            }
            else
            {
                // Check if specific hotels need room types (Ipoh, Kota Kinabalu, Kuching, Cameron Highlands)
                var ipohHotel = context.Hotels.FirstOrDefault(h => h.City == "Ipoh");
                var kkHotel = context.Hotels.FirstOrDefault(h => h.City == "Kota Kinabalu");
                var kuchingHotel = context.Hotels.FirstOrDefault(h => h.City == "Kuching");
                var cameronHotel = context.Hotels.FirstOrDefault(h => h.City == "Cameron Highlands");
                
                var newRoomTypes = new List<RoomType>();
                
                // Add room type for Ipoh if it doesn't have one
                if (ipohHotel != null && !context.RoomTypes.Any(rt => rt.HotelId == ipohHotel.HotelId))
                {
                    newRoomTypes.Add(new RoomType
                    {
                        Name = "Heritage Double Room",
                        Description = "Charming room with colonial-style furnishings in the heart of historic Ipoh.",
                        Occupancy = 2,
                        BasePrice = 99.99m,
                        HotelId = ipohHotel.HotelId
                    });
                }
                
                // Add room type for Kota Kinabalu if it doesn't have one
                if (kkHotel != null && !context.RoomTypes.Any(rt => rt.HotelId == kkHotel.HotelId))
                {
                    newRoomTypes.Add(new RoomType
                    {
                        Name = "Sabah View Room",
                        Description = "Comfortable room with mountain or sea view, perfect for exploring Kota Kinabalu.",
                        Occupancy = 2,
                        BasePrice = 119.99m,
                        HotelId = kkHotel.HotelId
                    });
                }
                
                // Add room type for Kuching if it doesn't have one
                if (kuchingHotel != null && !context.RoomTypes.Any(rt => rt.HotelId == kuchingHotel.HotelId))
                {
                    newRoomTypes.Add(new RoomType
                    {
                        Name = "Riverside Deluxe",
                        Description = "Spacious room overlooking the Sarawak River with modern amenities.",
                        Occupancy = 3,
                        BasePrice = 139.99m,
                        HotelId = kuchingHotel.HotelId
                    });
                }
                
                // Add room type for Cameron Highlands if it doesn't have one
                if (cameronHotel != null && !context.RoomTypes.Any(rt => rt.HotelId == cameronHotel.HotelId))
                {
                    newRoomTypes.Add(new RoomType
                    {
                        Name = "Highland Cozy Room",
                        Description = "Warm and comfortable room perfect for the cool highland climate.",
                        Occupancy = 2,
                        BasePrice = 89.99m,
                        HotelId = cameronHotel.HotelId
                    });
                }
                
                if (newRoomTypes.Any())
                {
                    context.RoomTypes.AddRange(newRoomTypes);
                    context.SaveChanges();
                    
                    // Add rooms for the new room types
                    var newRooms = new List<Room>();
                    var roomNumberCounter = 1200; // Start from 1200 to avoid conflicts
                    
                    foreach (var newRoomType in newRoomTypes)
                    {
                        // Add 2-3 rooms for each new room type with unique room numbers
                        for (int i = 0; i < 3; i++)
                        {
                            var roomNumber = roomNumberCounter++.ToString();
                            // Ensure room number doesn't exist
                            while (context.Rooms.Any(r => r.RoomNumber == roomNumber))
                            {
                                roomNumber = roomNumberCounter++.ToString();
                            }
                            
                            newRooms.Add(new Room
                            {
                                RoomNumber = roomNumber,
                                RoomTypeId = newRoomType.RoomTypeId,
                                Status = RoomStatus.Available
                            });
                        }
                    }
                    
                    if (newRooms.Any())
                    {
                        context.Rooms.AddRange(newRooms);
                        context.SaveChanges();
                    }
                    
                    // Add room images for the new room types
                    var newRoomImages = new List<RoomImage>();
                    var imageUrls = new[]
                    {
                        "https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?auto=format&fit=crop&w=1280&q=80", // Heritage
                        "https://images.unsplash.com/photo-1445019980597-93fa8acb246c?auto=format&fit=crop&w=1280&q=80", // Sabah
                        "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1280&q=80", // Riverside
                        "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1280&q=80"  // Highland
                    };
                    
                    for (int i = 0; i < newRoomTypes.Count && i < imageUrls.Length; i++)
                    {
                        if (!context.RoomImages.Any(ri => ri.RoomTypeId == newRoomTypes[i].RoomTypeId))
                        {
                            newRoomImages.Add(new RoomImage
                            {
                                RoomTypeId = newRoomTypes[i].RoomTypeId,
                                ImageUrl = imageUrls[i],
                                Caption = newRoomTypes[i].Name
                            });
                        }
                    }
                    
                    if (newRoomImages.Any())
                    {
                        context.RoomImages.AddRange(newRoomImages);
                        context.SaveChanges();
                    }
                }
                
                // Get existing roomTypes for reference
                roomTypes = context.RoomTypes.ToArray();
            }

            // Seeding Room Images - only add if they don't exist
            if (!context.RoomImages.Any())
            {
                var roomImages = new List<RoomImage>();
                
                // Add images for all room types that don't have images
                foreach (var rt in roomTypes)
                {
                    if (!context.RoomImages.Any(ri => ri.RoomTypeId == rt.RoomTypeId))
                    {
                        // Assign appropriate image based on room type index or name
                        string imageUrl;
                        string caption = rt.Name;
                        
                        if (rt.Name.Contains("Heritage") || rt.Name.Contains("Ipoh"))
                            imageUrl = "https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?auto=format&fit=crop&w=1280&q=80";
                        else if (rt.Name.Contains("Sabah") || rt.Name.Contains("Kota Kinabalu"))
                            imageUrl = "https://images.unsplash.com/photo-1445019980597-93fa8acb246c?auto=format&fit=crop&w=1280&q=80";
                        else if (rt.Name.Contains("Riverside") || rt.Name.Contains("Kuching"))
                            imageUrl = "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1280&q=80";
                        else if (rt.Name.Contains("Highland") || rt.Name.Contains("Cameron"))
                            imageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1280&q=80";
                        else if (rt.Name.Contains("Ocean") || rt.Name.Contains("Langkawi"))
                            imageUrl = "https://images.unsplash.com/photo-1496417263034-38ec4f0b665a?auto=format&fit=crop&w=1280&q=80";
                        else if (rt.Name.Contains("Studio") || rt.Name.Contains("Apartment"))
                            imageUrl = "https://images.unsplash.com/photo-1523217582562-09d0def993a6?auto=format&fit=crop&w=1280&q=80";
                        else if (rt.Name.Contains("Executive") || rt.Name.Contains("Suite"))
                            imageUrl = "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1280&q=80";
                        else if (rt.Name.Contains("Family"))
                            imageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1280&q=80";
                        else if (rt.Name.Contains("Twin"))
                            imageUrl = "https://images.unsplash.com/photo-1466978913421-dad2ebd01d17?auto=format&fit=crop&w=1280&q=80";
                        else
                            imageUrl = "https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?auto=format&fit=crop&w=1280&q=80";
                        
                        roomImages.Add(new RoomImage
                        {
                            RoomTypeId = rt.RoomTypeId,
                            ImageUrl = imageUrl,
                            Caption = caption
                        });
                    }
                }
                
                if (roomImages.Any())
                {
                    context.RoomImages.AddRange(roomImages);
                    context.SaveChanges();
                }
            }

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
                new Room{RoomNumber="702", RoomTypeId=roomTypes[6].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="801", RoomTypeId=roomTypes[7].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="802", RoomTypeId=roomTypes[7].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="803", RoomTypeId=roomTypes[7].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="901", RoomTypeId=roomTypes[8].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="902", RoomTypeId=roomTypes[8].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="1001", RoomTypeId=roomTypes[9].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="1002", RoomTypeId=roomTypes[9].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="1101", RoomTypeId=roomTypes[10].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="1102", RoomTypeId=roomTypes[10].RoomTypeId, Status=RoomStatus.Available}
            };
            
            // Only add rooms if they don't exist, and avoid duplicates
            if (!context.Rooms.Any())
            {
                context.Rooms.AddRange(rooms);
                context.SaveChanges();
            }
            else
            {
                // Add missing rooms for existing room types, avoiding duplicates
                var existingRoomNumbers = context.Rooms.Select(r => r.RoomNumber).ToList();
                var roomsToAdd = rooms.Where(r => !existingRoomNumbers.Contains(r.RoomNumber)).ToList();
                if (roomsToAdd.Any())
                {
                    context.Rooms.AddRange(roomsToAdd);
                    context.SaveChanges();
                }
            }

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
                new Service { Name = "Island Hopping Tour", Description = "Half-day Langkawi tour with hotel transfer.", Price = 250m },
                new Service { Name = "Spa Treatment", Description = "60-minute aromatherapy massage for one.", Price = 150m },
                new Service { Name = "Candlelight Dinner", Description = "Romantic 3-course dinner by the beach or city view.", Price = 300m },
                new Service { Name = "City Tour", Description = "Guided tour of historical landmarks and cultural sites.", Price = 100m },
                new Service { Name = "Car Rental", Description = "Daily car rental (compact sedan) including insurance.", Price = 180m },
                new Service { Name = "Laundry Service", Description = "Same-day laundry and dry cleaning service.", Price = 50m },
                new Service { Name = "Room Upgrade", Description = "Upgrade to a higher category room (subject to availability).", Price = 100m },
                new Service { Name = "Wi-Fi Premium", Description = "High-speed premium Wi-Fi for multiple devices.", Price = 30m },
                new Service { Name = "Pet Care Service", Description = "Pet sitting and care service for your furry friends.", Price = 90m },
                new Service { Name = "Concierge Service", Description = "Personal concierge assistance for restaurant reservations and activities.", Price = 70m },
                new Service { Name = "Gym Access", Description = "24/7 access to fully equipped fitness center.", Price = 40m },
                new Service { Name = "Pool Access", Description = "Access to infinity pool and poolside facilities.", Price = 35m }
            };
            
            // Only add services if they don't exist
            if (!hasServices)
            {
                context.Services.AddRange(services);
                context.SaveChanges();
            }
            else
            {
                // Get existing services for reference
                services = context.Services.ToArray();
            }

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
                    Name = "Malacca Family Fun Package",
                    Description = "Spacious family room with breakfast for four and late checkout in historic Malacca.",
                    TotalPrice = 549.99m,
                    IsActive = true
                },
                new Package
                {
                    Name = "Langkawi Beach Escape",
                    Description = "Ocean-view suite with breakfast and island hopping tour.",
                    TotalPrice = 789.99m,
                    IsActive = true
                },
                new Package
                {
                    Name = "Honeymoon Bliss",
                    Description = "Romantic getaway with spa treatment and candlelight dinner.",
                    TotalPrice = 999.99m,
                    IsActive = true
                },
                new Package
                {
                    Name = "Business Traveler",
                    Description = "Executive suite with airport transfer and high-speed Wi-Fi.",
                    TotalPrice = 450.00m,
                    IsActive = true
                },
                new Package
                {
                    Name = "Adventure Seeker",
                    Description = "City tour and car rental for exploring at your own pace.",
                    TotalPrice = 399.99m,
                    IsActive = true
                },
                new Package
                {
                    Name = "Ipoh Heritage Experience",
                    Description = "2-night stay in heritage room with city tour and breakfast.",
                    TotalPrice = 279.99m,
                    IsActive = true
                },
                new Package
                {
                    Name = "Sabah Adventure Package",
                    Description = "3-night stay with mountain tour and breakfast buffet.",
                    TotalPrice = 449.99m,
                    IsActive = true
                },
                new Package
                {
                    Name = "Sarawak Cultural Journey",
                    Description = "2-night riverside stay with cultural tour and traditional dinner.",
                    TotalPrice = 379.99m,
                    IsActive = true
                },
                new Package
                {
                    Name = "Cameron Highlands Retreat",
                    Description = "2-night highland stay with tea plantation tour and breakfast.",
                    TotalPrice = 249.99m,
                    IsActive = true
                },
                new Package
                {
                    Name = "Weekend Getaway Special",
                    Description = "2-night stay with late checkout and breakfast for two.",
                    TotalPrice = 299.99m,
                    IsActive = true
                },
                new Package
                {
                    Name = "Extended Stay Value",
                    Description = "5-night stay with complimentary breakfast and room upgrade.",
                    TotalPrice = 699.99m,
                    IsActive = true
                }
            };
            
            // Only add packages if they don't exist
            if (!hasPackages)
            {
                context.Packages.AddRange(packages);
                context.SaveChanges();
            }
            else
            {
                // Get existing packages for reference
                packages = context.Packages.ToArray();
            }

            // Only add packageItems if they don't exist (regardless of whether packages exist)
            // This ensures package items are always seeded even if packages were created earlier
            if (!context.PackageItems.Any() && roomTypes.Length > 0 && services.Length > 0)
            {
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
                new PackageItem { PackageId = packages[2].PackageId, ServiceId = services[3].ServiceId, Quantity = 2 },

                // Honeymoon Bliss
                new PackageItem { PackageId = packages[3].PackageId, RoomTypeId = roomTypes[6].RoomTypeId, Quantity = 2 },
                new PackageItem { PackageId = packages[3].PackageId, ServiceId = services[4].ServiceId, Quantity = 2 }, // Spa
                new PackageItem { PackageId = packages[3].PackageId, ServiceId = services[5].ServiceId, Quantity = 1 }, // Dinner

                // Business Traveler
                new PackageItem { PackageId = packages[4].PackageId, RoomTypeId = roomTypes[2].RoomTypeId, Quantity = 1 },
                new PackageItem { PackageId = packages[4].PackageId, ServiceId = services[0].ServiceId, Quantity = 2 }, // Transfer

                // Adventure Seeker
                new PackageItem { PackageId = packages[5].PackageId, RoomTypeId = roomTypes[0].RoomTypeId, Quantity = 3 },
                new PackageItem { PackageId = packages[5].PackageId, ServiceId = services[6].ServiceId, Quantity = 1 }, // City Tour
                new PackageItem { PackageId = packages[5].PackageId, ServiceId = services[7].ServiceId, Quantity = 3 }, // Car Rental

                // Ipoh Heritage Experience
                new PackageItem { PackageId = packages[6].PackageId, RoomTypeId = roomTypes[7].RoomTypeId, Quantity = 2 },
                new PackageItem { PackageId = packages[6].PackageId, ServiceId = services[1].ServiceId, Quantity = 2 }, // Breakfast
                new PackageItem { PackageId = packages[6].PackageId, ServiceId = services[6].ServiceId, Quantity = 1 }, // City Tour

                // Sabah Adventure Package
                new PackageItem { PackageId = packages[7].PackageId, RoomTypeId = roomTypes[8].RoomTypeId, Quantity = 3 },
                new PackageItem { PackageId = packages[7].PackageId, ServiceId = services[1].ServiceId, Quantity = 3 }, // Breakfast
                new PackageItem { PackageId = packages[7].PackageId, ServiceId = services[6].ServiceId, Quantity = 1 }, // City Tour

                // Sarawak Cultural Journey
                new PackageItem { PackageId = packages[8].PackageId, RoomTypeId = roomTypes[9].RoomTypeId, Quantity = 2 },
                new PackageItem { PackageId = packages[8].PackageId, ServiceId = services[5].ServiceId, Quantity = 1 }, // Candlelight Dinner
                new PackageItem { PackageId = packages[8].PackageId, ServiceId = services[6].ServiceId, Quantity = 1 }, // City Tour

                // Cameron Highlands Retreat
                new PackageItem { PackageId = packages[9].PackageId, RoomTypeId = roomTypes[10].RoomTypeId, Quantity = 2 },
                new PackageItem { PackageId = packages[9].PackageId, ServiceId = services[1].ServiceId, Quantity = 2 }, // Breakfast
                new PackageItem { PackageId = packages[9].PackageId, ServiceId = services[6].ServiceId, Quantity = 1 }, // City Tour

                // Weekend Getaway Special (use roomTypes[4] - Premier Twin Room from Johor Bahru)
                new PackageItem { PackageId = packages[10].PackageId, RoomTypeId = roomTypes[4].RoomTypeId, Quantity = 2 },
                new PackageItem { PackageId = packages[10].PackageId, ServiceId = services[1].ServiceId, Quantity = 2 }, // Breakfast
                new PackageItem { PackageId = packages[10].PackageId, ServiceId = services[2].ServiceId, Quantity = 1 }, // Late Checkout

                // Extended Stay Value (use roomTypes[5] - Studio Apartment from Bukit Bintang)
                new PackageItem { PackageId = packages[11].PackageId, RoomTypeId = roomTypes[5].RoomTypeId, Quantity = 5 },
                new PackageItem { PackageId = packages[11].PackageId, ServiceId = services[1].ServiceId, Quantity = 5 }, // Breakfast
                new PackageItem { PackageId = packages[11].PackageId, ServiceId = services[9].ServiceId, Quantity = 1 }  // Room Upgrade
                };
                context.PackageItems.AddRange(packageItems);
                context.SaveChanges();
            }

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
                },
                new Promotion
                {
                    Code = "EARLYBIRD",
                    Description = "Book early and save 15%",
                    Type = DiscountType.Percentage,
                    Value = 15,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(60),
                    IsActive = true
                },
                new Promotion
                {
                    Code = "LONGSTAY",
                    Description = "Stay 5 nights or more get RM100 off",
                    Type = DiscountType.FixedAmount,
                    Value = 100,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(90),
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

            // Update bookings with payment information (Payment merged into Booking)
            bookings[0].PaymentAmount = bookings[0].TotalPrice;
            bookings[0].PaymentMethod = PaymentMethod.CreditCard;
            bookings[0].PaymentStatus = PaymentStatus.Completed;
            bookings[0].TransactionId = "TXN-" + DateTime.Now.Ticks;
            bookings[0].PaymentDate = DateTime.Now.AddDays(-5);
            context.SaveChanges();

            // Seeding Reviews
            var reviews = new Review[]
            {
                new Review
                {
                    BookingId = bookings[0].BookingId,
                    Rating = 5,
                    Comment = "Excellent stay! The room was clean, comfortable, and the staff was very helpful. Great value for money!",
                    ReviewDate = DateTime.Now.AddDays(-3)
                },
                new Review
                {
                    BookingId = bookings[1].BookingId,
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
            
            // Add payment information to additional bookings (Payment merged into Booking)
            additionalBookings[0].PaymentAmount = additionalBookings[0].TotalPrice;
            additionalBookings[0].PaymentMethod = PaymentMethod.CreditCard;
            additionalBookings[0].PaymentStatus = PaymentStatus.Completed;
            additionalBookings[0].TransactionId = "TXN-CC-" + DateTime.Now.AddDays(-25).Ticks;
            additionalBookings[0].PaymentDate = DateTime.Now.AddDays(-30);
            
            additionalBookings[1].PaymentAmount = additionalBookings[1].TotalPrice;
            additionalBookings[1].PaymentMethod = PaymentMethod.PayPal;
            additionalBookings[1].PaymentStatus = PaymentStatus.Completed;
            additionalBookings[1].TransactionId = "TXN-PP-" + DateTime.Now.AddDays(-20).Ticks;
            additionalBookings[1].PaymentDate = DateTime.Now.AddDays(-25);
            
            additionalBookings[2].PaymentAmount = additionalBookings[2].TotalPrice;
            additionalBookings[2].PaymentMethod = PaymentMethod.BankTransfer;
            additionalBookings[2].PaymentStatus = PaymentStatus.Completed;
            additionalBookings[2].TransactionId = "TXN-BT-" + DateTime.Now.AddDays(-15).Ticks;
            additionalBookings[2].PaymentDate = DateTime.Now.AddDays(-20);
            context.SaveChanges();

            var additionalReviews = new Review[]
            {
                new Review
                {
                    BookingId = additionalBookings[0].BookingId,
                    Rating = 5,
                    Comment = "Perfect location and great service. Highly recommended!",
                    ReviewDate = DateTime.Now.AddDays(-25)
                },
                new Review
                {
                    BookingId = additionalBookings[1].BookingId,
                    Rating = 4,
                    Comment = "Comfortable room with all necessary amenities. Good value.",
                    ReviewDate = DateTime.Now.AddDays(-15)
                },
                new Review
                {
                    BookingId = additionalBookings[2].BookingId,
                    Rating = 5,
                    Comment = "Amazing experience! The suite was luxurious and the view was spectacular.",
                    ReviewDate = DateTime.Now.AddDays(-10)
                }
            };
            context.Reviews.AddRange(additionalReviews);
            context.SaveChanges();

            // Add even more bookings and reviews for better data
            var moreBookings = new Booking[]
            {
                new Booking
                {
                    UserId = users.Single(u => u.Email == "sarah@example.com").UserId,
                    RoomId = rooms.Single(r => r.RoomNumber == "302").RoomId,
                    CheckInDate = DateTime.Now.AddDays(-40),
                    CheckOutDate = DateTime.Now.AddDays(-37),
                    TotalPrice = 599.97m,
                    Status = BookingStatus.CheckedOut,
                    BookingDate = DateTime.Now.AddDays(-45)
                },
                new Booking
                {
                    UserId = users.Single(u => u.Email == "ali@example.com").UserId,
                    RoomId = rooms.Single(r => r.RoomNumber == "401").RoomId,
                    CheckInDate = DateTime.Now.AddDays(-50),
                    CheckOutDate = DateTime.Now.AddDays(-47),
                    TotalPrice = 479.97m,
                    Status = BookingStatus.CheckedOut,
                    BookingDate = DateTime.Now.AddDays(-55)
                },
                new Booking
                {
                    UserId = users.Single(u => u.Email == "lisa@example.com").UserId,
                    RoomId = rooms.Single(r => r.RoomNumber == "501").RoomId,
                    CheckInDate = DateTime.Now.AddDays(-60),
                    CheckOutDate = DateTime.Now.AddDays(-57),
                    TotalPrice = 329.97m,
                    Status = BookingStatus.CheckedOut,
                    BookingDate = DateTime.Now.AddDays(-65)
                },
                new Booking
                {
                    UserId = users.Single(u => u.Email == "david@example.com").UserId,
                    RoomId = rooms.Single(r => r.RoomNumber == "601").RoomId,
                    CheckInDate = DateTime.Now.AddDays(-70),
                    CheckOutDate = DateTime.Now.AddDays(-67),
                    TotalPrice = 449.97m,
                    Status = BookingStatus.CheckedOut,
                    BookingDate = DateTime.Now.AddDays(-75)
                },
                new Booking
                {
                    UserId = users.Single(u => u.Email == "sarah@example.com").UserId,
                    RoomId = rooms.Single(r => r.RoomNumber == "701").RoomId,
                    CheckInDate = DateTime.Now.AddDays(-80),
                    CheckOutDate = DateTime.Now.AddDays(-77),
                    TotalPrice = 689.97m,
                    Status = BookingStatus.CheckedOut,
                    BookingDate = DateTime.Now.AddDays(-85)
                },
                new Booking
                {
                    UserId = users.Single(u => u.Email == "ali@example.com").UserId,
                    RoomId = rooms.Single(r => r.RoomNumber == "801").RoomId,
                    CheckInDate = DateTime.Now.AddDays(-90),
                    CheckOutDate = DateTime.Now.AddDays(-87),
                    TotalPrice = 299.97m,
                    Status = BookingStatus.CheckedOut,
                    BookingDate = DateTime.Now.AddDays(-95)
                }
            };
            context.Bookings.AddRange(moreBookings);
            context.SaveChanges();
            
            // Add payment information to more bookings (Payment merged into Booking)
            var paymentMethods = new[] { PaymentMethod.CreditCard, PaymentMethod.PayPal, PaymentMethod.BankTransfer };
            var random = new Random();
            for (int i = 0; i < moreBookings.Length; i++)
            {
                moreBookings[i].PaymentAmount = moreBookings[i].TotalPrice;
                moreBookings[i].PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)];
                moreBookings[i].PaymentStatus = PaymentStatus.Completed;
                moreBookings[i].TransactionId = $"TXN-{moreBookings[i].PaymentMethod.ToString().Substring(0, 2)}-{DateTime.Now.AddDays(-(i + 1) * 5).Ticks}";
                moreBookings[i].PaymentDate = DateTime.Now.AddDays(-(i + 1) * 5 - 5);
            }
            context.SaveChanges();

            var moreReviews = new Review[]
            {
                new Review
                {
                    BookingId = moreBookings[0].BookingId,
                    Rating = 5,
                    Comment = "Absolutely fantastic! The staff went above and beyond to make our stay memorable. The room was spotless and the breakfast was delicious. Will definitely return!",
                    ReviewDate = DateTime.Now.AddDays(-35)
                },
                new Review
                {
                    BookingId = moreBookings[1].BookingId,
                    Rating = 4,
                    Comment = "Great value for money. The location is perfect, close to everything. Room was clean and comfortable. Only minor issue was the Wi-Fi speed, but overall very satisfied.",
                    ReviewDate = DateTime.Now.AddDays(-45)
                },
                new Review
                {
                    BookingId = moreBookings[2].BookingId,
                    Rating = 5,
                    Comment = "Best budget hotel experience I've had! The family room was spacious, perfect for our needs. Kids loved it and we'll be back for sure. Highly recommend!",
                    ReviewDate = DateTime.Now.AddDays(-55)
                },
                new Review
                {
                    BookingId = moreBookings[3].BookingId,
                    Rating = 4,
                    Comment = "Clean, modern, and well-maintained. The twin room was perfect for our business trip. Good amenities and friendly staff. Would stay again.",
                    ReviewDate = DateTime.Now.AddDays(-65)
                },
                new Review
                {
                    BookingId = moreBookings[4].BookingId,
                    Rating = 5,
                    Comment = "Stunning ocean view! The suite exceeded all expectations. Beautiful balcony, comfortable bed, and excellent service. Worth every ringgit. Perfect for a romantic getaway!",
                    ReviewDate = DateTime.Now.AddDays(-75)
                },
                new Review
                {
                    BookingId = moreBookings[5].BookingId,
                    Rating = 4,
                    Comment = "Charming heritage room in the heart of Ipoh. Great location for exploring the old town and trying local food. Room was clean and had character. Enjoyed our stay!",
                    ReviewDate = DateTime.Now.AddDays(-85)
                }
            };
            context.Reviews.AddRange(moreReviews);
            context.SaveChanges();

            // Note: Favorites/Wishlist feature has been removed
            } // End of main seeding block
            
            // Ensure reviews exist - if reviews are missing but bookings exist, create reviews for checked-out bookings
            if (!context.Reviews.Any())
            {
                var checkedOutBookings = context.Bookings
                    .Where(b => b.Status == BookingStatus.CheckedOut && !context.Reviews.Any(r => r.BookingId == b.BookingId))
                    .Include(b => b.User)
                    .Take(10) // Limit to 10 reviews
                    .ToList();
                
                if (checkedOutBookings.Any())
                {
                    var sampleComments = new[]
                    {
                        "Excellent stay! The room was clean, comfortable, and the staff was very helpful. Great value for money!",
                        "Good experience overall. The room was spacious and well-maintained. Would stay again.",
                        "Perfect location and great service. Highly recommended!",
                        "Comfortable room with all necessary amenities. Good value.",
                        "Amazing experience! The suite was luxurious and the view was spectacular.",
                        "Great value for money. The location is perfect, close to everything. Room was clean and comfortable.",
                        "Best budget hotel experience I've had! The family room was spacious, perfect for our needs.",
                        "Clean, modern, and well-maintained. The twin room was perfect for our business trip.",
                        "Stunning view! The suite exceeded all expectations. Beautiful balcony and excellent service.",
                        "Charming room in a great location. Room was clean and had character. Enjoyed our stay!"
                    };
                    
                    var sampleRatings = new[] { 5, 4, 5, 4, 5, 4, 5, 4, 5, 4 };
                    var random = new Random();
                    
                    var missingReviews = checkedOutBookings.Select((booking, index) => new Review
                    {
                        BookingId = booking.BookingId,
                        Rating = sampleRatings[index % sampleRatings.Length],
                        Comment = sampleComments[index % sampleComments.Length],
                        ReviewDate = DateTime.Now.AddDays(-(index + 1) * 5)
                    }).ToList();
                    
                    context.Reviews.AddRange(missingReviews);
            context.SaveChanges();
                }
            }
            
            // Seed ContactMessages (if empty)
            if (!context.ContactMessages.Any())
            {
                var contactMessages = new ContactMessage[]
                {
                    new ContactMessage
                    {
                        Name = "John Smith",
                        Email = "john.smith@example.com",
                        Subject = "Inquiry about group bookings",
                        Message = "Hello, I'm planning a group booking for 20 people next month. Could you please provide more information about group rates?",
                        SentAt = DateTime.Now.AddDays(-10),
                        IsRead = true
                    },
                    new ContactMessage
                    {
                        Name = "Sarah Johnson",
                        Email = "sarah.j@example.com",
                        Subject = "Special dietary requirements",
                        Message = "Do your hotels accommodate guests with special dietary requirements? I have a severe nut allergy.",
                        SentAt = DateTime.Now.AddDays(-5),
                        IsRead = false
                    },
                    new ContactMessage
                    {
                        Name = "Michael Chen",
                        Email = "michael.chen@example.com",
                        Subject = "Early check-in availability",
                        Message = "I have an early morning flight arrival. Is early check-in available and what are the charges?",
                        SentAt = DateTime.Now.AddDays(-2),
                        IsRead = false
                    }
                };
                context.ContactMessages.AddRange(contactMessages);
                context.SaveChanges();
            }
            
            // Seed Newsletters (if empty)
            if (!context.Newsletters.Any())
            {
                var newsletters = new Newsletter[]
                {
                    new Newsletter
                    {
                        Email = "newsletter1@example.com",
                        SubscribedAt = DateTime.Now.AddDays(-30),
                        IsActive = true
                    },
                    new Newsletter
                    {
                        Email = "newsletter2@example.com",
                        SubscribedAt = DateTime.Now.AddDays(-20),
                        IsActive = true
                    },
                    new Newsletter
                    {
                        Email = "newsletter3@example.com",
                        SubscribedAt = DateTime.Now.AddDays(-10),
                        IsActive = true
                    }
                };
                context.Newsletters.AddRange(newsletters);
                context.SaveChanges();
            }
        }
    }
}