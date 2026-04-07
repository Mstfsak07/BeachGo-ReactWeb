using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachRehberi.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBusinessUserAndAuthModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "VerificationCodes");

            migrationBuilder.RenameColumn(
                name: "ExpireDate",
                table: "VerificationCodes",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "Attempts",
                table: "VerificationCodes",
                newName: "Purpose");

            migrationBuilder.AddColumn<string>(
                name: "CodeHash",
                table: "VerificationCodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "BusinessUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "BusinessUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "BusinessUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "BusinessUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeHash",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "BusinessUsers");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "BusinessUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "BusinessUsers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "BusinessUsers");

            migrationBuilder.RenameColumn(
                name: "Purpose",
                table: "VerificationCodes",
                newName: "Attempts");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "VerificationCodes",
                newName: "ExpireDate");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "VerificationCodes",
                type: "TEXT",
                maxLength: 6,
                nullable: false,
                defaultValue: "");
        }
    }
}
