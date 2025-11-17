using Assignment.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Assignment.Models.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<SecurityToken> SecurityTokens { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<RoomTypeAmenity> RoomTypeAmenities { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<SeasonalPricing> SeasonalPricings { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageItem> PackageItems { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<BookingCancellation> BookingCancellations { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // FIX #1: Break cascade path for Support Tickets
            modelBuilder.Entity<SupportTicket>()
                .HasOne(st => st.User)
                .WithMany(u => u.SupportTickets)
                .HasForeignKey(st => st.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Sender)
                .WithMany()
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // **********************************
            // ***** ADD THIS NEW FIX HERE ******
            // **********************************
            // FIX #2: Break cascade path for Reviews
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User) // A Review is written by one User
                .WithMany(u => u.Reviews) // A User can write many Reviews
                .HasForeignKey(r => r.UserId) // The foreign key is UserId
                .OnDelete(DeleteBehavior.Restrict); //  <-- PREVENT CASCADE DELETE
        }
    }
}