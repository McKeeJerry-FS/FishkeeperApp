using System;

namespace AquaHub.MVC.Models;

public class DosingPump : Equipment
{
    public int NumberOfChannels { get; set; }
    public string SolutionType { get; set; } = string.Empty;
    public double DoseAmount { get; set; } // ml per dose
    public string DosingSchedule { get; set; } = string.Empty;
    public DateTime LastRefillDate { get; set; }
}
