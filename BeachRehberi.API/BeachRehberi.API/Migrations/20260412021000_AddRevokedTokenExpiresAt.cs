using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachRehberi.API.Migrations
{
    public partial class AddRevokedTokenExpiresAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "RevokedTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() + INTERVAL '15 minutes'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "RevokedTokens");
        }
    }
}
