using System;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Models.ViewModels;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class EquipmentService : IEquipmentService
{
    private readonly ApplicationDbContext _context;

    public EquipmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Equipment>> GetAllEquipmentAsync(string userId)
    {
        var filters = await _context.Filters
            .Include(e => e.Tank)
            .Where(e => e.Tank!.UserId == userId)
            .ToListAsync();

        var lights = await _context.Lights
            .Include(e => e.Tank)
            .Where(e => e.Tank!.UserId == userId)
            .ToListAsync();

        var skimmers = await _context.ProteinSkimmers
            .Include(e => e.Tank)
            .Where(e => e.Tank!.UserId == userId)
            .ToListAsync();

        var allEquipment = new List<Equipment>();
        allEquipment.AddRange(filters);
        allEquipment.AddRange(lights);
        allEquipment.AddRange(skimmers);

        return allEquipment
            .OrderBy(e => e.Tank!.Name)
            .ThenBy(e => e.Brand)
            .ToList();
    }

    public async Task<List<Equipment>> GetEquipmentByTankAsync(int tankId, string userId)
    {
        var filters = await _context.Filters
            .Include(e => e.Tank)
            .Where(e => e.TankId == tankId && e.Tank!.UserId == userId)
            .ToListAsync();

        var lights = await _context.Lights
            .Include(e => e.Tank)
            .Where(e => e.TankId == tankId && e.Tank!.UserId == userId)
            .ToListAsync();

        var skimmers = await _context.ProteinSkimmers
            .Include(e => e.Tank)
            .Where(e => e.TankId == tankId && e.Tank!.UserId == userId)
            .ToListAsync();

        var allEquipment = new List<Equipment>();
        allEquipment.AddRange(filters);
        allEquipment.AddRange(lights);
        allEquipment.AddRange(skimmers);

        return allEquipment.OrderBy(e => e.Brand).ToList();
    }

    public async Task<Equipment?> GetEquipmentByIdAsync(int id, string userId)
    {
        // Try to find in Filters
        var filter = await _context.Filters
            .Include(e => e.Tank)
            .FirstOrDefaultAsync(e => e.Id == id && e.Tank!.UserId == userId);
        if (filter != null) return filter;

        // Try to find in Lights
        var light = await _context.Lights
            .Include(e => e.Tank)
            .FirstOrDefaultAsync(e => e.Id == id && e.Tank!.UserId == userId);
        if (light != null) return light;

        // Try to find in Protein Skimmers
        var skimmer = await _context.ProteinSkimmers
            .Include(e => e.Tank)
            .FirstOrDefaultAsync(e => e.Id == id && e.Tank!.UserId == userId);
        if (skimmer != null) return skimmer;

        return null;
    }

    public async Task<Equipment> CreateEquipmentAsync(Equipment equipment, int tankId, string userId)
    {
        // Verify tank ownership
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == tankId && t.UserId == userId);

        if (tank == null)
        {
            throw new UnauthorizedAccessException("You don't have permission to add equipment to this tank.");
        }

        equipment.TankId = tankId;

        // Add to the appropriate DbSet
        if (equipment is Filter filter)
        {
            _context.Filters.Add(filter);
        }
        else if (equipment is Light light)
        {
            _context.Lights.Add(light);
        }
        else if (equipment is ProteinSkimmer skimmer)
        {
            _context.ProteinSkimmers.Add(skimmer);
        }
        else
        {
            throw new InvalidOperationException("Unknown equipment type");
        }

        await _context.SaveChangesAsync();
        return equipment;
    }

    public async Task<Equipment> UpdateEquipmentAsync(Equipment equipment, string userId)
    {
        Equipment? existing = null;

        // Find the existing equipment in the appropriate DbSet
        if (equipment is Filter)
        {
            existing = await _context.Filters
                .Include(e => e.Tank)
                .FirstOrDefaultAsync(e => e.Id == equipment.Id && e.Tank!.UserId == userId);
        }
        else if (equipment is Light)
        {
            existing = await _context.Lights
                .Include(e => e.Tank)
                .FirstOrDefaultAsync(e => e.Id == equipment.Id && e.Tank!.UserId == userId);
        }
        else if (equipment is ProteinSkimmer)
        {
            existing = await _context.ProteinSkimmers
                .Include(e => e.Tank)
                .FirstOrDefaultAsync(e => e.Id == equipment.Id && e.Tank!.UserId == userId);
        }

        if (existing == null)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this equipment.");
        }

        existing.Brand = equipment.Brand;
        existing.Model = equipment.Model;
        existing.InstalledOn = equipment.InstalledOn;

        // Handle specific equipment types
        if (existing is Filter existingFilter && equipment is Filter newFilter)
        {
            existingFilter.Type = newFilter.Type;
            existingFilter.FlowRate = newFilter.FlowRate;
            existingFilter.Media = newFilter.Media;
        }
        else if (existing is Light existingLight && equipment is Light newLight)
        {
            existingLight.Wattage = newLight.Wattage;
            existingLight.Spectrum = newLight.Spectrum;
            existingLight.IsDimmable = newLight.IsDimmable;
            existingLight.IntensityPercent = newLight.IntensityPercent;
            existingLight.Schedule = newLight.Schedule;
        }
        else if (existing is ProteinSkimmer existingSkimmer && equipment is ProteinSkimmer newSkimmer)
        {
            existingSkimmer.Capacity = newSkimmer.Capacity;
            existingSkimmer.Type = newSkimmer.Type;
            existingSkimmer.AirIntake = newSkimmer.AirIntake;
            existingSkimmer.CupFillLevel = newSkimmer.CupFillLevel;
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteEquipmentAsync(int id, string userId)
    {
        // Try to find and delete from Filters
        var filter = await _context.Filters
            .Include(e => e.Tank)
            .FirstOrDefaultAsync(e => e.Id == id && e.Tank!.UserId == userId);
        if (filter != null)
        {
            _context.Filters.Remove(filter);
            await _context.SaveChangesAsync();
            return true;
        }

        // Try to find and delete from Lights
        var light = await _context.Lights
            .Include(e => e.Tank)
            .FirstOrDefaultAsync(e => e.Id == id && e.Tank!.UserId == userId);
        if (light != null)
        {
            _context.Lights.Remove(light);
            await _context.SaveChangesAsync();
            return true;
        }

        // Try to find and delete from Protein Skimmers
        var skimmer = await _context.ProteinSkimmers
            .Include(e => e.Tank)
            .FirstOrDefaultAsync(e => e.Id == id && e.Tank!.UserId == userId);
        if (skimmer != null)
        {
            _context.ProteinSkimmers.Remove(skimmer);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<EquipmentDashboardViewModel> GetEquipmentDashboardAsync(int equipmentId, string userId)
    {
        // Try to find equipment in all DbSets
        Equipment? equipment = await GetEquipmentByIdAsync(equipmentId, userId);

        if (equipment == null)
        {
            return new EquipmentDashboardViewModel();
        }

        // Load the tank with its related data
        var tank = await _context.Tanks
            .Include(t => t.MaintenanceLogs)
            .FirstOrDefaultAsync(t => t.Id == equipment.TankId);

        var viewModel = new EquipmentDashboardViewModel
        {
            Equipment = equipment,
            Tank = tank
        };

        // Calculate days since installation
        viewModel.DaysSinceInstallation = (DateTime.Now - equipment.InstalledOn).Days;

        // Get maintenance logs related to equipment
        viewModel.RelatedMaintenanceLogs = tank?.MaintenanceLogs
            .Where(m => m.Notes != null && m.Notes.Contains(equipment.Brand, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(m => m.Timestamp)
            .Take(10)
            .ToList() ?? new List<MaintenanceLog>();

        // Calculate maintenance recommendation based on type
        viewModel.RecommendedMaintenanceInterval = GetMaintenanceInterval(equipment);

        var lastMaintenance = viewModel.RelatedMaintenanceLogs.FirstOrDefault();
        if (lastMaintenance != null)
        {
            viewModel.DaysSinceLastMaintenance = (DateTime.Now - lastMaintenance.Timestamp).Days;
            viewModel.NeedsMaintenance = viewModel.DaysSinceLastMaintenance > viewModel.RecommendedMaintenanceInterval;
        }
        else
        {
            viewModel.DaysSinceLastMaintenance = viewModel.DaysSinceInstallation;
            viewModel.NeedsMaintenance = viewModel.DaysSinceLastMaintenance > 30; // Default
        }

        return viewModel;
    }

    private int GetMaintenanceInterval(Equipment equipment)
    {
        return equipment switch
        {
            Filter filter => filter.Type switch
            {
                FilterType.HangOnBack => 30,
                FilterType.Canister => 45,
                FilterType.Sump => 60,
                FilterType.Sponge => 14,
                FilterType.Undergravel => 90,
                _ => 30
            },
            Light => 180, // Lights need less frequent maintenance
            ProteinSkimmer => 7, // Protein skimmers need frequent cleaning
            _ => 30
        };
    }
}
