using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaHub.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddedImageService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Tanks",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "Tanks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Tanks");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "Tanks");
        }
    }
}
