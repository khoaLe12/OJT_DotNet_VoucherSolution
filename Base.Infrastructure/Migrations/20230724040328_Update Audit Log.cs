using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AffectedColumns",
                table: "Logs");

            migrationBuilder.RenameColumn(
                name: "OldValue",
                table: "Logs",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "NewValue",
                table: "Logs",
                newName: "Changes");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Logs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Logs",
                newName: "OldValue");

            migrationBuilder.RenameColumn(
                name: "Changes",
                table: "Logs",
                newName: "NewValue");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Logs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AffectedColumns",
                table: "Logs",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
