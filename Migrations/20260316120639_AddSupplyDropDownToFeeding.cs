using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaHub.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplyDropDownToFeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupplyItemId",
                table: "FeedingRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedingRecords_SupplyItemId",
                table: "FeedingRecords",
                column: "SupplyItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_FeedingRecords_SupplyItems_SupplyItemId",
                table: "FeedingRecords",
                column: "SupplyItemId",
                principalTable: "SupplyItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeedingRecords_SupplyItems_SupplyItemId",
                table: "FeedingRecords");

            migrationBuilder.DropIndex(
                name: "IX_FeedingRecords_SupplyItemId",
                table: "FeedingRecords");

            migrationBuilder.DropColumn(
                name: "SupplyItemId",
                table: "FeedingRecords");
        }
    }
}
