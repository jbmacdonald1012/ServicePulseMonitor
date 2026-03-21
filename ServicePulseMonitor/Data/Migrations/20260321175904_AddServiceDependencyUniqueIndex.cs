using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicePulseMonitor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceDependencyUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_service_dependencies_service_id",
                table: "service_dependencies");

            migrationBuilder.CreateIndex(
                name: "IX_service_dependencies_service_id_depends_on_service_id",
                table: "service_dependencies",
                columns: new[] { "service_id", "depends_on_service_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_service_dependencies_service_id_depends_on_service_id",
                table: "service_dependencies");

            migrationBuilder.CreateIndex(
                name: "IX_service_dependencies_service_id",
                table: "service_dependencies",
                column: "service_id");
        }
    }
}
