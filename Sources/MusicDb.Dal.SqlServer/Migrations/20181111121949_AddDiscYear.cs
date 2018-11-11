using Microsoft.EntityFrameworkCore.Migrations;

namespace MusicDb.Dal.SqlServer.Migrations
{
    public partial class AddDiscYear : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Discs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Year",
                table: "Discs");
        }
    }
}
