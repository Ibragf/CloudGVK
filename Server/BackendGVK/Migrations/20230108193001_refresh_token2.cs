using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendGVK.Migrations
{
    public partial class refresh_token2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_RefreshTokens_RefreshTokenId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenId",
                table: "AspNetUsers",
                newName: "TokenId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_RefreshTokenId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_TokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_RefreshTokens_TokenId",
                table: "AspNetUsers",
                column: "TokenId",
                principalTable: "RefreshTokens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_RefreshTokens_TokenId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "TokenId",
                table: "AspNetUsers",
                newName: "RefreshTokenId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_TokenId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_RefreshTokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_RefreshTokens_RefreshTokenId",
                table: "AspNetUsers",
                column: "RefreshTokenId",
                principalTable: "RefreshTokens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
