using AquaHub.MVC.Models;
using AquaHub.MVC.Models.ViewModels;

namespace AquaHub.MVC.Services.Interfaces;

public interface IBreedingWaterAnalysisService
{
    BreedingWaterConditionViewModel AnalyzeWaterConditions(BreedingPair breedingPair, WaterTest? latestWaterTest);
    List<WaterParameterStatus> GetParameterStatuses(BreedingPair breedingPair, WaterTest? waterTest);
    List<string> GenerateRecommendations(BreedingPair breedingPair, WaterTest? waterTest);
}
