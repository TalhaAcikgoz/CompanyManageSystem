using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyIdentityApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCVInfoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CVInfo_AspNetUsers_ApplicationUserId",
                table: "CVInfo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CVInfo",
                table: "CVInfo");

            migrationBuilder.RenameTable(
                name: "CVInfo",
                newName: "CVInfos");

            migrationBuilder.RenameIndex(
                name: "IX_CVInfo_ApplicationUserId",
                table: "CVInfos",
                newName: "IX_CVInfos_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CVInfos",
                table: "CVInfos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CVInfos_AspNetUsers_ApplicationUserId",
                table: "CVInfos",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CVInfos_AspNetUsers_ApplicationUserId",
                table: "CVInfos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CVInfos",
                table: "CVInfos");

            migrationBuilder.RenameTable(
                name: "CVInfos",
                newName: "CVInfo");

            migrationBuilder.RenameIndex(
                name: "IX_CVInfos_ApplicationUserId",
                table: "CVInfo",
                newName: "IX_CVInfo_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CVInfo",
                table: "CVInfo",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CVInfo_AspNetUsers_ApplicationUserId",
                table: "CVInfo",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
