using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaHub.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalEquipmentTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AutoFeeder_LastRefillDate",
                table: "Equipment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BTUCapacity",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BulbInstalledDate",
                table: "Equipment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BulbLifespanHours",
                table: "Equipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CalibrationIntervalDays",
                table: "Equipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentPH",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiffuserType",
                table: "Equipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DoseAmount",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DosingSchedule",
                table: "Equipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedingSchedule",
                table: "Equipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FeedingsPerDay",
                table: "Equipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GPDRating",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAlerts",
                table: "Equipment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasWifiConnectivity",
                table: "Equipment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Heater_MaxTemperature",
                table: "Equipment",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Heater_MinTemperature",
                table: "Equipment",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HopperCapacity",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsControllable",
                table: "Equipment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVariableSpeed",
                table: "Equipment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCalibrationDate",
                table: "Equipment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFilterChange",
                table: "Equipment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMediaChange",
                table: "Equipment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMembraneChange",
                table: "Equipment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRefillDate",
                table: "Equipment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReservoirRefill",
                table: "Equipment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MaxHeadHeight",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MaxTankSize",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MediaCapacity",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "Equipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MembraneLifespanMonths",
                table: "Equipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfChannels",
                table: "Equipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfStages",
                table: "Equipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParametersMonitored",
                table: "Equipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PortionSize",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PowerConsumption",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PumpRate",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Pump_FlowRate",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReactorType",
                table: "Equipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Reactor_FlowRate",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegulatorType",
                table: "Equipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ReservoirCapacity",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SensorType",
                table: "Equipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SolutionType",
                table: "Equipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TargetPH",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetTemperature",
                table: "Equipment",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UVSterilizer_FlowRate",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UVSterilizer_MaxTankSize",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UVSterilizer_Wattage",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WasteWaterRatio",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WaveMaker_FlowRate",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WaveMaker_IntensityPercent",
                table: "Equipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WaveMaker_PowerConsumption",
                table: "Equipment",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WavePattern",
                table: "Equipment",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoFeeder_LastRefillDate",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "BTUCapacity",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "BulbInstalledDate",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "BulbLifespanHours",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "CalibrationIntervalDays",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "CurrentPH",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "DiffuserType",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "DoseAmount",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "DosingSchedule",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "FeedingSchedule",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "FeedingsPerDay",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "GPDRating",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "HasAlerts",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "HasWifiConnectivity",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "Heater_MaxTemperature",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "Heater_MinTemperature",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "HopperCapacity",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "IsControllable",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "IsVariableSpeed",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "LastCalibrationDate",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "LastFilterChange",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "LastMediaChange",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "LastMembraneChange",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "LastRefillDate",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "LastReservoirRefill",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "MaxHeadHeight",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "MaxTankSize",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "MediaCapacity",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "MembraneLifespanMonths",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "NumberOfChannels",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "NumberOfStages",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "ParametersMonitored",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "PortionSize",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "PowerConsumption",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "PumpRate",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "Pump_FlowRate",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "ReactorType",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "Reactor_FlowRate",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "RegulatorType",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "ReservoirCapacity",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "SensorType",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "SolutionType",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "TargetPH",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "TargetTemperature",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "UVSterilizer_FlowRate",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "UVSterilizer_MaxTankSize",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "UVSterilizer_Wattage",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "WasteWaterRatio",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "WaveMaker_FlowRate",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "WaveMaker_IntensityPercent",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "WaveMaker_PowerConsumption",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "WavePattern",
                table: "Equipment");
        }
    }
}
