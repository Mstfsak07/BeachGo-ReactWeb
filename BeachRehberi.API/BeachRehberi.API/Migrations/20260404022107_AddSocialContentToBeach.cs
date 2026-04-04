using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachRehberi.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialContentToBeach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstagramUsername",
                table: "Beaches",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SocialContentSource",
                table: "Beaches",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstagramUsername",
                table: "Beaches");

            migrationBuilder.DropColumn(
                name: "SocialContentSource",
                table: "Beaches");
        }
    }
}
