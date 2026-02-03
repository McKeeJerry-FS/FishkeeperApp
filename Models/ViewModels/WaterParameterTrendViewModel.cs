using System;

namespace AquaHub.MVC.Models.ViewModels;

public class WaterParameterTrendViewModel
{
    public string ParameterName { get; set; } = string.Empty;
    public List<DataPoint> DataPoints { get; set; } = new();
    public ParameterStatistics Statistics { get; set; } = new();
    public string Unit { get; set; } = string.Empty;
    public double? IdealMin { get; set; }
    public double? IdealMax { get; set; }
}

public class DataPoint
{
    public DateTime Date { get; set; }
    public double? Value { get; set; }
}

public class ParameterStatistics
{
    public double? CurrentValue { get; set; }
    public double? Average { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public double? Median { get; set; }
    public double? StandardDeviation { get; set; }
    public int TotalReadings { get; set; }
    public TrendDirection Trend { get; set; }
    public double? TrendPercentage { get; set; }
    public bool IsInIdealRange { get; set; }
    public int DaysAboveIdeal { get; set; }
    public int DaysBelowIdeal { get; set; }
}

public enum TrendDirection
{
    Stable,
    Rising,
    Falling,
    Fluctuating
}

public class WaterParameterTrendsViewModel
{
    public Tank? Tank { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<WaterParameterTrendViewModel> ParameterTrends { get; set; } = new();
    public int TotalTests { get; set; }
    public double AverageTestFrequency { get; set; } // days between tests
    public Dictionary<string, List<string>> Alerts { get; set; } = new();
}

