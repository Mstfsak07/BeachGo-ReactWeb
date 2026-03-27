using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachRehberi.API.Migrations
{
    /// <inheritdoc />
    public partial class MakeBusinessUserBeachOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessUsers_Beaches_BeachId",
                table: "BusinessUsers");

            migrationBuilder.AlterColumn<int>(
                name: "BeachId",
                table: "BusinessUsers",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessUsers_Beaches_BeachId",
                table: "BusinessUsers",
                column: "BeachId",
                principalTable: "Beaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessUsers_Beaches_BeachId",
                table: "BusinessUsers");

            migrationBuilder.AlterColumn<int>(
                name: "BeachId",
                table: "BusinessUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessUsers_Beaches_BeachId",
                table: "BusinessUsers",
                column: "BeachId",
                principalTable: "Beaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
