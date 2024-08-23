using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyIdentityApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAspNetUsersSchema2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CvFilePath",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CvFilePath",
                table: "AspNetUsers");
        }
    }
}
