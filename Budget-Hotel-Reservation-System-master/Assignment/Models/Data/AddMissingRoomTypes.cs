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
            // ========== PURPOSE ==========
            // This method ensures that hotels in specific cities (Ipoh, Kota Kinabalu, Kuching, Cameron Highlands)
            // have at least one room type available for booking. This is important because:
            // 1. Hotels without room types cannot accept bookings
            // 2. Some hotels might be created without room types initially
            // 3. This method automatically creates room types, rooms, and images if missing
            
            // ========== STEP 1: FIND HOTELS IN SPECIFIC CITIES ==========
            // Check if specific hotels need room types (Ipoh, Kota Kinabalu, Kuching, Cameron Highlands)
            // These cities might have hotels that were created without room types
            // We search by city name to find hotels that need room types
            var ipohHotel = await context.Hotels.FirstOrDefaultAsync(h => h.City == "Ipoh");
            var kkHotel = await context.Hotels.FirstOrDefaultAsync(h => h.City == "Kota Kinabalu");
            var kuchingHotel = await context.Hotels.FirstOrDefaultAsync(h => h.City == "Kuching");
            var cameronHotel = await context.Hotels.FirstOrDefaultAsync(h => h.City == "Cameron Highlands");
            
            // ========== STEP 2: INITIALIZE COLLECTIONS ==========
            // Create lists to store new entities that will be added to database
            // We batch insert all new entities at once for better performance
            var newRoomTypes = new List<RoomType>();    // New room types to create
            var newRooms = new List<Room>();            // New physical rooms to create
            var newRoomImages = new List<RoomImage>();  // New room images to create
            var roomNumberCounter = 1200;                // Starting room number (to avoid conflicts)
            
            // ========== STEP 3: CHECK AND CREATE ROOM TYPE FOR IPOH ==========
            // Add room type for Ipoh if it doesn't have one
            // Check: 1) Hotel exists, 2) Hotel has no room types yet
            if (ipohHotel != null && !await context.RoomTypes.AnyAsync(rt => rt.HotelId == ipohHotel.HotelId))
            {
                // Create a new room type with appropriate details for Ipoh
                var roomType = new RoomType
                {
                    Name = "Heritage Double Room",                                                    // Room type name
                    Description = "Charming room with colonial-style furnishings in the heart of historic Ipoh.",  // Description
                    Occupancy = 2,                                                                    // Maximum 2 guests
                    BasePrice = 99.99m,                                                               // Price per night in RM
                    HotelId = ipohHotel.HotelId                                                       // Link to Ipoh hotel
                };
                newRoomTypes.Add(roomType);  // Add to list for batch insertion
            }
            
            // ========== STEP 4: CHECK AND CREATE ROOM TYPE FOR KOTA KINABALU ==========
            // Add room type for Kota Kinabalu if it doesn't have one
            // Check: 1) Hotel exists, 2) Hotel has no room types yet
            if (kkHotel != null && !await context.RoomTypes.AnyAsync(rt => rt.HotelId == kkHotel.HotelId))
            {
                // Create a new room type with appropriate details for Kota Kinabalu
                var roomType = new RoomType
                {
                    Name = "Sabah View Room",                                                         // Room type name
                    Description = "Comfortable room with mountain or sea view, perfect for exploring Kota Kinabalu.",  // Description
                    Occupancy = 2,                                                                    // Maximum 2 guests
                    BasePrice = 119.99m,                                                              // Price per night in RM
                    HotelId = kkHotel.HotelId                                                          // Link to Kota Kinabalu hotel
                };
                newRoomTypes.Add(roomType);  // Add to list for batch insertion
            }
            
            // ========== STEP 5: CHECK AND CREATE ROOM TYPE FOR KUCHING ==========
            // Add room type for Kuching if it doesn't have one
            // Check: 1) Hotel exists, 2) Hotel has no room types yet
            if (kuchingHotel != null && !await context.RoomTypes.AnyAsync(rt => rt.HotelId == kuchingHotel.HotelId))
            {
                // Create a new room type with appropriate details for Kuching
                var roomType = new RoomType
                {
                    Name = "Riverside Deluxe",                                                         // Room type name
                    Description = "Spacious room overlooking the Sarawak River with modern amenities.",  // Description
                    Occupancy = 3,                                                                    // Maximum 3 guests
                    BasePrice = 139.99m,                                                              // Price per night in RM
                    HotelId = kuchingHotel.HotelId                                                    // Link to Kuching hotel
                };
                newRoomTypes.Add(roomType);  // Add to list for batch insertion
            }
            
            // ========== STEP 6: CHECK AND CREATE ROOM TYPE FOR CAMERON HIGHLANDS ==========
            // Add room type for Cameron Highlands if it doesn't have one
            // Check: 1) Hotel exists, 2) Hotel has no room types yet
            if (cameronHotel != null && !await context.RoomTypes.AnyAsync(rt => rt.HotelId == cameronHotel.HotelId))
            {
                // Create a new room type with appropriate details for Cameron Highlands
                var roomType = new RoomType
                {
                    Name = "Highland Cozy Room",                                                       // Room type name
                    Description = "Warm and comfortable room perfect for the cool highland climate.",  // Description
                    Occupancy = 2,                                                                    // Maximum 2 guests
                    BasePrice = 89.99m,                                                               // Price per night in RM
                    HotelId = cameronHotel.HotelId                                                    // Link to Cameron Highlands hotel
                };
                newRoomTypes.Add(roomType);  // Add to list for batch insertion
            }
            
            // ========== STEP 7: SAVE ROOM TYPES TO DATABASE ==========
            // If any new room types were created, save them to database
            // This must be done before creating rooms (rooms need RoomTypeId)
            if (newRoomTypes.Any())
            {
                // Add all new room types to database context
                context.RoomTypes.AddRange(newRoomTypes);
                // Save changes to get RoomTypeId values (auto-generated by database)
                await context.SaveChangesAsync();
                
                // ========== STEP 8: CREATE PHYSICAL ROOMS FOR EACH ROOM TYPE ==========
                // For each new room type, create 3 physical rooms
                // Physical rooms are the actual bookable units (e.g., Room 101, Room 102, Room 103)
                foreach (var newRoomType in newRoomTypes)
                {
                    // Create 3 rooms for this room type
                    for (int i = 0; i < 3; i++)
                    {
                        // ========== GENERATE UNIQUE ROOM NUMBER ==========
                        // Generate a unique room number starting from 1200
                        // Check if room number already exists (to avoid conflicts)
                        var roomNumber = roomNumberCounter++.ToString();
                        while (await context.Rooms.AnyAsync(r => r.RoomNumber == roomNumber))
                        {
                            // If room number exists, try next number
                            roomNumber = roomNumberCounter++.ToString();
                        }
                        
                        // ========== CREATE ROOM ENTITY ==========
                        // Create a new physical room linked to the room type
                        newRooms.Add(new Room
                        {
                            RoomNumber = roomNumber,                    // Unique room identifier (e.g., "1201")
                            RoomTypeId = newRoomType.RoomTypeId,        // Link to room type (defines room characteristics)
                            Status = RoomStatus.Available                // Set status to Available (ready for booking)
                        });
                    }
                    
                    // ========== STEP 9: CREATE ROOM IMAGE ==========
                    // Add a default image for each new room type
                    // Images are displayed to customers when browsing rooms
                    // Different images based on room type name (Heritage, Sabah, Riverside, Highland)
                    string imageUrl;
                    if (newRoomType.Name.Contains("Heritage"))
                        // Heritage room image (colonial style)
                        imageUrl = "https://images.unsplash.com/photo-1505692794400-5e0fd4802a03?auto=format&fit=crop&w=1280&q=80";
                    else if (newRoomType.Name.Contains("Sabah"))
                        // Sabah view room image (mountain/sea view)
                        imageUrl = "https://images.unsplash.com/photo-1445019980597-93fa8acb246c?auto=format&fit=crop&w=1280&q=80";
                    else if (newRoomType.Name.Contains("Riverside"))
                        // Riverside room image (river view)
                        imageUrl = "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1280&q=80";
                    else
                        // Default highland room image (cozy mountain view)
                        imageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1280&q=80";
                    
                    // ========== CREATE ROOM IMAGE ENTITY ==========
                    // Create room image linked to the room type
                    newRoomImages.Add(new RoomImage
                    {
                        RoomTypeId = newRoomType.RoomTypeId,  // Link to room type
                        ImageUrl = imageUrl,                   // URL to room image
                        Caption = newRoomType.Name             // Image caption (room type name)
                    });
                }
                
                // ========== STEP 10: SAVE ROOMS TO DATABASE ==========
                // Save all new rooms to database in batch
                if (newRooms.Any())
                {
                    context.Rooms.AddRange(newRooms);
                    await context.SaveChangesAsync();
                }
                
                // ========== STEP 11: SAVE ROOM IMAGES TO DATABASE ==========
                // Save all new room images to database in batch
                if (newRoomImages.Any())
                {
                    context.RoomImages.AddRange(newRoomImages);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}

