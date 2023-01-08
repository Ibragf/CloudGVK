using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendGVK.Migrations
{
    public partial class for_example : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RefreshTokens",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "RefreshTokens");
        }
    }
}
