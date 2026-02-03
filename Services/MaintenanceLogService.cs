using System;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class MaintenanceLogService : IMaintenanceLogService
{
    private readonly ApplicationDbContext _context;

    public MaintenanceLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MaintenanceLog>> GetAllMaintenanceLogsAsync(string userId)
    {
        return await _context.MaintenanceLogs
            .Include(m => m.Tank)
            .Where(m => m.Tank!.UserId == userId)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<List<MaintenanceLog>> GetMaintenanceLogsByTankAsync(int tankId, string userId)
    {
        return await _context.MaintenanceLogs
            .Include(m => m.Tank)
            .Where(m => m.TankId == tankId && m.Tank!.UserId == userId)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<MaintenanceLog?> GetMaintenanceLogByIdAsync(int id, string userId)
    {
        return await _context.MaintenanceLogs
            .Include(m => m.Tank)
            .Where(m => m.Tank!.UserId == userId)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<MaintenanceLog> CreateMaintenanceLogAsync(MaintenanceLog maintenanceLog, int tankId, string userId)
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

        await _context.SaveChangesAsync();
        return existingLog;
    }

    public async Task<bool> DeleteMaintenanceLogAsync(int id, string userId)
    {
        var maintenanceLog = await _context.MaintenanceLogs
            .Include(m => m.Tank)
            .FirstOrDefaultAsync(m => m.Id == id && m.Tank!.UserId == userId);

        if (maintenanceLog == null) return false;

        _context.MaintenanceLogs.Remove(maintenanceLog);
        await _context.SaveChangesAsync();
        return true;
    }
}
