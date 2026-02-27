using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaHub.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddLivestockCareFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SaltwaterInvertebrate_WaterParameters",
                table: "Livestock",
                newName: "WaterFlowNeeds");

            migrationBuilder.RenameColumn(
                name: "SaltwaterInvertebrate_Placement",
                table: "Livestock",
                newName: "SafetyWarnings");

            migrationBuilder.RenameColumn(
                name: "Plant_Placement",
                table: "Livestock",
                newName: "FeedingAndNutrition");

            migrationBuilder.RenameColumn(
                name: "Plant_LightingNeeds",
                table: "Livestock",
                newName: "CommonPests");

            migrationBuilder.RenameColumn(
                name: "Coral_Placement",
                table: "Livestock",
                newName: "CommonIllnesses");

            migrationBuilder.AddColumn<string>(
                name: "AggressionLevel",
                table: "Livestock",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AggressionLevel",
                table: "Livestock");

            migrationBuilder.RenameColumn(
                name: "WaterFlowNeeds",
                table: "Livestock",
                newName: "SaltwaterInvertebrate_WaterParameters");

            migrationBuilder.RenameColumn(
                name: "SafetyWarnings",
                table: "Livestock",
                newName: "SaltwaterInvertebrate_Placement");

            migrationBuilder.RenameColumn(
                name: "FeedingAndNutrition",
                table: "Livestock",
                newName: "Plant_Placement");

            migrationBuilder.RenameColumn(
                name: "CommonPests",
                table: "Livestock",
                newName: "Plant_LightingNeeds");

            migrationBuilder.RenameColumn(
                name: "CommonIllnesses",
                table: "Livestock",
                newName: "Coral_Placement");
        }
    }
}
