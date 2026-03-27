using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachRehberi.API.Migrations
{
    /// <inheritdoc />
    public partial class ProductionAuthImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReasonRevoked",
                table: "RefreshTokens",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReplacedByToken",
                table: "RefreshTokens",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessUsers_Email",
                table: "BusinessUsers",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BusinessUsers_Email",
                table: "BusinessUsers");

            migrationBuilder.DropColumn(
                name: "ReasonRevoked",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "ReplacedByToken",
                table: "RefreshTokens");
        }
    }
}
