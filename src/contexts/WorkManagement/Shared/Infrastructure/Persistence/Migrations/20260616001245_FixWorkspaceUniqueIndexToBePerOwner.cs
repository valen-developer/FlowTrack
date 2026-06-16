using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowTrack.WorkManagement.Shared.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixWorkspaceUniqueIndexToBePerOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workspaces_Name",
                table: "workspaces");

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_OwnerId_Name",
                table: "workspaces",
                columns: new[] { "OwnerId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workspaces_OwnerId_Name",
                table: "workspaces");

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_Name",
                table: "workspaces",
                column: "Name",
                unique: true);
        }
    }
}
