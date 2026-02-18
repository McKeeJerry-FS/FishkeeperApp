using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaHub.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantedTankWaterParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CO2",
                table: "WaterTests",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Iron",
                table: "WaterTests",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CO2",
                table: "WaterTests");

            migrationBuilder.DropColumn(
                name: "Iron",
                table: "WaterTests");
        }
    }
}
