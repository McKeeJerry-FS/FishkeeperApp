using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AquaHub.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddBreeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BreedingPairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TankId = table.Column<int>(type: "integer", nullable: false),
                    PairName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Parent1Id = table.Column<int>(type: "integer", nullable: true),
                    Parent2Id = table.Column<int>(type: "integer", nullable: true),
                    Species = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BreedingType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DatePaired = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BreedingSetup = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    WaterConditions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DietConditioning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BehaviorNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ImagePath = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreedingPairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BreedingPairs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BreedingPairs_Livestock_Parent1Id",
                        column: x => x.Parent1Id,
                        principalTable: "Livestock",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BreedingPairs_Livestock_Parent2Id",
                        column: x => x.Parent2Id,
                        principalTable: "Livestock",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BreedingPairs_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BreedingAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BreedingPairId = table.Column<int>(type: "integer", nullable: false),
                    AttemptDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SpawningObserved = table.Column<bool>(type: "boolean", nullable: false),
                    SpawningDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EggsLaid = table.Column<int>(type: "integer", nullable: true),
                    EggsFertilized = table.Column<int>(type: "integer", nullable: true),
                    HatchingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NumberHatched = table.Column<int>(type: "integer", nullable: true),
                    OffspringCount = table.Column<int>(type: "integer", nullable: true),
                    SurvivalRatePercent = table.Column<double>(type: "double precision", nullable: true),
                    CurrentSurvivors = table.Column<int>(type: "integer", nullable: true),
                    WaterTemperature = table.Column<double>(type: "double precision", nullable: true),
                    PhLevel = table.Column<double>(type: "double precision", nullable: true),
                    Hardness = table.Column<int>(type: "integer", nullable: true),
                    BreedingTrigger = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BehaviorObservations = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ParentalCareNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FeedingRegimen = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Challenges = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ImagePath = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreedingAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BreedingAttempts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BreedingAttempts_BreedingPairs_BreedingPairId",
                        column: x => x.BreedingPairId,
                        principalTable: "BreedingPairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BreedingAttempts_BreedingPairId",
                table: "BreedingAttempts",
                column: "BreedingPairId");

            migrationBuilder.CreateIndex(
                name: "IX_BreedingAttempts_UserId",
                table: "BreedingAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BreedingPairs_Parent1Id",
                table: "BreedingPairs",
                column: "Parent1Id");

            migrationBuilder.CreateIndex(
                name: "IX_BreedingPairs_Parent2Id",
                table: "BreedingPairs",
                column: "Parent2Id");

            migrationBuilder.CreateIndex(
                name: "IX_BreedingPairs_TankId",
                table: "BreedingPairs",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_BreedingPairs_UserId",
                table: "BreedingPairs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BreedingAttempts");

            migrationBuilder.DropTable(
                name: "BreedingPairs");
        }
    }
}
