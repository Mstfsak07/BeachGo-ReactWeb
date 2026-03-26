using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BeachRehberi.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Beaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    Website = table.Column<string>(type: "TEXT", nullable: false),
                    Instagram = table.Column<string>(type: "TEXT", nullable: false),
                    OpenTime = table.Column<string>(type: "TEXT", nullable: false),
                    CloseTime = table.Column<string>(type: "TEXT", nullable: false),
                    HasEntryFee = table.Column<bool>(type: "INTEGER", nullable: false),
                    EntryFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SunbedPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    Rating = table.Column<double>(type: "double precision", nullable: false),
                    ReviewCount = table.Column<int>(type: "INTEGER", nullable: false),
                    GooglePlaceId = table.Column<string>(type: "TEXT", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "TEXT", nullable: false),
                    HasSunbeds = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasShower = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasParking = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasRestaurant = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasBar = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasAlcohol = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsChildFriendly = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasWaterSports = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasWifi = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasPool = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasDJ = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasAccessibility = table.Column<bool>(type: "INTEGER", nullable: false),
                    OccupancyPercent = table.Column<int>(type: "INTEGER", nullable: false),
                    OccupancyLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsOpen = table.Column<bool>(type: "INTEGER", nullable: false),
                    TodaySpecial = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beaches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BeachId = table.Column<int>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    ContactName = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessUsers_Beaches_BeachId",
                        column: x => x.BeachId,
                        principalTable: "Beaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BeachId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TicketPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    AvailableSpots = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAgeRestricted = table.Column<bool>(type: "INTEGER", nullable: false),
                    MinAge = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Beaches_BeachId",
                        column: x => x.BeachId,
                        principalTable: "Beaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BeachId = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Caption = table.Column<string>(type: "TEXT", nullable: false),
                    IsCover = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_Beaches_BeachId",
                        column: x => x.BeachId,
                        principalTable: "Beaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConfirmationCode = table.Column<string>(type: "TEXT", nullable: false),
                    BeachId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    UserPhone = table.Column<string>(type: "TEXT", nullable: false),
                    UserEmail = table.Column<string>(type: "TEXT", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PersonCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SunbedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Beaches_BeachId",
                        column: x => x.BeachId,
                        principalTable: "Beaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BeachId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    UserPhone = table.Column<string>(type: "TEXT", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Beaches_BeachId",
                        column: x => x.BeachId,
                        principalTable: "Beaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Beaches",
                columns: new[] { "Id", "Address", "CloseTime", "CoverImageUrl", "Description", "EntryFee", "GooglePlaceId", "HasAccessibility", "HasAlcohol", "HasBar", "HasDJ", "HasEntryFee", "HasParking", "HasPool", "HasRestaurant", "HasShower", "HasSunbeds", "HasWaterSports", "HasWifi", "Instagram", "IsChildFriendly", "IsOpen", "LastUpdated", "Latitude", "Longitude", "Name", "OccupancyLevel", "OccupancyPercent", "OpenTime", "Phone", "Rating", "ReviewCount", "SunbedPrice", "TodaySpecial", "Website" },
                values: new object[,]
                {
                    { 1, "Meltem, Beach Park No:10, Muratpaşa/Antalya", "01:00", "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800", "Konyaaltı'nın en popüler beach club'larından.", 0m, "ChIJeWMyN6yRwxQRGQAjkCC3kCs", false, true, true, true, false, true, false, true, true, true, true, true, "@kalypsobeach", false, true, new DateTime(2026, 3, 20, 8, 6, 42, 880, DateTimeKind.Utc).AddTicks(6770), 36.878581099999998, 30.665650200000002, "Kalypso Beach Club", 2, 60, "08:30", "+90 530 783 71 20", 4.5999999999999996, 2741, 400m, "", "" },
                    { 2, "Kuşkavağı, Akdeniz Blv. No:17, Konyaaltı/Antalya", "23:59", "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?w=800", "Gece gündüz açık, canlı müzikli plaj.", 0m, "ChIJo0ibX0GRwxQRmp3ywLZoEtA", false, true, true, true, false, true, false, true, true, true, false, true, "@sunshinebeachantalya", true, true, new DateTime(2026, 3, 20, 8, 6, 42, 881, DateTimeKind.Utc).AddTicks(3775), 36.868841400000001, 30.649725499999999, "Sunshine Beach", 1, 45, "00:00", "+90 530 345 92 85", 4.5999999999999996, 2814, 300m, "", "" },
                    { 3, "Meltem, Akdeniz Blv. No:5, Muratpaşa/Antalya", "01:00", "https://images.unsplash.com/photo-1519046904884-53103b34b206?w=800", "Şık tasarım, premium hizmet.", 0m, "ChIJjxyCDpWRwxQRkaHDzpSqOsM", false, true, true, true, false, true, false, true, true, true, true, true, "@roxybeachantalya", false, true, new DateTime(2026, 3, 20, 8, 6, 42, 881, DateTimeKind.Utc).AddTicks(3795), 36.881675399999999, 30.6724529, "Roxy Beach Lounge", 3, 80, "08:00", "+90 532 489 65 05", 4.5, 2179, 400m, "", "" },
                    { 4, "Test", "20:00", "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800", "API testi.", 0m, "", false, false, false, false, false, false, false, false, false, false, false, false, "", false, true, new DateTime(2026, 3, 20, 8, 6, 42, 879, DateTimeKind.Utc).AddTicks(9241), 36.869999999999997, 30.66, "TEST BEACH API", 1, 10, "08:00", "", 5.0, 1, 0m, "", "" }
                });

            migrationBuilder.InsertData(
                table: "BusinessUsers",
                columns: new[] { "Id", "BeachId", "ContactName", "CreatedAt", "Email", "IsActive", "LastLoginAt", "PasswordHash" },
                values: new object[] { 1, 1, "Kalypso Yönetici", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "kalypso@beach.com", true, null, "UOm0YNf5xsCBeKQJM3Yxt8d8zNcBESfLxj3H1gKKQbE=" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessUsers_BeachId",
                table: "BusinessUsers",
                column: "BeachId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_BeachId",
                table: "Events",
                column: "BeachId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_BeachId",
                table: "Photos",
                column: "BeachId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BeachId",
                table: "Reservations",
                column: "BeachId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BeachId",
                table: "Reviews",
                column: "BeachId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessUsers");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Beaches");
        }
    }
}
