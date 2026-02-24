namespace AquaHub.MVC.Models.Enums;

using System.ComponentModel.DataAnnotations;

public enum MaintenanceType
{
    [Display(Name = "Water Change")]
    WaterChange = 1,

    [Display(Name = "Filter Cleaning")]
    FilterCleaning = 2,

    [Display(Name = "Glass Cleaning")]
    GlassCleaning = 3,

    [Display(Name = "Substrate Vacuum")]
    SubstrateVacuum = 4,

    [Display(Name = "Equipment Maintenance")]
    EquipmentMaintenance = 5,

    [Display(Name = "Trim Plants")]
    TrimPlants = 6,

    [Display(Name = "Algae Scraping")]
    AlgaeScraping = 7,

    [Display(Name = "Water Testing")]
    Testing = 8,

    [Display(Name = "Dosing Chemicals")]
    Dosing = 9,

    [Display(Name = "Create Salt Water")]
    CreateSaltWater = 11,

    [Display(Name = "CO2 Cartridge Change")]
    CO2CartridgeChange = 12,

    [Display(Name = "Water Top Off")]
    WaterTopOff = 13,

    [Display(Name = "Weekly Maintenance (Water Change, Glass Cleaning, Dosing)")]
    WeeklyMaintenance = 14,

    [Display(Name = "Other")]
    Other = 10
}
