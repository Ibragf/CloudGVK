using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendGVK.Migrations
{
    public partial class add_files_folders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Directories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Size = table.Column<ulong>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<string>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Directories_Directories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Directories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserDirectoryModel",
                columns: table => new
                {
                    AllowedUsersId = table.Column<string>(type: "TEXT", nullable: false),
                    DirectoriesId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserDirectoryModel", x => new { x.AllowedUsersId, x.DirectoriesId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserDirectoryModel_AspNetUsers_AllowedUsersId",
                        column: x => x.AllowedUsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserDirectoryModel_Directories_DirectoriesId",
                        column: x => x.DirectoriesId,
                        principalTable: "Directories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UntrustedName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TrustedName = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<ulong>(type: "INTEGER", nullable: false),
                    MD5Hash = table.Column<string>(type: "TEXT", nullable: false),
                    DirectoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<string>(type: "TEXT", nullable: false),
                    DirectoryModelId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_Directories_DirectoryModelId",
                        column: x => x.DirectoryModelId,
                        principalTable: "Directories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserFileModel",
                columns: table => new
                {
                    AllowedUsersId = table.Column<string>(type: "TEXT", nullable: false),
                    FilesId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserFileModel", x => new { x.AllowedUsersId, x.FilesId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserFileModel_AspNetUsers_AllowedUsersId",
                        column: x => x.AllowedUsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserFileModel_Files_FilesId",
                        column: x => x.FilesId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserDirectoryModel_DirectoriesId",
                table: "ApplicationUserDirectoryModel",
                column: "DirectoriesId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserFileModel_FilesId",
                table: "ApplicationUserFileModel",
                column: "FilesId");

            migrationBuilder.CreateIndex(
                name: "IX_Directories_ParentId",
                table: "Directories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_DirectoryModelId",
                table: "Files",
                column: "DirectoryModelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserDirectoryModel");

            migrationBuilder.DropTable(
                name: "ApplicationUserFileModel");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Directories");
        }
    }
}
