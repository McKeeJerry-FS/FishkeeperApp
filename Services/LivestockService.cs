using System;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.ViewModels;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class LivestockService : ILivestockService
{
    private readonly ApplicationDbContext _context;

    public LivestockService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Livestock>> GetAllLivestockAsync(string userId)
    {
        var corals = await _context.Corals
            .Include(l => l.Tank)
            .Where(l => l.Tank!.UserId == userId)
            .ToListAsync();

        var freshwaterFish = await _context.FreshwaterFish
            .Include(l => l.Tank)
            .Where(l => l.Tank!.UserId == userId)
            .ToListAsync();

        var saltwaterFish = await _context.SaltwaterFish
            .Include(l => l.Tank)
            .Where(l => l.Tank!.UserId == userId)
            .ToListAsync();

        var freshwaterInvertebrates = await _context.FreshwaterInvertebrates
            .Include(l => l.Tank)
            .Where(l => l.Tank!.UserId == userId)
            .ToListAsync();

        var saltwaterInvertebrates = await _context.SaltwaterInvertebrates
            .Include(l => l.Tank)
            .Where(l => l.Tank!.UserId == userId)
            .ToListAsync();

        var plants = await _context.Plants
            .Include(l => l.Tank)
            .Where(l => l.Tank!.UserId == userId)
            .ToListAsync();

        var allLivestock = new List<Livestock>();
        allLivestock.AddRange(corals);
        allLivestock.AddRange(freshwaterFish);
        allLivestock.AddRange(saltwaterFish);
        allLivestock.AddRange(freshwaterInvertebrates);
        allLivestock.AddRange(saltwaterInvertebrates);
        allLivestock.AddRange(plants);

        return allLivestock
            .OrderBy(l => l.Tank!.Name)
            .ThenBy(l => l.Name)
            .ToList();
    }

    public async Task<List<Livestock>> GetLivestockByTankAsync(int tankId, string userId)
    {
        var corals = await _context.Corals
            .Include(l => l.Tank)
            .Where(l => l.TankId == tankId && l.Tank!.UserId == userId)
            .ToListAsync();

        var freshwaterFish = await _context.FreshwaterFish
            .Include(l => l.Tank)
            .Where(l => l.TankId == tankId && l.Tank!.UserId == userId)
            .ToListAsync();

        var saltwaterFish = await _context.SaltwaterFish
            .Include(l => l.Tank)
            .Where(l => l.TankId == tankId && l.Tank!.UserId == userId)
            .ToListAsync();

        var freshwaterInvertebrates = await _context.FreshwaterInvertebrates
            .Include(l => l.Tank)
            .Where(l => l.TankId == tankId && l.Tank!.UserId == userId)
            .ToListAsync();

        var saltwaterInvertebrates = await _context.SaltwaterInvertebrates
            .Include(l => l.Tank)
            .Where(l => l.TankId == tankId && l.Tank!.UserId == userId)
            .ToListAsync();

        var plants = await _context.Plants
            .Include(l => l.Tank)
            .Where(l => l.TankId == tankId && l.Tank!.UserId == userId)
            .ToListAsync();

        var allLivestock = new List<Livestock>();
        allLivestock.AddRange(corals);
        allLivestock.AddRange(freshwaterFish);
        allLivestock.AddRange(saltwaterFish);
        allLivestock.AddRange(freshwaterInvertebrates);
        allLivestock.AddRange(saltwaterInvertebrates);
        allLivestock.AddRange(plants);

        return allLivestock.OrderBy(l => l.Name).ToList();
    }

    public async Task<Livestock?> GetLivestockByIdAsync(int id, string userId)
    {
        // Try each DbSet
        var coral = await _context.Corals
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == id && l.Tank!.UserId == userId);
        if (coral != null) return coral;

        var freshwaterFish = await _context.FreshwaterFish
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == id && l.Tank!.UserId == userId);
        if (freshwaterFish != null) return freshwaterFish;

        var saltwaterFish = await _context.SaltwaterFish
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == id && l.Tank!.UserId == userId);
        if (saltwaterFish != null) return saltwaterFish;

        var freshwaterInvert = await _context.FreshwaterInvertebrates
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == id && l.Tank!.UserId == userId);
        if (freshwaterInvert != null) return freshwaterInvert;

        var saltwaterInvert = await _context.SaltwaterInvertebrates
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == id && l.Tank!.UserId == userId);
        if (saltwaterInvert != null) return saltwaterInvert;

        var plant = await _context.Plants
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == id && l.Tank!.UserId == userId);
        if (plant != null) return plant;

        return null;
    }

    public async Task<Livestock> CreateLivestockAsync(Livestock livestock, int tankId, string userId)
    {
        // Verify tank ownership
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == tankId && t.UserId == userId);

        if (tank == null)
        {
            throw new UnauthorizedAccessException("You don't have permission to add livestock to this tank.");
        }

        livestock.TankId = tankId;

        // Add to the appropriate DbSet
        switch (livestock)
        {
            case Coral coral:
                _context.Corals.Add(coral);
                break;
            case SaltwaterFish saltwaterFish:
                _context.SaltwaterFish.Add(saltwaterFish);
                break;
            case FreshwaterFish freshwaterFish:
                _context.FreshwaterFish.Add(freshwaterFish);
                break;
            case SaltwaterInvertebrate saltwaterInvert:
                _context.SaltwaterInvertebrates.Add(saltwaterInvert);
                break;
            case FreshwaterInvertebrate freshwaterInvert:
                _context.FreshwaterInvertebrates.Add(freshwaterInvert);
                break;
            case Plant plant:
                _context.Plants.Add(plant);
                break;
            default:
                throw new InvalidOperationException("Unknown livestock type");
        }

        await _context.SaveChangesAsync();
        return livestock;
    }

    public async Task<Livestock> UpdateLivestockAsync(Livestock livestock, string userId)
    {
        Livestock? existing = null;

        // Find the existing livestock in the appropriate DbSet
        if (livestock is Coral)
        {
            existing = await _context.Corals
                .Include(l => l.Tank)
                .FirstOrDefaultAsync(l => l.Id == livestock.Id && l.Tank!.UserId == userId);
        }
        else if (livestock is SaltwaterFish)
        {
            existing = await _context.SaltwaterFish
                .Include(l => l.Tank)
                .FirstOrDefaultAsync(l => l.Id == livestock.Id && l.Tank!.UserId == userId);
        }
        else if (livestock is FreshwaterFish)
        {
            existing = await _context.FreshwaterFish
                .Include(l => l.Tank)
                .FirstOrDefaultAsync(l => l.Id == livestock.Id && l.Tank!.UserId == userId);
        }
        else if (livestock is SaltwaterInvertebrate)
        {
            existing = await _context.SaltwaterInvertebrates
                .Include(l => l.Tank)
                .FirstOrDefaultAsync(l => l.Id == livestock.Id && l.Tank!.UserId == userId);
        }
        else if (livestock is FreshwaterInvertebrate)
        {
            existing = await _context.FreshwaterInvertebrates
                .Include(l => l.Tank)
                .FirstOrDefaultAsync(l => l.Id == livestock.Id && l.Tank!.UserId == userId);
        }
        else if (livestock is Plant)
        {
            existing = await _context.Plants
                .Include(l => l.Tank)
                .FirstOrDefaultAsync(l => l.Id == livestock.Id && l.Tank!.UserId == userId);
        }

        if (existing == null)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this livestock.");
        }

        // Update common properties
        existing.Name = livestock.Name;
        existing.Species = livestock.Species;
        existing.AddedOn = livestock.AddedOn;
        existing.Notes = livestock.Notes;

        // Update type-specific properties
        UpdateTypeSpecificProperties(existing, livestock);

        await _context.SaveChangesAsync();
        return existing;
    }

    private void UpdateTypeSpecificProperties(Livestock existing, Livestock updated)
    {
        switch (existing)
        {
            case Coral existingCoral when updated is Coral newCoral:
                UpdateCoralProperties(existingCoral, newCoral);
                break;
            case SaltwaterFish existingSaltwaterFish when updated is SaltwaterFish newSaltwaterFish:
                UpdateSaltwaterFishProperties(existingSaltwaterFish, newSaltwaterFish);
                break;
            case FreshwaterFish existingFreshwaterFish when updated is FreshwaterFish newFreshwaterFish:
                UpdateFreshwaterFishProperties(existingFreshwaterFish, newFreshwaterFish);
                break;
            case SaltwaterInvertebrate existingSaltwaterInvert when updated is SaltwaterInvertebrate newSaltwaterInvert:
                UpdateSaltwaterInvertebrateProperties(existingSaltwaterInvert, newSaltwaterInvert);
                break;
            case FreshwaterInvertebrate existingFreshwaterInvert when updated is FreshwaterInvertebrate newFreshwaterInvert:
                UpdateFreshwaterInvertebrateProperties(existingFreshwaterInvert, newFreshwaterInvert);
                break;
            case Plant existingPlant when updated is Plant newPlant:
                UpdatePlantProperties(existingPlant, newPlant);
                break;
        }
    }

    private void UpdateCoralProperties(Coral existing, Coral updated)
    {
        existing.CoralType = updated.CoralType;
        existing.CoralFamily = updated.CoralFamily;
        existing.ScientificName = updated.ScientificName;
        existing.ColonySize = updated.ColonySize;
        existing.GrowthRate = updated.GrowthRate;
        existing.Coloration = updated.Coloration;
        existing.PolyExtension = updated.PolyExtension;
        existing.LightingNeeds = updated.LightingNeeds;
        existing.LightIntensityPAR = updated.LightIntensityPAR;
        existing.LightSpectrum = updated.LightSpectrum;
        existing.FlowNeeds = updated.FlowNeeds;
        existing.FlowType = updated.FlowType;
        existing.Placement = updated.Placement;
        existing.SpacingRequirement = updated.SpacingRequirement;
        existing.IsAggressive = updated.IsAggressive;
        existing.AggressionMethod = updated.AggressionMethod;
        existing.OptimalTemperatureMin = updated.OptimalTemperatureMin;
        existing.OptimalTemperatureMax = updated.OptimalTemperatureMax;
        existing.OptimalSalinityMin = updated.OptimalSalinityMin;
        existing.OptimalSalinityMax = updated.OptimalSalinityMax;
        existing.pHRange = updated.pHRange;
        existing.AlkalinityRange = updated.AlkalinityRange;
        existing.CalciumRange = updated.CalciumRange;
        existing.MagnesiumRange = updated.MagnesiumRange;
        existing.RequiresFeeding = updated.RequiresFeeding;
        existing.FoodTypes = updated.FoodTypes;
        existing.FeedingFrequency = updated.FeedingFrequency;
        existing.CareLevel = updated.CareLevel;
        existing.RequiresStableParameters = updated.RequiresStableParameters;
        existing.RequiresDosing = updated.RequiresDosing;
        existing.DosingRequirements = updated.DosingRequirements;
        existing.HasZooxanthellae = updated.HasZooxanthellae;
        existing.IsToxic = updated.IsToxic;
        existing.RequiresAcclimation = updated.RequiresAcclimation;
        existing.SpecialRequirements = updated.SpecialRequirements;
        existing.CanBeFragged = updated.CanBeFragged;
        existing.FraggingDifficulty = updated.FraggingDifficulty;
        existing.FraggingMethod = updated.FraggingMethod;
        existing.FraggingNotes = updated.FraggingNotes;
        existing.CommonDiseases = updated.CommonDiseases;
        existing.StressSigns = updated.StressSigns;
    }

    private void UpdateSaltwaterFishProperties(SaltwaterFish existing, SaltwaterFish updated)
    {
        existing.FishType = updated.FishType;
        existing.AdultSize = updated.AdultSize;
        existing.Coloration = updated.Coloration;
        existing.BodyShape = updated.BodyShape;
        existing.Temperament = updated.Temperament;
        existing.ActivityLevel = updated.ActivityLevel;
        existing.IsReefSafe = updated.IsReefSafe;
        existing.IsSchooling = updated.IsSchooling;
        existing.RecommendedSchoolSize = updated.RecommendedSchoolSize;
        existing.MinTankSize = updated.MinTankSize;
        existing.SwimmingRegion = updated.SwimmingRegion;
        existing.RequiresLiveRock = updated.RequiresLiveRock;
        existing.RequiresHidingSpots = updated.RequiresHidingSpots;
        existing.OptimalTemperatureMin = updated.OptimalTemperatureMin;
        existing.OptimalTemperatureMax = updated.OptimalTemperatureMax;
        existing.OptimalSalinityMin = updated.OptimalSalinityMin;
        existing.OptimalSalinityMax = updated.OptimalSalinityMax;
        existing.pHRange = updated.pHRange;
        existing.Diet = updated.Diet;
        existing.FoodTypes = updated.FoodTypes;
        existing.FeedingFrequency = updated.FeedingFrequency;
        existing.FeedingBehavior = updated.FeedingBehavior;
        existing.CareLevel = updated.CareLevel;
        existing.RequiresQuarantine = updated.RequiresQuarantine;
        existing.AggressiveToSameSpecies = updated.AggressiveToSameSpecies;
        existing.AggressiveToOtherFish = updated.AggressiveToOtherFish;
        existing.TankMateCompatibility = updated.TankMateCompatibility;
        existing.NipsAtCorals = updated.NipsAtCorals;
        existing.NipsAtInvertebrates = updated.NipsAtInvertebrates;
        existing.IsJumper = updated.IsJumper;
        existing.IsVenomous = updated.IsVenomous;
        existing.SpecialRequirements = updated.SpecialRequirements;
        existing.BreedingDifficulty = updated.BreedingDifficulty;
        existing.BreedingNotes = updated.BreedingNotes;
        existing.AverageLifespanYears = updated.AverageLifespanYears;
        existing.ProneToIch = updated.ProneToIch;
        existing.CommonDiseases = updated.CommonDiseases;
    }

    private void UpdateFreshwaterFishProperties(FreshwaterFish existing, FreshwaterFish updated)
    {
        existing.FishType = updated.FishType;
        existing.AdultSize = updated.AdultSize;
        existing.Coloration = updated.Coloration;
        existing.BodyShape = updated.BodyShape;
        existing.HasLongFins = updated.HasLongFins;
        existing.Temperament = updated.Temperament;
        existing.ActivityLevel = updated.ActivityLevel;
        existing.IsSchooling = updated.IsSchooling;
        existing.RecommendedSchoolSize = updated.RecommendedSchoolSize;
        existing.IsNocturnal = updated.IsNocturnal;
        existing.MinTankSize = updated.MinTankSize;
        existing.SwimmingRegion = updated.SwimmingRegion;
        existing.RequiresPlants = updated.RequiresPlants;
        existing.RequiresHidingSpots = updated.RequiresHidingSpots;
        existing.RequiresDriftwood = updated.RequiresDriftwood;
        existing.OptimalTemperatureMin = updated.OptimalTemperatureMin;
        existing.OptimalTemperatureMax = updated.OptimalTemperatureMax;
        existing.pHRange = updated.pHRange;
        existing.HardnessRange = updated.HardnessRange;
        existing.PreferredWaterFlow = updated.PreferredWaterFlow;
        existing.Diet = updated.Diet;
        existing.FoodTypes = updated.FoodTypes;
        existing.FeedingFrequency = updated.FeedingFrequency;
        existing.IsBottomFeeder = updated.IsBottomFeeder;
        existing.CareLevel = updated.CareLevel;
        existing.RequiresQuarantine = updated.RequiresQuarantine;
        existing.RequiresCycledTank = updated.RequiresCycledTank;
        existing.AggressiveToSameSpecies = updated.AggressiveToSameSpecies;
        existing.AggressiveToOtherFish = updated.AggressiveToOtherFish;
        existing.TankMateCompatibility = updated.TankMateCompatibility;
        existing.NipsAtFins = updated.NipsAtFins;
        existing.EatsSmallFish = updated.EatsSmallFish;
        existing.EatsShrimp = updated.EatsShrimp;
        existing.EatsSnails = updated.EatsSnails;
        existing.IsJumper = updated.IsJumper;
        existing.IsLabyrinthFish = updated.IsLabyrinthFish;
        existing.RequiresAirstone = updated.RequiresAirstone;
        existing.SpecialRequirements = updated.SpecialRequirements;
        existing.BreedingDifficulty = updated.BreedingDifficulty;
        existing.BreedingType = updated.BreedingType;
        existing.BreedingNotes = updated.BreedingNotes;
        existing.AverageLifespanYears = updated.AverageLifespanYears;
        existing.ProneToIch = updated.ProneToIch;
        existing.ProneToDropsy = updated.ProneToDropsy;
        existing.CommonDiseases = updated.CommonDiseases;
    }

    private void UpdateSaltwaterInvertebrateProperties(SaltwaterInvertebrate existing, SaltwaterInvertebrate updated)
    {
        existing.InvertebrateType = updated.InvertebrateType;
        existing.AdultSize = updated.AdultSize;
        existing.Coloration = updated.Coloration;
        existing.Habitat = updated.Habitat;
        existing.Behavior = updated.Behavior;
        existing.IsReefSafe = updated.IsReefSafe;
        existing.MinTankSize = updated.MinTankSize;
        existing.Placement = updated.Placement;
        existing.WaterParameters = updated.WaterParameters;
        existing.OptimalTemperatureMin = updated.OptimalTemperatureMin;
        existing.OptimalTemperatureMax = updated.OptimalTemperatureMax;
        existing.OptimalSalinityMin = updated.OptimalSalinityMin;
        existing.OptimalSalinityMax = updated.OptimalSalinityMax;
        existing.pHRange = updated.pHRange;
        existing.CareLevel = updated.CareLevel;
        existing.Diet = updated.Diet;
        existing.FeedingFrequency = updated.FeedingFrequency;
        existing.AggressiveTowardsOwnSpecies = updated.AggressiveTowardsOwnSpecies;
        existing.TankMateCompatibility = updated.TankMateCompatibility;
        existing.IsCleaner = updated.IsCleaner;
        existing.RequiresAcclimation = updated.RequiresAcclimation;
        existing.SpecialRequirements = updated.SpecialRequirements;
        existing.AverageLifespanYears = updated.AverageLifespanYears;
        existing.CommonDiseases = updated.CommonDiseases;
    }

    private void UpdateFreshwaterInvertebrateProperties(FreshwaterInvertebrate existing, FreshwaterInvertebrate updated)
    {
        existing.InvertebrateType = updated.InvertebrateType;
        existing.AdultSize = updated.AdultSize;
        existing.Coloration = updated.Coloration;
        existing.Habitat = updated.Habitat;
        existing.Behavior = updated.Behavior;
        existing.IsPlantSafe = updated.IsPlantSafe;
        existing.MinTankSize = updated.MinTankSize;
        existing.Placement = updated.Placement;
        existing.WaterParameters = updated.WaterParameters;
        existing.OptimalTemperatureMin = updated.OptimalTemperatureMin;
        existing.OptimalTemperatureMax = updated.OptimalTemperatureMax;
        existing.pHRange = updated.pHRange;
        existing.HardnessRange = updated.HardnessRange;
        existing.CareLevel = updated.CareLevel;
        existing.Diet = updated.Diet;
        existing.FeedingFrequency = updated.FeedingFrequency;
        existing.AggressiveTowardsOwnSpecies = updated.AggressiveTowardsOwnSpecies;
        existing.TankMateCompatibility = updated.TankMateCompatibility;
        existing.IsAlgaeEater = updated.IsAlgaeEater;
        existing.IsScavenger = updated.IsScavenger;
        existing.RequiresAcclimation = updated.RequiresAcclimation;
        existing.SensitiveToCopper = updated.SensitiveToCopper;
        existing.SpecialRequirements = updated.SpecialRequirements;
        existing.BreedingDifficulty = updated.BreedingDifficulty;
        existing.BreedingNotes = updated.BreedingNotes;
        existing.AverageLifespanYears = updated.AverageLifespanYears;
        existing.CommonDiseases = updated.CommonDiseases;
    }

    private void UpdatePlantProperties(Plant existing, Plant updated)
    {
        existing.LightingNeeds = updated.LightingNeeds;
        existing.Co2Needs = updated.Co2Needs;
        existing.GrowthRate = updated.GrowthRate;
    }

    public async Task<bool> DeleteLivestockAsync(int id, string userId)
    {
        // Try to find and delete from each DbSet
        var coral = await _context.Corals
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == id && l.Tank!.UserId == userId);
        if (coral != null)
        {
            _context.Corals.Remove(coral);
            await _context.SaveChangesAsync();
            return true;
        }

        var saltwaterFish = await _context.SaltwaterFish
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == id && l.Tank!.UserId == userId);
        if (saltwaterFish != null)
        {
            _context.SaltwaterFish.Remove(saltwaterFish);
            await _context.SaveChangesAsync();
            return true;
        }

        var freshwaterFish = await _context.FreshwaterFish
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == id && l.Tank!.UserId == userId);
        if (freshwaterFish != null)
        {
            _context.FreshwaterFish.Remove(freshwaterFish);
            await _context.SaveChangesAsync();
            return true;
        }

        var saltwaterInvert = await _context.SaltwaterInvertebrates
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == id && l.Tank!.UserId == userId);
        if (saltwaterInvert != null)
        {
            _context.SaltwaterInvertebrates.Remove(saltwaterInvert);
            await _context.SaveChangesAsync();
            return true;
        }

        var freshwaterInvert = await _context.FreshwaterInvertebrates
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == id && l.Tank!.UserId == userId);
        if (freshwaterInvert != null)
        {
            _context.FreshwaterInvertebrates.Remove(freshwaterInvert);
            await _context.SaveChangesAsync();
            return true;
        }

        var plant = await _context.Plants
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == id && l.Tank!.UserId == userId);
        if (plant != null)
        {
            _context.Plants.Remove(plant);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<LivestockDashboardViewModel> GetLivestockDashboardAsync(int livestockId, string userId)
    {
        // Try to find livestock in all DbSets
        Livestock? livestock = await GetLivestockByIdAsync(livestockId, userId);

        if (livestock == null)
        {
            return new LivestockDashboardViewModel();
        }

        // Load the tank with its related data
        var tank = await _context.Tanks
            .Include(t => t.WaterTests)
            .FirstOrDefaultAsync(t => t.Id == livestock.TankId);

        var viewModel = new LivestockDashboardViewModel
        {
            Livestock = livestock,
            Tank = tank
        };

        // Calculate days in tank
        viewModel.DaysInTank = (DateTime.Now - livestock.AddedOn).Days;

        // Get recent water tests
        viewModel.RecentWaterTests = tank?.WaterTests
            .OrderByDescending(w => w.Timestamp)
            .Take(5)
            .ToList() ?? new List<WaterTest>();

        // Get the most recent water test
        viewModel.MostRecentWaterTest = viewModel.RecentWaterTests.FirstOrDefault();

        return viewModel;
    }
}
