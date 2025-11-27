using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment.Migrations
{
    /// <summary>
    /// Migration: AddFavoriteRoomTypes
    /// 
    /// Creates the FavoriteRoomTypes table to implement the favorites/wishlist feature.
    /// This table establishes a many-to-many relationship between Users and RoomTypes,
    /// allowing users to save favorite room types for later viewing.
    /// 
    /// Table includes:
    /// - FavoriteId (Primary Key)
    /// - UserId (Foreign Key to Users)
    /// - RoomTypeId (Foreign Key to RoomTypes)
    /// - AddedAt (Timestamp when favorited)
    /// - IsDeleted and DeletedAt (Soft delete support)
    /// </summary>
    public partial class AddFavoriteRoomTypes : Migration
    {
        /// <summary>
        /// Applies the migration - creates the FavoriteRoomTypes table with indexes.
        /// </summary>
        /// <param name="migrationBuilder">Migration builder for executing SQL commands.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FavoriteRoomTypes",
                columns: table => new
                {
                    FavoriteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoomTypeId = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteRoomTypes", x => x.FavoriteId);
                    table.ForeignKey(
                        name: "FK_FavoriteRoomTypes_RoomTypes_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "RoomTypes",
                        principalColumn: "RoomTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteRoomTypes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteRoomTypes_RoomTypeId",
                table: "FavoriteRoomTypes",
                column: "RoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteRoomTypes_UserId",
                table: "FavoriteRoomTypes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteRoomTypes");
        }
    }
}
