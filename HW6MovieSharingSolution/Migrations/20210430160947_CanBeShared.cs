using Microsoft.EntityFrameworkCore.Migrations;

namespace HW6MovieSharingSolution.Migrations
{
    public partial class CanBeShared : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanBeShared",
                table: "Movie",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanBeShared",
                table: "Movie");
        }
    }
}
