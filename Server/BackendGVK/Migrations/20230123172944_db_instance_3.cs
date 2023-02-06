using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendGVK.Migrations
{
    public partial class db_instance_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Path",
                table: "FileModel",
                newName: "CloudPath");

            migrationBuilder.RenameColumn(
                name: "Path",
                table: "DirectoryModel",
                newName: "CloudPath");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CloudPath",
                table: "FileModel",
                newName: "Path");

            migrationBuilder.RenameColumn(
                name: "CloudPath",
                table: "DirectoryModel",
                newName: "Path");
        }
    }
}
