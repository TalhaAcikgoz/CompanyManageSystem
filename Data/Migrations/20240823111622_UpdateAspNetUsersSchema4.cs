using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyIdentityApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAspNetUsersSchema4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CvFilePath",
                table: "AspNetUsers",
                newName: "Department");

            migrationBuilder.CreateTable(
                name: "CVInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CVInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CVInfo_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CVInfo_ApplicationUserId",
                table: "CVInfo",
                column: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CVInfo");

            migrationBuilder.RenameColumn(
                name: "Department",
                table: "AspNetUsers",
                newName: "CvFilePath");
        }
    }
}
