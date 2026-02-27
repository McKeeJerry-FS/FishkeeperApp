using System.Collections.Generic;
using System.Threading.Tasks;
using AquaHub.MVC.Models;

namespace AquaHub.MVC.Services.Interfaces;

public interface ITankMilestoneService
{
    Task<List<TankMilestone>> GetMilestonesForTankAsync(int tankId);
    Task<TankMilestone?> GetMilestoneByIdAsync(int milestoneId);
    Task<TankMilestone> AddMilestoneAsync(TankMilestone milestone);
    Task<TankMilestone> UpdateMilestoneAsync(TankMilestone milestone);
    Task<bool> DeleteMilestoneAsync(int milestoneId);
    Task<List<TankMilestone>> GenerateDefaultMilestonesAsync(Tank tank);
}
