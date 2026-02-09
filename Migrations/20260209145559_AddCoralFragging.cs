using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AquaHub.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddCoralFragging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoralFrags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TankId = table.Column<int>(type: "integer", nullable: false),
                    ParentCoralId = table.Column<int>(type: "integer", nullable: true),
                    FragDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FragName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CoralSpecies = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InitialSize = table.Column<double>(type: "double precision", nullable: false),
                    NumberOfPolyps = table.Column<int>(type: "integer", nullable: true),
                    Coloration = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FraggingMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FraggingNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsEncrusted = table.Column<bool>(type: "boolean", nullable: false),
                    EncrustmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsReadyForSale = table.Column<bool>(type: "boolean", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SalePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    SoldTo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CurrentSize = table.Column<double>(type: "double precision", nullable: true),
                    LastMeasurementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LocationInTank = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MountedOn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ImagePath = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoralFrags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoralFrags_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoralFrags_Livestock_ParentCoralId",
                        column: x => x.ParentCoralId,
                        principalTable: "Livestock",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CoralFrags_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoralFrags_ParentCoralId",
                table: "CoralFrags",
                column: "ParentCoralId");

            migrationBuilder.CreateIndex(
                name: "IX_CoralFrags_TankId",
                table: "CoralFrags",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_CoralFrags_UserId",
                table: "CoralFrags",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoralFrags");
        }
    }
}
