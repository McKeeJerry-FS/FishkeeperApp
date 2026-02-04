using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaHub.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplyTrackingToMaintenanceLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AmountUsed",
                table: "MaintenanceLogs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplyItemId",
                table: "MaintenanceLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceLogs_SupplyItemId",
                table: "MaintenanceLogs",
                column: "SupplyItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceLogs_SupplyItems_SupplyItemId",
                table: "MaintenanceLogs",
                column: "SupplyItemId",
                principalTable: "SupplyItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceLogs_SupplyItems_SupplyItemId",
                table: "MaintenanceLogs");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceLogs_SupplyItemId",
                table: "MaintenanceLogs");

            migrationBuilder.DropColumn(
                name: "AmountUsed",
                table: "MaintenanceLogs");

            migrationBuilder.DropColumn(
                name: "SupplyItemId",
                table: "MaintenanceLogs");
        }
    }
}
