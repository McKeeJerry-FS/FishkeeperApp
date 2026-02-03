namespace AquaHub.MVC.Models.Enums;

public enum WaterParameter
{
    PH = 1,
    Temperature = 2,
    Ammonia = 3,
    Nitrite = 4,
    Nitrate = 5,
    GH = 6,          // General Hardness (Freshwater)
    KH = 7,          // Carbonate Hardness (Freshwater)
    TDS = 8,         // Total Dissolved Solids (Freshwater)
    Salinity = 9,    // Saltwater/Reef
    Alkalinity = 10,  // Reef
    Calcium = 11,     // Reef
    Magnesium = 12,   // Reef
    Phosphate = 13,    // Reef
    Oxygen = 14,       // Oxygen levels
    CO2 = 15,          // Carbon Dioxide levels
    Other = 99
}
