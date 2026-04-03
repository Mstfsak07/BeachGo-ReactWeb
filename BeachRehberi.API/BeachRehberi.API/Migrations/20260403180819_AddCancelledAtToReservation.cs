using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachRehberi.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelledAtToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Reservations",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Reservations");
        }
    }
}
