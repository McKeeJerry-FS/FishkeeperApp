using System;

namespace AquaHub.MVC.Models.ViewModels;

public class LivestockDashboardViewModel
{
    public Livestock? Livestock { get; set; }
    public Tank? Tank { get; set; }
    public int DaysInTank { get; set; }
    public List<WaterTest> RecentWaterTests { get; set; } = new();
    public WaterTest? MostRecentWaterTest { get; set; }

    public string LivestockTypeName
    {
        get
        {
            return Livestock switch
            {
                Coral => "Coral",
                FreshwaterFish => "Fish",
                Plant => "Plant",
                _ => "Livestock"
            };
        }
    }

    public string HealthStatus
    {
        get
        {
            // Simple health status based on days in tank
            if (DaysInTank < 7)
                return "Acclimating";
            else if (DaysInTank < 30)
                return "Establishing";
            else
                return "Established";
        }
    }

    public string HealthStatusClass
    {
        get
        {
            return HealthStatus switch
            {
                "Acclimating" => "warning",
                "Establishing" => "info",
                "Established" => "success",
                _ => "secondary"
            };
        }
    }
}
