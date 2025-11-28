using Assignment.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Assignment.Models.Data
{
    /// <summary>
    /// Entity Framework Core database context for the Budget Hotel Reservation System.
    /// Manages database connections, entity configurations, relationships, and query filters.
    /// Implements soft delete pattern with global query filters that automatically exclude deleted records.
    /// </summary>
    public class HotelDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the HotelDbContext.
        /// </summary>
        /// <param name="options">Database context options (connection string, provider, etc.).</param>
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
        // Note: Payment information is merged into Booking entity - no separate Payment table
        public DbSet<Promotion> Promotions { get; set; }
        // Note: PromotionUsage table removed - usage tracking now stored in Booking table
        public DbSet<Newsletter> Newsletters { get; set; }
        // Note: FavoriteRoomType feature removed


        /// <summary>
        /// Configures entity relationships, constraints, and global query filters.
        /// Called by Entity Framework when building the model.
        /// </summary>
        /// <param name="modelBuilder">Model builder for configuring entities.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Review relationships - prevent circular dependencies
            // Review is only linked to Booking, user info obtained from Booking.UserId
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Booking)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid circular dependency

            // Global query filters for soft delete - automatically filter out deleted records
            // These filters ensure that IsDeleted=true records are excluded from all queries
            // unless explicitly overridden using IgnoreQueryFilters()
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
            // Note: PromotionUsage table removed - usage tracking now in Booking table
            modelBuilder.Entity<Service>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Newsletter>().HasQueryFilter(n => !n.IsDeleted);
            modelBuilder.Entity<SecurityLog>().HasQueryFilter(sl => !sl.IsDeleted);
            // Note: FavoriteRoomType feature removed
        }
    }
}