using Microsoft.EntityFrameworkCore.Migrations;

namespace HW6MovieSharingSolution.Migrations
{
    public partial class Owner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AprovalStatus",
                table: "Movie",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Movie",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerEmail",
                table: "Movie",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isApproved",
                table: "Movie",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AprovalStatus",
                table: "Movie");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Movie");

            migrationBuilder.DropColumn(
                name: "OwnerEmail",
                table: "Movie");

            migrationBuilder.DropColumn(
                name: "isApproved",
                table: "Movie");
        }
    }
}
