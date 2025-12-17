using Assignment.Models;
using Assignment.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Models.Data
{
    /// <summary>
    /// Static class for adding missing room types to hotels in specific cities.
    /// Ensures that hotels in Ipoh, Kota Kinabalu, Kuching, and Cameron Highlands
    /// have at least one room type available for booking.
    /// Also creates corresponding rooms and room images for the new room types.
    /// </summary>
    public static class AddMissingRoomTypes
    {
        /// <summary>
        /// Adds missing room types, rooms, and images for hotels in specific cities.
        /// Checks if hotels in Ipoh, Kota Kinabalu, Kuching, and Cameron Highlands have room types,
        /// and creates them if they don't exist.
        /// </summary>
        /// <param name="context">The database context to update.</param>
        public static async Task AddMissingData(HotelDbContext context)
        {
            // Check if specific hotels need room types (Ipoh, Kota Kinabalu, Kuching, Cameron Highlands)
            var ipohHotel = await context.Hotels.FirstOrDefaultAsync(h => h.City == "Ipoh");
            var kkHotel = await context.Hotels.FirstOrDefaultAsync(h => h.City == "Kota Kinabalu");
            var kuchingHotel = await context.Hotels.FirstOrDefaultAsync(h => h.City == "Kuching");
            var cameronHotel = await context.Hotels.FirstOrDefaultAsync(h => h.City == "Cameron Highlands");
            
            var newRoomTypes = new List<RoomType>();
            var newRooms = new List<Room>();
            var newRoomImages = new List<RoomImage>();
            var roomNumberCounter = 1200;
            
            // Add room type for Ipoh if it doesn't have one
            if (ipohHotel != null && !await context.RoomTypes.AnyAsync(rt => rt.HotelId == ipohHotel.HotelId))
            {
                var roomType = new RoomType
                {
                    Name = "Heritage Double Room",
                    Description = "Charming room with colonial-style furnishings in the heart of historic Ipoh.",
                    Occupancy = 2,
                    BasePrice = 99.99m,
                    HotelId = ipohHotel.HotelId
                };
                newRoomTypes.Add(roomType);
            }
            
            // Add room type for Kota Kinabalu if it doesn't have one
            if (kkHotel != null && !await context.RoomTypes.AnyAsync(rt => rt.HotelId == kkHotel.HotelId))
            {
                var roomType = new RoomType
                {
                    Name = "Sabah View Room",
                    Description = "Comfortable room with mountain or sea view, perfect for exploring Kota Kinabalu.",
                    Occupancy = 2,
                    BasePrice = 119.99m,
                    HotelId = kkHotel.HotelId
                };
                newRoomTypes.Add(roomType);
            }
            
            // Add room type for Kuching if it doesn't have one
            if (kuchingHotel != null && !await context.RoomTypes.AnyAsync(rt => rt.HotelId == kuchingHotel.HotelId))
            {
                var roomType = new RoomType
                {
                    Name = "Riverside Deluxe",
                    Description = "Spacious room overlooking the Sarawak River with modern amenities.",
                    Occupancy = 3,
                    BasePrice = 139.99m,
                    HotelId = kuchingHotel.HotelId
                };
                newRoomTypes.Add(roomType);
            }
            
            // Add room type for Cameron Highlands if it doesn't have one
            if (cameronHotel != null && !await context.RoomTypes.AnyAsync(rt => rt.HotelId == cameronHotel.HotelId))
            {
                var roomType = new RoomType
                {
                    Name = "Highland Cozy Room",
                    Description = "Warm and comfortable room perfect for the cool highland climate.",
                    Occupancy = 2,
                    BasePrice = 89.99m,
                    HotelId = cameronHotel.HotelId
                };
                newRoomTypes.Add(roomType);
            }
            
            if (newRoomTypes.Any())
            {
                context.RoomTypes.AddRange(newRoomTypes);
                await context.SaveChangesAsync();
                
                // Add rooms for the new room types
                foreach (var newRoomType in newRoomTypes)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var roomNumber = roomNumberCounter++.ToString();
                        while (await context.Rooms.AnyAsync(r => r.RoomNumber == roomNumber))
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
                    
                    // Add room image
                    string imageUrl;
                    if (newRoomType.Name.Contains("Heritage"))
                        imageUrl = "https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?auto=format&fit=crop&w=1280&q=80";
                    else if (newRoomType.Name.Contains("Sabah"))
                        imageUrl = "https://images.unsplash.com/photo-1445019980597-93fa8acb246c?auto=format&fit=crop&w=1280&q=80";
                    else if (newRoomType.Name.Contains("Riverside"))
                        imageUrl = "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1280&q=80";
                    else
                        imageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1280&q=80";
                    
                    newRoomImages.Add(new RoomImage
                    {
                        RoomTypeId = newRoomType.RoomTypeId,
                        ImageUrl = imageUrl,
                        Caption = newRoomType.Name
                    });
                }
                
                if (newRooms.Any())
                {
                    context.Rooms.AddRange(newRooms);
                    await context.SaveChangesAsync();
                }
                
                if (newRoomImages.Any())
                {
                    context.RoomImages.AddRange(newRoomImages);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}

