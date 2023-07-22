using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateonServicePackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ServicePackageName",
                table: "ServicePackages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_ServicePackageName",
                table: "ServicePackages",
                column: "ServicePackageName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServicePackages_ServicePackageName",
                table: "ServicePackages");

            migrationBuilder.AlterColumn<string>(
                name: "ServicePackageName",
                table: "ServicePackages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
