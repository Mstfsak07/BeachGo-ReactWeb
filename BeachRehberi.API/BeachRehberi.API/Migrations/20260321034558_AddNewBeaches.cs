using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BeachRehberi.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNewBeaches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 21, 3, 45, 57, 964, DateTimeKind.Utc).AddTicks(6984));

            migrationBuilder.UpdateData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1420));

            migrationBuilder.UpdateData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1589));

            migrationBuilder.UpdateData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 21, 3, 45, 57, 964, DateTimeKind.Utc).AddTicks(1222));

            migrationBuilder.InsertData(
                table: "Beaches",
                columns: new[] { "Id", "Address", "CloseTime", "CoverImageUrl", "Description", "EntryFee", "GooglePlaceId", "HasAccessibility", "HasAlcohol", "HasBar", "HasDJ", "HasEntryFee", "HasParking", "HasPool", "HasRestaurant", "HasShower", "HasSunbeds", "HasWaterSports", "HasWifi", "Instagram", "IsChildFriendly", "IsOpen", "LastUpdated", "Latitude", "Longitude", "Name", "OccupancyLevel", "OccupancyPercent", "OpenTime", "Phone", "Rating", "ReviewCount", "SunbedPrice", "TodaySpecial", "Website" },
                values: new object[,]
                {
                    { 5, "Sahil Yaşam Parkı No:7, Konyaaltı/Antalya", "02:00", "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800", "Canlı müzik ve kokteyllerle sahil keyfi.", 0m, "", false, true, true, true, false, true, false, true, true, true, false, true, "", false, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1614), 36.880509600000003, 30.6699175, "Flamingo Lounge", 2, 50, "09:00", "+90 555 053 13 36", 4.2000000000000002, 850, 200m, "", "" },
                    { 6, "Konyaaltı Beachpark, Konyaaltı/Antalya", "20:00", "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?w=800", "1999'dan beri Konyaaltı'nın köklü plajlarından, aile dostu.", 0m, "", false, false, true, false, false, true, false, true, true, true, true, false, "", true, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1621), 36.875, 30.658000000000001, "Aydın Beach Club", 1, 40, "08:00", "", 4.5, 1200, 150m, "", "" },
                    { 7, "Arapsuyu, Akdeniz Bulv. No:64, Konyaaltı/Antalya", "19:00", "https://images.unsplash.com/photo-1519046904884-53103b34b206?w=800", "Burger, pizza ve frozen içeceklerle sahil keyfi.", 0m, "", false, false, true, false, false, true, false, true, false, true, false, false, "", true, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1624), 36.868099999999998, 30.649799999999999, "Lucky 13 Beach Restaurant", 1, 30, "08:00", "0542 408 07 87", 4.0, 430, 100m, "", "" },
                    { 8, "Akdeniz Bulvarı No:15, Konyaaltı/Antalya", "01:00", "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800", "Canlı müzik ve deniz ürünleriyle gece hayatı.", 0m, "", false, true, true, true, false, true, false, true, true, true, false, true, "", false, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1628), 36.880000000000003, 30.664999999999999, "Ferma Beach", 2, 55, "12:00", "", 4.0999999999999996, 620, 250m, "", "" },
                    { 9, "Kuşkavağı Mah., Akdeniz Bulv. No:25/1, Konyaaltı/Antalya", "02:00", "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?w=800", "Sunset manzarasıyla bistro yemekleri ve şezlong keyfi.", 0m, "", false, true, true, true, false, true, false, true, true, true, false, true, "", true, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1646), 36.866022299999997, 30.645267799999999, "Twenty Beach & Bistro", 1, 45, "11:00", "", 4.5, 980, 200m, "", "" },
                    { 10, "Kuşkavağı Mah., Akdeniz Bulv. No:27/21, Konyaaltı/Antalya", "01:00", "https://images.unsplash.com/photo-1519046904884-53103b34b206?w=800", "Kahvaltıdan geceye kadar sahil keyfi, aile dostu.", 0m, "", false, true, true, true, false, true, false, true, true, true, false, true, "", true, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1649), 36.866999999999997, 30.646000000000001, "Alabama Beach & Restaurant", 2, 50, "09:00", "", 4.5, 750, 200m, "", "" },
                    { 11, "Akdeniz Blv. No:22, Kuşkavağı, Konyaaltı/Antalya", "02:00", "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800", "Bohemian atmosfer, steakhouse ve DJ performance.", 0m, "", false, true, true, true, false, true, false, true, true, true, false, true, "", false, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1654), 36.866999999999997, 30.648, "La Bohem Beach Restaurant", 3, 70, "12:00", "+90 540 156 07 56", 4.5999999999999996, 1100, 500m, "", "" },
                    { 12, "Akdeniz Blv. No:33, Konyaaltı/Antalya", "02:00", "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?w=800", "Premium şezlong ve daybed, steakhouse ve gece müziği.", 0m, "", false, true, true, true, false, true, false, true, true, true, false, true, "", false, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1657), 36.862472799999999, 30.639734300000001, "Dubai Beach Konyaaltı", 2, 65, "09:00", "+90 537 652 25 94", 4.5999999999999996, 1850, 400m, "", "" },
                    { 13, "Akdeniz Bulvarı, Konyaaltı/Antalya", "23:00", "https://images.unsplash.com/photo-1519046904884-53103b34b206?w=800", "Kahve ve sahil keyfinin buluştuğu sakin atmosfer.", 0m, "", false, false, true, false, false, false, false, true, false, true, false, true, "", true, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1661), 36.878999999999998, 30.661999999999999, "Vento Mare Coffee & Beach", 1, 30, "09:00", "", 4.2999999999999998, 320, 150m, "", "" },
                    { 14, "Gürsu Mah. Akdeniz Blv. No:45/1, Konyaaltı/Antalya", "02:00", "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800", "Akdeniz mutfağı, kokteyl ve her gece DJ performance.", 0m, "", false, true, true, true, false, true, false, true, true, true, false, true, "", false, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1664), 36.8795, 30.663499999999999, "Cafe Belle Bistro & Beach", 2, 55, "09:00", "", 4.5, 680, 300m, "", "" },
                    { 15, "Gürsu Mah. Akdeniz Blv., Konyaaltı/Antalya", "01:00", "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?w=800", "Pizza, burger ve daybed ile bohem sahil atmosferi.", 0m, "", false, true, true, true, false, false, false, true, true, true, false, true, "", false, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1680), 36.880000000000003, 30.664000000000001, "Kuki Box Beach Lounge", 2, 60, "10:00", "+90 242 248 47 07", 4.4000000000000004, 540, 400m, "", "" },
                    { 16, "Gürsu Mah. Akdeniz Bulvarı, Konyaaltı/Antalya", "23:00", "https://images.unsplash.com/photo-1519046904884-53103b34b206?w=800", "Aile dostu, oyun parkı ve kahvaltıyla sahil keyfi.", 0m, "", false, true, true, true, false, true, false, true, true, true, false, false, "", true, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1684), 36.879199999999997, 30.662800000000001, "Frida Beach 33", 1, 35, "08:00", "+90 530 668 21 07", 4.0, 290, 150m, "", "" },
                    { 17, "Liman Mah. Akdeniz Blv. No:207, Konyaaltı/Antalya", "23:00", "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800", "Pet friendly, bistro yemekleri ve aile dostu sahil.", 0m, "", false, false, true, false, false, false, false, true, true, true, false, true, "", true, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1687), 36.881999999999998, 30.672999999999998, "Shakespeare Beach", 1, 40, "08:00", "+90 507 072 06 00", 4.2000000000000002, 410, 200m, "", "" },
                    { 18, "Akdeniz Blv. No:211, Liman, Konyaaltı/Antalya", "23:00", "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?w=800", "Sakin atmosfer, deniz manzarası ve özenli servis.", 0m, "", false, true, true, true, false, false, false, true, true, true, false, true, "", true, true, new DateTime(2026, 3, 21, 3, 45, 57, 965, DateTimeKind.Utc).AddTicks(1691), 36.882199999999997, 30.673500000000001, "Riviera Beach Lounge", 1, 30, "09:00", "", 4.7999999999999998, 95, 300m, "", "" }
                });

            migrationBuilder.UpdateData(
                table: "BusinessUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "UOm0YNf5xsCBeKQJM3Yxt8d8zNcBESfLxj3H1gKKQbE=");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.UpdateData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 20, 8, 6, 42, 880, DateTimeKind.Utc).AddTicks(6770));

            migrationBuilder.UpdateData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 20, 8, 6, 42, 881, DateTimeKind.Utc).AddTicks(3775));

            migrationBuilder.UpdateData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 20, 8, 6, 42, 881, DateTimeKind.Utc).AddTicks(3795));

            migrationBuilder.UpdateData(
                table: "Beaches",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 20, 8, 6, 42, 879, DateTimeKind.Utc).AddTicks(9241));

            migrationBuilder.UpdateData(
                table: "BusinessUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$uO5ZQBzO5F9KN6sICQ42uO.cCu/eF/4E/RUu18f9TCY4qnzpuDqhG");
        }
    }
}
