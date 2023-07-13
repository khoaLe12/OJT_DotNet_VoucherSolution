using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImplementingHierarchyIdforUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<HierarchyId>(
                name: "PathFromRootManager",
                table: "AspNetUsers",
                type: "hierarchyid",
                nullable: false,
                defaultValueSql: "hierarchyid::Parse('/')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PathFromRootManager",
                table: "AspNetUsers");
        }
    }
}
