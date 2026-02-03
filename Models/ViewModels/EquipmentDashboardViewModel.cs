using System;

namespace AquaHub.MVC.Models.ViewModels;

public class EquipmentDashboardViewModel
{
public Equipment? Equipment { get; set; }
    public Tank? Tank { get; set; }
    public int DaysSinceInstallation { get; set; }
    public int DaysSinceLastMaintenance { get; set; }
    public int RecommendedMaintenanceInterval { get; set; }
    public bool NeedsMaintenance { get; set; }
    public List<MaintenanceLog> RelatedMaintenanceLogs { get; set; } = new();

    public string EquipmentTypeName
    {
        get
        {
            return Equipment switch
            {
                Filter => "Filter",
                Light => "Light",
                ProteinSkimmer => "Protein Skimmer",
                _ => "Equipment"
            };
        }
    }

    public string MaintenanceStatus
    {
        get
        {
            if (NeedsMaintenance)
                return "Needs Maintenance";

            var daysUntilMaintenance = RecommendedMaintenanceInterval - DaysSinceLastMaintenance;
            if (daysUntilMaintenance <= 7)
                return $"Maintenance Due Soon ({daysUntilMaintenance} days)";

            return "Up to Date";
        }
    }

    public string MaintenanceStatusClass
    {
        get
        {
            if (NeedsMaintenance)
                return "danger";

            var daysUntilMaintenance = RecommendedMaintenanceInterval - DaysSinceLastMaintenance;
            if (daysUntilMaintenance <= 7)
                return "warning";

            return "success";
        }
    }
}
