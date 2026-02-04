using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaHub.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplyTrackingToDosingRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupplyItemId",
                table: "DosingRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "DosingRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DosingRecords_SupplyItemId",
                table: "DosingRecords",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DosingRecords_UserId",
                table: "DosingRecords",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DosingRecords_AspNetUsers_UserId",
                table: "DosingRecords",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DosingRecords_SupplyItems_SupplyItemId",
                table: "DosingRecords",
                column: "SupplyItemId",
                principalTable: "SupplyItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DosingRecords_AspNetUsers_UserId",
                table: "DosingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DosingRecords_SupplyItems_SupplyItemId",
                table: "DosingRecords");

            migrationBuilder.DropIndex(
                name: "IX_DosingRecords_SupplyItemId",
                table: "DosingRecords");

            migrationBuilder.DropIndex(
                name: "IX_DosingRecords_UserId",
                table: "DosingRecords");

            migrationBuilder.DropColumn(
                name: "SupplyItemId",
                table: "DosingRecords");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DosingRecords");
        }
    }
}
