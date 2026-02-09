using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaHub.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddIdealWaterParametersToBreedingPairFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdealGhMax",
                table: "BreedingPairs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdealGhMin",
                table: "BreedingPairs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdealKhMax",
                table: "BreedingPairs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdealKhMin",
                table: "BreedingPairs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "IdealPhMax",
                table: "BreedingPairs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "IdealPhMin",
                table: "BreedingPairs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "IdealTempMax",
                table: "BreedingPairs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "IdealTempMin",
                table: "BreedingPairs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MaxAmmonia",
                table: "BreedingPairs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MaxNitrate",
                table: "BreedingPairs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MaxNitrite",
                table: "BreedingPairs",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdealGhMax",
                table: "BreedingPairs");

            migrationBuilder.DropColumn(
                name: "IdealGhMin",
                table: "BreedingPairs");

            migrationBuilder.DropColumn(
                name: "IdealKhMax",
                table: "BreedingPairs");

            migrationBuilder.DropColumn(
                name: "IdealKhMin",
                table: "BreedingPairs");

            migrationBuilder.DropColumn(
                name: "IdealPhMax",
                table: "BreedingPairs");

            migrationBuilder.DropColumn(
                name: "IdealPhMin",
                table: "BreedingPairs");

            migrationBuilder.DropColumn(
                name: "IdealTempMax",
                table: "BreedingPairs");

            migrationBuilder.DropColumn(
                name: "IdealTempMin",
                table: "BreedingPairs");

            migrationBuilder.DropColumn(
                name: "MaxAmmonia",
                table: "BreedingPairs");

            migrationBuilder.DropColumn(
                name: "MaxNitrate",
                table: "BreedingPairs");

            migrationBuilder.DropColumn(
                name: "MaxNitrite",
                table: "BreedingPairs");
        }
    }
}
