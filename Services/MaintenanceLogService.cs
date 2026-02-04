using System;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class MaintenanceLogService : IMaintenanceLogService
{
    private readonly ApplicationDbContext _context;
    private readonly ISupplyService _supplyService;

    public MaintenanceLogService(ApplicationDbContext context, ISupplyService supplyService)
    {
        _context = context;
        _supplyService = supplyService;
    }

    public async Task<List<MaintenanceLog>> GetAllMaintenanceLogsAsync(string userId)
    {
        return await _context.MaintenanceLogs
            .Include(m => m.Tank)
            .Include(m => m.SupplyItem)
            .Where(m => m.Tank!.UserId == userId)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<List<MaintenanceLog>> GetMaintenanceLogsByTankAsync(int tankId, string userId)
    {
        return await _context.MaintenanceLogs
            .Include(m => m.Tank)
            .Include(m => m.SupplyItem)
            .Where(m => m.TankId == tankId && m.Tank!.UserId == userId)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<MaintenanceLog?> GetMaintenanceLogByIdAsync(int id, string userId)
    {
        return await _context.MaintenanceLogs
            .Include(m => m.Tank)
            .Include(m => m.SupplyItem)
            .Where(m => m.Tank!.UserId == userId)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<MaintenanceLog> CreateMaintenanceLogAsync(MaintenanceLog maintenanceLog, int tankId, string userId, int? supplyItemId = null, double? amountUsed = null)
    {
        // Verify tank ownership
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == tankId && t.UserId == userId);

        if (tank == null)
        {
            throw new UnauthorizedAccessException("You don't have permission to add maintenance logs to this tank.");
        }

        maintenanceLog.TankId = tankId;
        maintenanceLog.Timestamp = DateTime.UtcNow;

        // Handle supply item usage if provided
        if (supplyItemId.HasValue && amountUsed.HasValue && amountUsed.Value > 0)
        {
            // Verify supply ownership
            var supply = await _context.SupplyItems
                .FirstOrDefaultAsync(s => s.Id == supplyItemId.Value && s.UserId == userId);

            if (supply == null)
            {
                throw new UnauthorizedAccessException("You don't have permission to use this supply item.");
            }

            // Deduct the amount from inventory
            var success = await _supplyService.RemoveQuantityAsync(supplyItemId.Value, userId, amountUsed.Value);

            if (success)
            {
                maintenanceLog.SupplyItemId = supplyItemId.Value;
                maintenanceLog.AmountUsed = amountUsed.Value;
            }
        }

        _context.MaintenanceLogs.Add(maintenanceLog);
        await _context.SaveChangesAsync();

        return maintenanceLog;
    }

    public async Task<MaintenanceLog> UpdateMaintenanceLogAsync(MaintenanceLog maintenanceLog, string userId)
    {
        // Verify ownership through tank
        var existingLog = await _context.MaintenanceLogs
            .Include(m => m.Tank)
            .FirstOrDefaultAsync(m => m.Id == maintenanceLog.Id && m.Tank!.UserId == userId);

        if (existingLog == null)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this maintenance log.");
        }

        // Update properties
        existingLog.Type = maintenanceLog.Type;
        existingLog.WaterChangePercent = maintenanceLog.WaterChangePercent;
        existingLog.Notes = maintenanceLog.Notes;

        // Note: We don't allow updating supply usage after creation to maintain inventory accuracy

        await _context.SaveChangesAsync();
        return existingLog;
    }

    public async Task<bool> DeleteMaintenanceLogAsync(int id, string userId)
    {
        var maintenanceLog = await _context.MaintenanceLogs
            .Include(m => m.Tank)
            .FirstOrDefaultAsync(m => m.Id == id && m.Tank!.UserId == userId);

        if (maintenanceLog == null) return false;

        // Note: Consider whether to restore supply quantities when deleting logs
        // For now, we won't automatically restore to prevent inventory manipulation

        _context.MaintenanceLogs.Remove(maintenanceLog);
        await _context.SaveChangesAsync();
        return true;
    }
}
