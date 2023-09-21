using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addimageforservicepackagenotforservice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Services");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "ServicePackages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "ServicePackages");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
