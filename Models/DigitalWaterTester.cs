using System;

namespace AquaHub.MVC.Models;

public class DigitalWaterTester : Equipment
{
    public string ParametersMonitored { get; set; } = string.Empty; // pH, temp, salinity, etc.
    public bool HasWifiConnectivity { get; set; }
    public bool HasAlerts { get; set; }
    public DateTime LastCalibrationDate { get; set; }
    public int CalibrationIntervalDays { get; set; }
}
