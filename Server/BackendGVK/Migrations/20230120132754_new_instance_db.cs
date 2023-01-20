using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendGVK.Migrations
{
    public partial class new_instance_db : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Directories_Directories_ParentId",
                table: "Directories");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Directories_DirectoryModelId",
                table: "Files");

            migrationBuilder.DropTable(
                name: "ApplicationUserDirectoryModel");

            migrationBuilder.DropTable(
                name: "ApplicationUserFileModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Files",
                table: "Files");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Directories",
                table: "Directories");

            migrationBuilder.DropColumn(
                name: "DirectoryId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Directories");

            migrationBuilder.RenameTable(
                name: "Files",
                newName: "FileModel");

            migrationBuilder.RenameTable(
                name: "Directories",
                newName: "DirectoryModel");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "FileModel",
                newName: "Path");

            migrationBuilder.RenameColumn(
                name: "DirectoryModelId",
                table: "FileModel",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Files_DirectoryModelId",
                table: "FileModel",
                newName: "IX_FileModel_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "DirectoryModel",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "DirectoryModel",
                newName: "UntrustedName");

            migrationBuilder.RenameIndex(
                name: "IX_Directories_ParentId",
                table: "DirectoryModel",
                newName: "IX_DirectoryModel_ApplicationUserId");

            migrationBuilder.AddColumn<bool>(
                name: "isAdded",
                table: "FileModel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isShared",
                table: "FileModel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "DirectoryModel",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "isAdded",
                table: "DirectoryModel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isShared",
                table: "DirectoryModel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileModel",
                table: "FileModel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DirectoryModel",
                table: "DirectoryModel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectoryModel_AspNetUsers_ApplicationUserId",
                table: "DirectoryModel",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileModel_AspNetUsers_ApplicationUserId",
                table: "FileModel",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryModel_AspNetUsers_ApplicationUserId",
                table: "DirectoryModel");

            migrationBuilder.DropForeignKey(
                name: "FK_FileModel_AspNetUsers_ApplicationUserId",
                table: "FileModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FileModel",
                table: "FileModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DirectoryModel",
                table: "DirectoryModel");

            migrationBuilder.DropColumn(
                name: "isAdded",
                table: "FileModel");

            migrationBuilder.DropColumn(
                name: "isShared",
                table: "FileModel");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "DirectoryModel");

            migrationBuilder.DropColumn(
                name: "isAdded",
                table: "DirectoryModel");

            migrationBuilder.DropColumn(
                name: "isShared",
                table: "DirectoryModel");

            migrationBuilder.RenameTable(
                name: "FileModel",
                newName: "Files");

            migrationBuilder.RenameTable(
                name: "DirectoryModel",
                newName: "Directories");

            migrationBuilder.RenameColumn(
                name: "Path",
                table: "Files",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Files",
                newName: "DirectoryModelId");

            migrationBuilder.RenameIndex(
                name: "IX_FileModel_ApplicationUserId",
                table: "Files",
                newName: "IX_Files_DirectoryModelId");

            migrationBuilder.RenameColumn(
                name: "UntrustedName",
                table: "Directories",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Directories",
                newName: "ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_DirectoryModel_ApplicationUserId",
                table: "Directories",
                newName: "IX_Directories_ParentId");

            migrationBuilder.AddColumn<Guid>(
                name: "DirectoryId",
                table: "Files",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Directories",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Files",
                table: "Files",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Directories",
                table: "Directories",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Directories_Directories_ParentId",
                table: "Directories",
                column: "ParentId",
                principalTable: "Directories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Directories_DirectoryModelId",
                table: "Files",
                column: "DirectoryModelId",
                principalTable: "Directories",
                principalColumn: "Id");
        }
    }
}
