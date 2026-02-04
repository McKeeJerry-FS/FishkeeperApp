using System;

namespace AquaHub.MVC.Models;

public class AutoFeeder : Equipment
{
    public int FeedingsPerDay { get; set; }
    public string FeedingSchedule { get; set; } = string.Empty;
    public double PortionSize { get; set; }
    public double HopperCapacity { get; set; } // grams
    public DateTime LastRefillDate { get; set; }
}
