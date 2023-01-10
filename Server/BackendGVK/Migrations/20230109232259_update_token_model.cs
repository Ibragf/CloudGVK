using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendGVK.Migrations
{
    public partial class update_token_model : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_RefreshTokens_TokenId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TokenId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TokenId",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Exp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FingerPrint = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ApplicationUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tokens_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_ApplicationUserId",
                table: "Tokens",
                column: "ApplicationUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.AddColumn<string>(
                name: "TokenId",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    exp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TokenId",
                table: "AspNetUsers",
                column: "TokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_RefreshTokens_TokenId",
                table: "AspNetUsers",
                column: "TokenId",
                principalTable: "RefreshTokens",
                principalColumn: "Id");
        }
    }
}
