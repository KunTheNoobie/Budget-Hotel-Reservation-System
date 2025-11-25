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
        public DbSet<SecurityToken> SecurityTokens { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<SecurityLog> SecurityLogs { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<RoomTypeAmenity> RoomTypeAmenities { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageItem> PackageItems { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        // Payment merged into Booking - no longer needed as separate table
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionUsage> PromotionUsages { get; set; }
        public DbSet<Newsletter> Newsletters { get; set; }
        public DbSet<FavoriteRoomType> FavoriteRoomTypes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Review relationships - prevent circular dependencies
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
            
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Booking)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid circular dependency

            // Global query filters for soft delete - automatically filter out deleted records
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Hotel>().HasQueryFilter(h => !h.IsDeleted);
            modelBuilder.Entity<Room>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<RoomType>().HasQueryFilter(rt => !rt.IsDeleted);
            modelBuilder.Entity<RoomImage>().HasQueryFilter(ri => !ri.IsDeleted);
            modelBuilder.Entity<Amenity>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<Package>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<PackageItem>().HasQueryFilter(pi => !pi.IsDeleted);
            modelBuilder.Entity<Booking>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<ContactMessage>().HasQueryFilter(cm => !cm.IsDeleted);
            modelBuilder.Entity<Promotion>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<PromotionUsage>().HasQueryFilter(pu => !pu.IsDeleted);
            modelBuilder.Entity<Service>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Newsletter>().HasQueryFilter(n => !n.IsDeleted);
            modelBuilder.Entity<SecurityLog>().HasQueryFilter(sl => !sl.IsDeleted);
            modelBuilder.Entity<FavoriteRoomType>().HasQueryFilter(f => !f.IsDeleted);
        }
    }
}