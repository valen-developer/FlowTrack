using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowTrack.WorkManagement.Shared.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateWorkspaceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workspaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workspaces", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_Name",
                table: "workspaces",
                column: "Name",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_workspaces_Name", table: "workspaces");
            migrationBuilder.DropTable(name: "workspaces");
        }
    }
}
