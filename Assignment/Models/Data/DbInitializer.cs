using Assignment.Models;
using System;
using System.Linq;
using Assignment.Models;

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

            // Seeding Users
            var users = new User[]
            {
                new User{FullName="Admin Hotel", Email="admin@hotel.com", PasswordHash="...hashed_password...", Role=UserRole.Admin, IsEmailVerified=true},
                new User{FullName="Alice Johnson", Email="alice@example.com", PasswordHash="...hashed_password...", Role=UserRole.Customer, IsEmailVerified=true},
                new User{FullName="Bob Williams", Email="bob@example.com", PasswordHash="...hashed_password...", Role=UserRole.Customer, IsEmailVerified=true}
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // Seeding RoomTypes
            var roomTypes = new RoomType[]
            {
                new RoomType{Name="Standard Single Room", Description="A cozy room for a single traveler.", Occupancy=1, BasePrice=79.99m},
                new RoomType{Name="Deluxe Double Room", Description="Spacious room with two double beds.", Occupancy=4, BasePrice=129.99m},
                new RoomType{Name="Executive Suite", Description="Luxurious suite with a king bed and city view.", Occupancy=2, BasePrice=199.99m}
            };
            context.RoomTypes.AddRange(roomTypes);
            context.SaveChanges();

            // Seeding Rooms
            var rooms = new Room[]
            {
                new Room{RoomNumber="101", RoomTypeId=roomTypes[0].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="102", RoomTypeId=roomTypes[0].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="201", RoomTypeId=roomTypes[1].RoomTypeId, Status=RoomStatus.UnderMaintenance},
                new Room{RoomNumber="202", RoomTypeId=roomTypes[1].RoomTypeId, Status=RoomStatus.Available},
                new Room{RoomNumber="301", RoomTypeId=roomTypes[2].RoomTypeId, Status=RoomStatus.Available}
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
                new Amenity{Name="Coffee Maker"}
            };
            context.Amenities.AddRange(amenities);
            context.SaveChanges();

            var roomTypeAmenities = new RoomTypeAmenity[]
            {
                new RoomTypeAmenity{RoomTypeId=roomTypes[0].RoomTypeId, AmenityId=amenities[0].AmenityId}, // Wi-Fi for Single
                new RoomTypeAmenity{RoomTypeId=roomTypes[0].RoomTypeId, AmenityId=amenities[1].AmenityId}, // AC for Single
                new RoomTypeAmenity{RoomTypeId=roomTypes[1].RoomTypeId, AmenityId=amenities[0].AmenityId}, // Wi-Fi for Double
                new RoomTypeAmenity{RoomTypeId=roomTypes[1].RoomTypeId, AmenityId=amenities[1].AmenityId}, // AC for Double
                new RoomTypeAmenity{RoomTypeId=roomTypes[1].RoomTypeId, AmenityId=amenities[2].AmenityId}, // TV for Double
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[0].AmenityId}, // All for Suite
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[1].AmenityId},
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[2].AmenityId},
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[3].AmenityId},
                new RoomTypeAmenity{RoomTypeId=roomTypes[2].RoomTypeId, AmenityId=amenities[4].AmenityId},
            };
            context.RoomTypeAmenities.AddRange(roomTypeAmenities);
            context.SaveChanges();


            // Seeding a Booking
            var booking = new Booking
            {
                UserId = users.Single(u => u.Email == "alice@example.com").UserId,
                RoomId = rooms.Single(r => r.RoomNumber == "101").RoomId,
                CheckInDate = DateTime.Now.AddDays(10),
                CheckOutDate = DateTime.Now.AddDays(13),
                TotalPrice = 239.97m,
                Status = BookingStatus.Confirmed
            };
            context.Bookings.Add(booking);
            context.SaveChanges();
        }
    }
}