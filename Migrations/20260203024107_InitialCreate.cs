using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AquaHub.MVC.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tanks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VolumeGallons = table.Column<double>(type: "double precision", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ImagePath = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tanks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tanks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNotificationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    EmailNotificationsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ReminderNotificationsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    WaterParameterAlertsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    EquipmentAlertsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    EmailDigestFrequencyHours = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotificationSettings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DosingRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TankId = table.Column<int>(type: "integer", nullable: false),
                    Additive = table.Column<string>(type: "text", nullable: false),
                    AmountMl = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DosingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DosingRecords_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TankId = table.Column<int>(type: "integer", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    InstalledOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: true),
                    FlowRate = table.Column<double>(type: "double precision", nullable: true),
                    Media = table.Column<string>(type: "text", nullable: true),
                    LastMaintenanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MinTemperature = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxTemperature = table.Column<decimal>(type: "numeric", nullable: true),
                    Wattage = table.Column<double>(type: "double precision", nullable: true),
                    Spectrum = table.Column<string>(type: "text", nullable: true),
                    IsDimmable = table.Column<bool>(type: "boolean", nullable: true),
                    IntensityPercent = table.Column<int>(type: "integer", nullable: true),
                    Schedule = table.Column<string>(type: "text", nullable: true),
                    Capacity = table.Column<double>(type: "double precision", nullable: true),
                    ProteinSkimmer_Type = table.Column<string>(type: "text", nullable: true),
                    AirIntake = table.Column<int>(type: "integer", nullable: true),
                    CupFillLevel = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipment_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TankId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ImagePath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Livestock",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TankId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Species = table.Column<string>(type: "text", nullable: false),
                    AddedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    CoralType = table.Column<int>(type: "integer", nullable: true),
                    CoralFamily = table.Column<string>(type: "text", nullable: true),
                    ScientificName = table.Column<string>(type: "text", nullable: true),
                    ColonySize = table.Column<double>(type: "double precision", nullable: true),
                    GrowthRate = table.Column<string>(type: "text", nullable: true),
                    Coral_Coloration = table.Column<string>(type: "text", nullable: true),
                    PolyExtension = table.Column<string>(type: "text", nullable: true),
                    LightingNeeds = table.Column<string>(type: "text", nullable: true),
                    LightIntensityPAR = table.Column<string>(type: "text", nullable: true),
                    LightSpectrum = table.Column<string>(type: "text", nullable: true),
                    FlowNeeds = table.Column<string>(type: "text", nullable: true),
                    FlowType = table.Column<string>(type: "text", nullable: true),
                    Coral_Placement = table.Column<string>(type: "text", nullable: true),
                    SpacingRequirement = table.Column<double>(type: "double precision", nullable: true),
                    IsAggressive = table.Column<bool>(type: "boolean", nullable: true),
                    AggressionMethod = table.Column<string>(type: "text", nullable: true),
                    Coral_OptimalTemperatureMin = table.Column<double>(type: "double precision", nullable: true),
                    Coral_OptimalTemperatureMax = table.Column<double>(type: "double precision", nullable: true),
                    OptimalSalinityMin = table.Column<double>(type: "double precision", nullable: true),
                    OptimalSalinityMax = table.Column<double>(type: "double precision", nullable: true),
                    Coral_pHRange = table.Column<string>(type: "text", nullable: true),
                    AlkalinityRange = table.Column<string>(type: "text", nullable: true),
                    CalciumRange = table.Column<string>(type: "text", nullable: true),
                    MagnesiumRange = table.Column<string>(type: "text", nullable: true),
                    RequiresFeeding = table.Column<bool>(type: "boolean", nullable: true),
                    Coral_FoodTypes = table.Column<string>(type: "text", nullable: true),
                    Coral_FeedingFrequency = table.Column<string>(type: "text", nullable: true),
                    Coral_CareLevel = table.Column<string>(type: "text", nullable: true),
                    RequiresStableParameters = table.Column<bool>(type: "boolean", nullable: true),
                    RequiresDosing = table.Column<bool>(type: "boolean", nullable: true),
                    DosingRequirements = table.Column<string>(type: "text", nullable: true),
                    HasZooxanthellae = table.Column<bool>(type: "boolean", nullable: true),
                    IsToxic = table.Column<bool>(type: "boolean", nullable: true),
                    Coral_RequiresAcclimation = table.Column<bool>(type: "boolean", nullable: true),
                    Coral_SpecialRequirements = table.Column<string>(type: "text", nullable: true),
                    CanBeFragged = table.Column<bool>(type: "boolean", nullable: true),
                    FraggingDifficulty = table.Column<string>(type: "text", nullable: true),
                    FraggingMethod = table.Column<string>(type: "text", nullable: true),
                    FraggingNotes = table.Column<string>(type: "text", nullable: true),
                    Coral_CommonDiseases = table.Column<string>(type: "text", nullable: true),
                    StressSigns = table.Column<string>(type: "text", nullable: true),
                    FishType = table.Column<int>(type: "integer", nullable: true),
                    FreshwaterFish_AdultSize = table.Column<double>(type: "double precision", nullable: true),
                    FreshwaterFish_Coloration = table.Column<string>(type: "text", nullable: true),
                    BodyShape = table.Column<string>(type: "text", nullable: true),
                    HasLongFins = table.Column<bool>(type: "boolean", nullable: true),
                    Temperament = table.Column<string>(type: "text", nullable: true),
                    ActivityLevel = table.Column<string>(type: "text", nullable: true),
                    IsSchooling = table.Column<bool>(type: "boolean", nullable: true),
                    RecommendedSchoolSize = table.Column<int>(type: "integer", nullable: true),
                    IsNocturnal = table.Column<bool>(type: "boolean", nullable: true),
                    FreshwaterFish_MinTankSize = table.Column<double>(type: "double precision", nullable: true),
                    SwimmingRegion = table.Column<string>(type: "text", nullable: true),
                    RequiresPlants = table.Column<bool>(type: "boolean", nullable: true),
                    RequiresHidingSpots = table.Column<bool>(type: "boolean", nullable: true),
                    RequiresDriftwood = table.Column<bool>(type: "boolean", nullable: true),
                    FreshwaterFish_OptimalTemperatureMin = table.Column<double>(type: "double precision", nullable: true),
                    FreshwaterFish_OptimalTemperatureMax = table.Column<double>(type: "double precision", nullable: true),
                    FreshwaterFish_pHRange = table.Column<string>(type: "text", nullable: true),
                    FreshwaterFish_HardnessRange = table.Column<string>(type: "text", nullable: true),
                    PreferredWaterFlow = table.Column<string>(type: "text", nullable: true),
                    FreshwaterFish_Diet = table.Column<string>(type: "text", nullable: true),
                    FoodTypes = table.Column<string>(type: "text", nullable: true),
                    FreshwaterFish_FeedingFrequency = table.Column<string>(type: "text", nullable: true),
                    IsBottomFeeder = table.Column<bool>(type: "boolean", nullable: true),
                    FreshwaterFish_CareLevel = table.Column<string>(type: "text", nullable: true),
                    RequiresQuarantine = table.Column<bool>(type: "boolean", nullable: true),
                    RequiresCycledTank = table.Column<bool>(type: "boolean", nullable: true),
                    AggressiveToSameSpecies = table.Column<bool>(type: "boolean", nullable: true),
                    AggressiveToOtherFish = table.Column<bool>(type: "boolean", nullable: true),
                    FreshwaterFish_TankMateCompatibility = table.Column<string>(type: "text", nullable: true),
                    NipsAtFins = table.Column<bool>(type: "boolean", nullable: true),
                    EatsSmallFish = table.Column<bool>(type: "boolean", nullable: true),
                    EatsShrimp = table.Column<bool>(type: "boolean", nullable: true),
                    EatsSnails = table.Column<bool>(type: "boolean", nullable: true),
                    IsJumper = table.Column<bool>(type: "boolean", nullable: true),
                    IsLabyrinthFish = table.Column<bool>(type: "boolean", nullable: true),
                    RequiresAirstone = table.Column<bool>(type: "boolean", nullable: true),
                    FreshwaterFish_SpecialRequirements = table.Column<string>(type: "text", nullable: true),
                    FreshwaterFish_BreedingDifficulty = table.Column<string>(type: "text", nullable: true),
                    BreedingType = table.Column<string>(type: "text", nullable: true),
                    FreshwaterFish_BreedingNotes = table.Column<string>(type: "text", nullable: true),
                    FreshwaterFish_AverageLifespanYears = table.Column<int>(type: "integer", nullable: true),
                    ProneToIch = table.Column<bool>(type: "boolean", nullable: true),
                    ProneToDropsy = table.Column<bool>(type: "boolean", nullable: true),
                    FreshwaterFish_CommonDiseases = table.Column<string>(type: "text", nullable: true),
                    InvertebrateType = table.Column<int>(type: "integer", nullable: true),
                    AdultSize = table.Column<double>(type: "double precision", nullable: true),
                    Coloration = table.Column<string>(type: "text", nullable: true),
                    Habitat = table.Column<string>(type: "text", nullable: true),
                    Behavior = table.Column<string>(type: "text", nullable: true),
                    IsPlantSafe = table.Column<bool>(type: "boolean", nullable: true),
                    MinTankSize = table.Column<double>(type: "double precision", nullable: true),
                    Placement = table.Column<string>(type: "text", nullable: true),
                    WaterParameters = table.Column<string>(type: "text", nullable: true),
                    OptimalTemperatureMin = table.Column<double>(type: "double precision", nullable: true),
                    OptimalTemperatureMax = table.Column<double>(type: "double precision", nullable: true),
                    pHRange = table.Column<string>(type: "text", nullable: true),
                    HardnessRange = table.Column<string>(type: "text", nullable: true),
                    CareLevel = table.Column<string>(type: "text", nullable: true),
                    Diet = table.Column<string>(type: "text", nullable: true),
                    FeedingFrequency = table.Column<string>(type: "text", nullable: true),
                    AggressiveTowardsOwnSpecies = table.Column<bool>(type: "boolean", nullable: true),
                    TankMateCompatibility = table.Column<string>(type: "text", nullable: true),
                    IsAlgaeEater = table.Column<bool>(type: "boolean", nullable: true),
                    IsScavenger = table.Column<bool>(type: "boolean", nullable: true),
                    RequiresAcclimation = table.Column<bool>(type: "boolean", nullable: true),
                    SensitiveToCopper = table.Column<bool>(type: "boolean", nullable: true),
                    SpecialRequirements = table.Column<string>(type: "text", nullable: true),
                    BreedingDifficulty = table.Column<string>(type: "text", nullable: true),
                    BreedingNotes = table.Column<string>(type: "text", nullable: true),
                    AverageLifespanYears = table.Column<int>(type: "integer", nullable: true),
                    CommonDiseases = table.Column<string>(type: "text", nullable: true),
                    PlantType = table.Column<int>(type: "integer", nullable: true),
                    MaxHeight = table.Column<double>(type: "double precision", nullable: true),
                    Plant_GrowthRate = table.Column<string>(type: "text", nullable: true),
                    Plant_Coloration = table.Column<string>(type: "text", nullable: true),
                    Plant_Placement = table.Column<string>(type: "text", nullable: true),
                    Plant_LightingNeeds = table.Column<string>(type: "text", nullable: true),
                    LightingRequirement = table.Column<string>(type: "text", nullable: true),
                    Co2Needs = table.Column<string>(type: "text", nullable: true),
                    RequiresCO2 = table.Column<bool>(type: "boolean", nullable: true),
                    Plant_CareLevel = table.Column<string>(type: "text", nullable: true),
                    Plant_OptimalTemperatureMin = table.Column<double>(type: "double precision", nullable: true),
                    Plant_OptimalTemperatureMax = table.Column<double>(type: "double precision", nullable: true),
                    Plant_pHRange = table.Column<string>(type: "text", nullable: true),
                    Plant_HardnessRange = table.Column<string>(type: "text", nullable: true),
                    SubstrateRequirement = table.Column<string>(type: "text", nullable: true),
                    RequiresRootTabs = table.Column<bool>(type: "boolean", nullable: true),
                    RequiresFertilization = table.Column<bool>(type: "boolean", nullable: true),
                    CanBePropagated = table.Column<bool>(type: "boolean", nullable: true),
                    PropagationMethod = table.Column<string>(type: "text", nullable: true),
                    Plant_SpecialRequirements = table.Column<string>(type: "text", nullable: true),
                    SafeForShrimp = table.Column<bool>(type: "boolean", nullable: true),
                    SafeForSnails = table.Column<bool>(type: "boolean", nullable: true),
                    OxygenProducer = table.Column<bool>(type: "boolean", nullable: true),
                    NitrateAbsorber = table.Column<bool>(type: "boolean", nullable: true),
                    Benefits = table.Column<string>(type: "text", nullable: true),
                    CommonIssues = table.Column<string>(type: "text", nullable: true),
                    SaltwaterFish_FishType = table.Column<int>(type: "integer", nullable: true),
                    SaltwaterFish_AdultSize = table.Column<double>(type: "double precision", nullable: true),
                    SaltwaterFish_Coloration = table.Column<string>(type: "text", nullable: true),
                    SaltwaterFish_BodyShape = table.Column<string>(type: "text", nullable: true),
                    SaltwaterFish_Temperament = table.Column<string>(type: "text", nullable: true),
                    SaltwaterFish_ActivityLevel = table.Column<string>(type: "text", nullable: true),
                    IsReefSafe = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterFish_IsSchooling = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterFish_RecommendedSchoolSize = table.Column<int>(type: "integer", nullable: true),
                    SaltwaterFish_MinTankSize = table.Column<double>(type: "double precision", nullable: true),
                    SaltwaterFish_SwimmingRegion = table.Column<string>(type: "text", nullable: true),
                    RequiresLiveRock = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterFish_RequiresHidingSpots = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterFish_OptimalTemperatureMin = table.Column<double>(type: "double precision", nullable: true),
                    SaltwaterFish_OptimalTemperatureMax = table.Column<double>(type: "double precision", nullable: true),
                    SaltwaterFish_OptimalSalinityMin = table.Column<double>(type: "double precision", nullable: true),
                    SaltwaterFish_OptimalSalinityMax = table.Column<double>(type: "double precision", nullable: true),
                    SaltwaterFish_pHRange = table.Column<string>(type: "text", nullable: true),
                    SaltwaterFish_Diet = table.Column<string>(type: "text", nullable: true),
                    SaltwaterFish_FoodTypes = table.Column<string>(type: "text", nullable: true),
                    SaltwaterFish_FeedingFrequency = table.Column<string>(type: "text", nullable: true),
                    FeedingBehavior = table.Column<string>(type: "text", nullable: true),
                    SaltwaterFish_CareLevel = table.Column<string>(type: "text", nullable: true),
                    SaltwaterFish_RequiresQuarantine = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterFish_AggressiveToSameSpecies = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterFish_AggressiveToOtherFish = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterFish_TankMateCompatibility = table.Column<string>(type: "text", nullable: true),
                    NipsAtCorals = table.Column<bool>(type: "boolean", nullable: true),
                    NipsAtInvertebrates = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterFish_IsJumper = table.Column<bool>(type: "boolean", nullable: true),
                    IsVenomous = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterFish_SpecialRequirements = table.Column<string>(type: "text", nullable: true),
                    SaltwaterFish_BreedingDifficulty = table.Column<string>(type: "text", nullable: true),
                    SaltwaterFish_BreedingNotes = table.Column<string>(type: "text", nullable: true),
                    SaltwaterFish_AverageLifespanYears = table.Column<int>(type: "integer", nullable: true),
                    SaltwaterFish_ProneToIch = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterFish_CommonDiseases = table.Column<string>(type: "text", nullable: true),
                    SaltwaterInvertebrate_InvertebrateType = table.Column<int>(type: "integer", nullable: true),
                    SaltwaterInvertebrate_AdultSize = table.Column<double>(type: "double precision", nullable: true),
                    SaltwaterInvertebrate_Coloration = table.Column<string>(type: "text", nullable: true),
                    SaltwaterInvertebrate_Habitat = table.Column<string>(type: "text", nullable: true),
                    SaltwaterInvertebrate_Behavior = table.Column<string>(type: "text", nullable: true),
                    SaltwaterInvertebrate_IsReefSafe = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterInvertebrate_MinTankSize = table.Column<double>(type: "double precision", nullable: true),
                    SaltwaterInvertebrate_Placement = table.Column<string>(type: "text", nullable: true),
                    SaltwaterInvertebrate_WaterParameters = table.Column<string>(type: "text", nullable: true),
                    SaltwaterInvertebrate_OptimalTemperatureMin = table.Column<double>(type: "double precision", nullable: true),
                    SaltwaterInvertebrate_OptimalTemperatureMax = table.Column<double>(type: "double precision", nullable: true),
                    SaltwaterInvertebrate_OptimalSalinityMin = table.Column<double>(type: "double precision", nullable: true),
                    SaltwaterInvertebrate_OptimalSalinityMax = table.Column<double>(type: "double precision", nullable: true),
                    SaltwaterInvertebrate_pHRange = table.Column<string>(type: "text", nullable: true),
                    SaltwaterInvertebrate_CareLevel = table.Column<string>(type: "text", nullable: true),
                    SaltwaterInvertebrate_Diet = table.Column<string>(type: "text", nullable: true),
                    SaltwaterInvertebrate_FeedingFrequency = table.Column<string>(type: "text", nullable: true),
                    SaltwaterInvertebrate_AggressiveTowardsOwnSpecies = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterInvertebrate_TankMateCompatibility = table.Column<string>(type: "text", nullable: true),
                    IsCleaner = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterInvertebrate_RequiresAcclimation = table.Column<bool>(type: "boolean", nullable: true),
                    SaltwaterInvertebrate_SpecialRequirements = table.Column<string>(type: "text", nullable: true),
                    SaltwaterInvertebrate_AverageLifespanYears = table.Column<int>(type: "integer", nullable: true),
                    SaltwaterInvertebrate_CommonDiseases = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livestock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Livestock_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TankId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    WaterChangePercent = table.Column<double>(type: "double precision", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceLogs_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParameterAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    TankId = table.Column<int>(type: "integer", nullable: false),
                    Parameter = table.Column<int>(type: "integer", nullable: false),
                    MinValue = table.Column<double>(type: "double precision", nullable: true),
                    MaxValue = table.Column<double>(type: "double precision", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    CustomMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastTriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TriggerCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParameterAlerts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParameterAlerts_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhotoLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TankId = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhotoLogs_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PredictiveReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    TankId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    SuggestedType = table.Column<int>(type: "integer", nullable: false),
                    SuggestedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "double precision", nullable: false),
                    Reasoning = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    PatternOccurrences = table.Column<int>(type: "integer", nullable: false),
                    AverageDaysBetween = table.Column<double>(type: "double precision", nullable: false),
                    LastOccurrence = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDismissed = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DismissedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedReminderId = table.Column<int>(type: "integer", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredictiveReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PredictiveReminders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PredictiveReminders_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    TankId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    NextDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastCompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SendEmailNotification = table.Column<bool>(type: "boolean", nullable: false),
                    NotificationHoursBefore = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reminders_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WaterTests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TankId = table.Column<int>(type: "integer", nullable: false),
                    PH = table.Column<double>(type: "double precision", nullable: true),
                    Temperature = table.Column<double>(type: "double precision", nullable: true),
                    Ammonia = table.Column<double>(type: "double precision", nullable: true),
                    Nitrite = table.Column<double>(type: "double precision", nullable: true),
                    Nitrate = table.Column<double>(type: "double precision", nullable: true),
                    GH = table.Column<double>(type: "double precision", nullable: true),
                    KH = table.Column<double>(type: "double precision", nullable: true),
                    TDS = table.Column<double>(type: "double precision", nullable: true),
                    Salinity = table.Column<double>(type: "double precision", nullable: true),
                    Alkalinity = table.Column<double>(type: "double precision", nullable: true),
                    Calcium = table.Column<double>(type: "double precision", nullable: true),
                    Magnesium = table.Column<double>(type: "double precision", nullable: true),
                    Phosphate = table.Column<double>(type: "double precision", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaterTests_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    TankId = table.Column<int>(type: "integer", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    ItemName = table.Column<string>(type: "text", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    Vendor = table.Column<string>(type: "text", nullable: true),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: true),
                    ReceiptNumber = table.Column<string>(type: "text", nullable: true),
                    ReceiptPhoto = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    IsRecurring = table.Column<bool>(type: "boolean", nullable: false),
                    RecurrenceFrequency = table.Column<string>(type: "text", nullable: true),
                    EquipmentId = table.Column<int>(type: "integer", nullable: true),
                    LivestockId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Expenses_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Expenses_Livestock_LivestockId",
                        column: x => x.LivestockId,
                        principalTable: "Livestock",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Expenses_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GrowthRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LivestockId = table.Column<int>(type: "integer", nullable: false),
                    MeasurementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LengthInches = table.Column<double>(type: "double precision", nullable: true),
                    WeightGrams = table.Column<double>(type: "double precision", nullable: true),
                    DiameterInches = table.Column<double>(type: "double precision", nullable: true),
                    HeightInches = table.Column<double>(type: "double precision", nullable: true),
                    HealthCondition = table.Column<string>(type: "text", nullable: true),
                    ColorVibrancy = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    PhotoPath = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrowthRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrowthRecords_Livestock_LivestockId",
                        column: x => x.LivestockId,
                        principalTable: "Livestock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalMaintenanceLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JournalEntryId = table.Column<int>(type: "integer", nullable: false),
                    MaintenanceLogId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalMaintenanceLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalMaintenanceLinks_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JournalMaintenanceLinks_MaintenanceLogs_MaintenanceLogId",
                        column: x => x.MaintenanceLogId,
                        principalTable: "MaintenanceLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalWaterTestLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JournalEntryId = table.Column<int>(type: "integer", nullable: false),
                    WaterTestId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalWaterTestLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalWaterTestLinks_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JournalWaterTestLinks_WaterTests_WaterTestId",
                        column: x => x.WaterTestId,
                        principalTable: "WaterTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    TankId = table.Column<int>(type: "integer", nullable: true),
                    ReminderId = table.Column<int>(type: "integer", nullable: true),
                    WaterTestId = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    EmailSent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActionUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Severity = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Reminders_ReminderId",
                        column: x => x.ReminderId,
                        principalTable: "Reminders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_WaterTests_WaterTestId",
                        column: x => x.WaterTestId,
                        principalTable: "WaterTests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TriggeredAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParameterAlertId = table.Column<int>(type: "integer", nullable: false),
                    WaterTestId = table.Column<int>(type: "integer", nullable: false),
                    TankId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Parameter = table.Column<int>(type: "integer", nullable: false),
                    ActualValue = table.Column<double>(type: "double precision", nullable: false),
                    MinSafeValue = table.Column<double>(type: "double precision", nullable: true),
                    MaxSafeValue = table.Column<double>(type: "double precision", nullable: true),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByWaterTestId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriggeredAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TriggeredAlerts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TriggeredAlerts_ParameterAlerts_ParameterAlertId",
                        column: x => x.ParameterAlertId,
                        principalTable: "ParameterAlerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TriggeredAlerts_Tanks_TankId",
                        column: x => x.TankId,
                        principalTable: "Tanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TriggeredAlerts_WaterTests_WaterTestId",
                        column: x => x.WaterTestId,
                        principalTable: "WaterTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DosingRecords_TankId",
                table: "DosingRecords",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_TankId",
                table: "Equipment",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_EquipmentId",
                table: "Expenses",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_LivestockId",
                table: "Expenses",
                column: "LivestockId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_TankId",
                table: "Expenses",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GrowthRecords_LivestockId",
                table: "GrowthRecords",
                column: "LivestockId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_TankId",
                table: "JournalEntries",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalMaintenanceLinks_JournalEntryId",
                table: "JournalMaintenanceLinks",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalMaintenanceLinks_MaintenanceLogId",
                table: "JournalMaintenanceLinks",
                column: "MaintenanceLogId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalWaterTestLinks_JournalEntryId",
                table: "JournalWaterTestLinks",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalWaterTestLinks_WaterTestId",
                table: "JournalWaterTestLinks",
                column: "WaterTestId");

            migrationBuilder.CreateIndex(
                name: "IX_Livestock_TankId",
                table: "Livestock",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceLogs_TankId",
                table: "MaintenanceLogs",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ReminderId",
                table: "Notifications",
                column: "ReminderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TankId",
                table: "Notifications",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_WaterTestId",
                table: "Notifications",
                column: "WaterTestId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterAlerts_TankId",
                table: "ParameterAlerts",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterAlerts_UserId",
                table: "ParameterAlerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoLogs_TankId",
                table: "PhotoLogs",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictiveReminders_TankId",
                table: "PredictiveReminders",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictiveReminders_UserId",
                table: "PredictiveReminders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_TankId",
                table: "Reminders",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserId",
                table: "Reminders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tanks_UserId",
                table: "Tanks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TriggeredAlerts_ParameterAlertId",
                table: "TriggeredAlerts",
                column: "ParameterAlertId");

            migrationBuilder.CreateIndex(
                name: "IX_TriggeredAlerts_TankId",
                table: "TriggeredAlerts",
                column: "TankId");

            migrationBuilder.CreateIndex(
                name: "IX_TriggeredAlerts_UserId",
                table: "TriggeredAlerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TriggeredAlerts_WaterTestId",
                table: "TriggeredAlerts",
                column: "WaterTestId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationSettings_UserId",
                table: "UserNotificationSettings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WaterTests_TankId",
                table: "WaterTests",
                column: "TankId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DosingRecords");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "GrowthRecords");

            migrationBuilder.DropTable(
                name: "JournalMaintenanceLinks");

            migrationBuilder.DropTable(
                name: "JournalWaterTestLinks");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PhotoLogs");

            migrationBuilder.DropTable(
                name: "PredictiveReminders");

            migrationBuilder.DropTable(
                name: "TriggeredAlerts");

            migrationBuilder.DropTable(
                name: "UserNotificationSettings");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "Livestock");

            migrationBuilder.DropTable(
                name: "MaintenanceLogs");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "ParameterAlerts");

            migrationBuilder.DropTable(
                name: "WaterTests");

            migrationBuilder.DropTable(
                name: "Tanks");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
