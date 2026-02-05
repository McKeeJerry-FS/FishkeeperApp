using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaHub.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceLogImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "MaintenanceLogs",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "MaintenanceLogs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "MaintenanceLogs");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "MaintenanceLogs");
        }
    }
}
