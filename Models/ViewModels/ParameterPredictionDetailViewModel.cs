using System;
using System.Collections.Generic;

namespace AquaHub.MVC.Models.ViewModels
{
    public class ParameterPredictionDetailViewModel
    {
        public string ParameterName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int DaysAhead { get; set; }
        public double? CurrentValue { get; set; }
        public double? PredictedValue { get; set; }
        public double? Confidence { get; set; }
        public double? IdealMin { get; set; }
        public double? IdealMax { get; set; }
        public List<string> RecommendedActions { get; set; } = new();
        public List<string> TrendDates { get; set; } = new();
        public List<double> HistoricalValues { get; set; } = new();
        public List<double> PredictedValues { get; set; } = new();
    }
}
