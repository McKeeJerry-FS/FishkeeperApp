using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaHub.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddQuarantineTankFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsQuarantineTank",
                table: "Tanks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "QuarantineEndDate",
                table: "Tanks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuarantinePurpose",
                table: "Tanks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "QuarantineStartDate",
                table: "Tanks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuarantineStatus",
                table: "Tanks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TreatmentProtocol",
                table: "Tanks",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsQuarantineTank",
                table: "Tanks");

            migrationBuilder.DropColumn(
                name: "QuarantineEndDate",
                table: "Tanks");

            migrationBuilder.DropColumn(
                name: "QuarantinePurpose",
                table: "Tanks");

            migrationBuilder.DropColumn(
                name: "QuarantineStartDate",
                table: "Tanks");

            migrationBuilder.DropColumn(
                name: "QuarantineStatus",
                table: "Tanks");

            migrationBuilder.DropColumn(
                name: "TreatmentProtocol",
                table: "Tanks");
        }
    }
}
