using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using Microsoft.EntityFrameworkCore;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Services;

public class TankMilestoneService : ITankMilestoneService
{
    private readonly ApplicationDbContext _context;
    public TankMilestoneService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TankMilestone>> GetMilestonesForTankAsync(int tankId)
    {
        return await _context.Set<TankMilestone>()
            .Where(m => m.TankId == tankId)
            .OrderBy(m => m.TargetDate)
            .ToListAsync();
    }

    public async Task<TankMilestone?> GetMilestoneByIdAsync(int milestoneId)
    {
        return await _context.Set<TankMilestone>().FindAsync(milestoneId);
    }

    public async Task<TankMilestone> AddMilestoneAsync(TankMilestone milestone)
    {
        _context.Set<TankMilestone>().Add(milestone);
        await _context.SaveChangesAsync();
        return milestone;
    }

    public async Task<TankMilestone> UpdateMilestoneAsync(TankMilestone milestone)
    {
        _context.Set<TankMilestone>().Update(milestone);
        await _context.SaveChangesAsync();
        return milestone;
    }

    public async Task<bool> DeleteMilestoneAsync(int milestoneId)
    {
        var milestone = await _context.Set<TankMilestone>().FindAsync(milestoneId);
        if (milestone == null) return false;
        _context.Set<TankMilestone>().Remove(milestone);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<TankMilestone>> GenerateDefaultMilestonesAsync(Tank tank)
    {
        var milestones = new List<TankMilestone>();
        if (tank.IsNewTank)
        {
            if (tank.Type == Models.Enums.AquariumType.Freshwater || tank.Type == Models.Enums.AquariumType.Planted)
            {
                milestones.Add(new TankMilestone { TankId = tank.Id, Type = TankMilestoneType.AddPlants, Description = "Add plants when ammonia/nitrite are 0", TargetDate = tank.StartDate.AddDays(7) });
                milestones.Add(new TankMilestone { TankId = tank.Id, Type = TankMilestoneType.AddFirstFish, Description = "Add first fish when cycle is complete", TargetDate = tank.StartDate.AddDays(21) });
            }
            else if (tank.Type == Models.Enums.AquariumType.Saltwater || tank.Type == Models.Enums.AquariumType.Reef || tank.Type == Models.Enums.AquariumType.NanoReef)
            {
                milestones.Add(new TankMilestone { TankId = tank.Id, Type = TankMilestoneType.AddCorals, Description = "Add corals when cycle is complete and parameters stable", TargetDate = tank.StartDate.AddDays(30) });
                milestones.Add(new TankMilestone { TankId = tank.Id, Type = TankMilestoneType.AddCleanupCrew, Description = "Add cleanup crew after ammonia/nitrite are 0", TargetDate = tank.StartDate.AddDays(14) });
                milestones.Add(new TankMilestone { TankId = tank.Id, Type = TankMilestoneType.AddFirstFish, Description = "Add first fish after cycle completes", TargetDate = tank.StartDate.AddDays(35) });
            }
        }
        foreach (var m in milestones)
        {
            _context.Set<TankMilestone>().Add(m);
        }
        await _context.SaveChangesAsync();
        return milestones;
    }
}
