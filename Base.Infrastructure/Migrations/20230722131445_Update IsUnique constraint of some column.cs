using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIsUniqueconstraintofsomecolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VoucherTypes_TypeName",
                table: "VoucherTypes");

            migrationBuilder.DropIndex(
                name: "IX_Services_ServiceName",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_ServicePackages_ServicePackageName",
                table: "ServicePackages");

            migrationBuilder.AlterColumn<string>(
                name: "TypeName",
                table: "VoucherTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "Services",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ServicePackageName",
                table: "ServicePackages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TypeName",
                table: "VoucherTypes",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "Services",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ServicePackageName",
                table: "ServicePackages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherTypes_TypeName",
                table: "VoucherTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_ServiceName",
                table: "Services",
                column: "ServiceName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_ServicePackageName",
                table: "ServicePackages",
                column: "ServicePackageName",
                unique: true);
        }
    }
}
