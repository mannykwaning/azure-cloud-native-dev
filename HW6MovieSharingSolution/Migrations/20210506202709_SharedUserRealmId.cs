using Microsoft.EntityFrameworkCore.Migrations;

namespace HW6MovieSharingSolution.Migrations
{
    public partial class SharedUserRealmId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SharedUserRealmId",
                table: "Movie",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SharedUserRealmId",
                table: "Movie");
        }
    }
}
