using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment.Migrations
{
    /// <summary>
    /// Migration: AddImageUrlToAmenity
    /// 
    /// This migration performs several database schema updates:
    /// 1. Adds ImageUrl column to Amenities table for displaying amenity icons
    /// 2. Merges Payment table into Bookings table (adds payment fields to Bookings)
    /// 3. Adds ImageUrl column to Hotels table
    /// 4. Creates PromotionUsages table for tracking promotion code usage
    /// 5. Adds abuse prevention fields to Promotions table (LimitPerDevice, LimitPerPaymentCard, etc.)
    /// 6. Removes old Payments table and updates Review foreign key constraints
    /// </summary>
    public partial class AddImageUrlToAmenity : Migration
    {
        /// <summary>
        /// Applies the migration - adds new columns and tables, removes old ones.
        /// Uses conditional SQL to handle cases where database was created manually.
        /// </summary>
        /// <param name="migrationBuilder">Migration builder for executing SQL commands.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key only if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Reviews_Bookings_BookingId')
                BEGIN
                    ALTER TABLE [Reviews] DROP CONSTRAINT [FK_Reviews_Bookings_BookingId];
                END
            ");

            // Drop Payments table only if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
                BEGIN
                    DROP TABLE [Payments];
                END
            ");

            // Drop index only if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Reviews') AND name = 'IX_Reviews_BookingId')
                BEGIN
                    DROP INDEX [IX_Reviews_BookingId] ON [Reviews];
                END
            ");

            // Drop columns only if they exist
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Hotels') AND name = 'Latitude')
                BEGIN
                    ALTER TABLE [Hotels] DROP COLUMN [Latitude];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Hotels') AND name = 'Longitude')
                BEGIN
                    ALTER TABLE [Hotels] DROP COLUMN [Longitude];
                END
            ");

            // Add columns only if they don't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Promotions') AND name = 'LimitPerDevice')
                BEGIN
                    ALTER TABLE [Promotions] ADD [LimitPerDevice] bit NOT NULL DEFAULT CAST(0 AS bit);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Promotions') AND name = 'LimitPerPaymentCard')
                BEGIN
                    ALTER TABLE [Promotions] ADD [LimitPerPaymentCard] bit NOT NULL DEFAULT CAST(0 AS bit);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Promotions') AND name = 'LimitPerPhoneNumber')
                BEGIN
                    ALTER TABLE [Promotions] ADD [LimitPerPhoneNumber] bit NOT NULL DEFAULT CAST(0 AS bit);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Promotions') AND name = 'LimitPerUserAccount')
                BEGIN
                    ALTER TABLE [Promotions] ADD [LimitPerUserAccount] bit NOT NULL DEFAULT CAST(0 AS bit);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Promotions') AND name = 'MaxTotalUses')
                BEGIN
                    ALTER TABLE [Promotions] ADD [MaxTotalUses] int NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Promotions') AND name = 'MaxUsesPerLimit')
                BEGIN
                    ALTER TABLE [Promotions] ADD [MaxUsesPerLimit] int NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Promotions') AND name = 'MinimumAmount')
                BEGIN
                    ALTER TABLE [Promotions] ADD [MinimumAmount] decimal(18,2) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Promotions') AND name = 'MinimumNights')
                BEGIN
                    ALTER TABLE [Promotions] ADD [MinimumNights] int NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Hotels') AND name = 'ImageUrl')
                BEGIN
                    ALTER TABLE [Hotels] ADD [ImageUrl] nvarchar(255) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'PaymentAmount')
                BEGIN
                    ALTER TABLE [Bookings] ADD [PaymentAmount] decimal(18,2) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'PaymentDate')
                BEGIN
                    ALTER TABLE [Bookings] ADD [PaymentDate] datetime2 NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'PaymentMethod')
                BEGIN
                    ALTER TABLE [Bookings] ADD [PaymentMethod] int NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'PaymentStatus')
                BEGIN
                    ALTER TABLE [Bookings] ADD [PaymentStatus] int NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'TransactionId')
                BEGIN
                    ALTER TABLE [Bookings] ADD [TransactionId] nvarchar(255) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Amenities') AND name = 'ImageUrl')
                BEGIN
                    ALTER TABLE [Amenities] ADD [ImageUrl] nvarchar(255) NULL;
                END
            ");

            // Create PromotionUsages table only if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PromotionUsages')
                BEGIN
                    CREATE TABLE [PromotionUsages] (
                        [PromotionUsageId] int NOT NULL IDENTITY,
                        [PromotionId] int NOT NULL,
                        [BookingId] int NOT NULL,
                        [PhoneNumberHash] nvarchar(255) NULL,
                        [CardIdentifier] nvarchar(100) NULL,
                        [DeviceFingerprint] nvarchar(100) NULL,
                        [IpAddress] nvarchar(50) NULL,
                        [UsedAt] datetime2 NOT NULL,
                        [UserId] int NOT NULL,
                        CONSTRAINT [PK_PromotionUsages] PRIMARY KEY ([PromotionUsageId]),
                        CONSTRAINT [FK_PromotionUsages_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([BookingId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_PromotionUsages_Promotions_PromotionId] FOREIGN KEY ([PromotionId]) REFERENCES [Promotions] ([PromotionId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_PromotionUsages_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
                    );
                END
            ");

            // Create indexes only if they don't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Reviews') AND name = 'IX_Reviews_BookingId')
                BEGIN
                    CREATE INDEX [IX_Reviews_BookingId] ON [Reviews] ([BookingId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('PromotionUsages') AND name = 'IX_PromotionUsages_BookingId')
                BEGIN
                    CREATE INDEX [IX_PromotionUsages_BookingId] ON [PromotionUsages] ([BookingId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('PromotionUsages') AND name = 'IX_PromotionUsages_PromotionId')
                BEGIN
                    CREATE INDEX [IX_PromotionUsages_PromotionId] ON [PromotionUsages] ([PromotionId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('PromotionUsages') AND name = 'IX_PromotionUsages_UserId')
                BEGIN
                    CREATE INDEX [IX_PromotionUsages_UserId] ON [PromotionUsages] ([UserId]);
                END
            ");

            // Add foreign key only if it doesn't exist
            // Note: SQL Server doesn't support ON DELETE RESTRICT syntax, default is NO ACTION (same as RESTRICT)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Reviews_Bookings_BookingId')
                BEGIN
                    ALTER TABLE [Reviews] ADD CONSTRAINT [FK_Reviews_Bookings_BookingId] 
                    FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([BookingId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Bookings_BookingId",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "PromotionUsages");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_BookingId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "LimitPerDevice",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "LimitPerPaymentCard",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "LimitPerPhoneNumber",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "LimitPerUserAccount",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MaxTotalUses",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MaxUsesPerLimit",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MinimumAmount",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MinimumNights",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "PaymentAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Amenities");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Hotels",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Hotels",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookingId",
                table: "Reviews",
                column: "BookingId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Bookings_BookingId",
                table: "Reviews",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
