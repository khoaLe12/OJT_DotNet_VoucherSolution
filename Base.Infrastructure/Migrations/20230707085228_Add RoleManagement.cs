using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleManagements",
                columns: table => new
                {
                    ManagedRoleId = table.Column<int>(type: "int", nullable: false),
                    ManagerRoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleManagements", x => new { x.ManagedRoleId, x.ManagerRoleId });
                    table.ForeignKey(
                        name: "FK_RoleManagements_Roles_ManagedRoleId",
                        column: x => x.ManagedRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoleManagements_Roles_ManagerRoleId",
                        column: x => x.ManagerRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleManagements_ManagerRoleId",
                table: "RoleManagements",
                column: "ManagerRoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleManagements");
        }
    }
}
