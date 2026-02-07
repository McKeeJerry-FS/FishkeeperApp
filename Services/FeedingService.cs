using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class FeedingService : BaseService, IFeedingService
{
    private readonly ILogger<FeedingService> _logger;

    public FeedingService(ApplicationDbContext context, ILogger<FeedingService> logger)
        : base(context)
    {
        _logger = logger;
    }

    #region Feeding Schedule Methods

    public async Task<IEnumerable<FeedingSchedule>> GetSchedulesForTankAsync(int tankId)
    {
        try
        {
            return await Context.FeedingSchedules
                .Include(fs => fs.Tank)
                .Where(fs => fs.TankId == tankId)
                .OrderBy(fs => fs.FeedName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feeding schedules for tank {TankId}", tankId);
            return Enumerable.Empty<FeedingSchedule>();
        }
    }

    public async Task<IEnumerable<FeedingSchedule>> GetSchedulesForUserAsync(string userId)
    {
        try
        {
            return await Context.FeedingSchedules
                .Include(fs => fs.Tank)
                .Where(fs => fs.Tank!.UserId == userId)
                .OrderBy(fs => fs.Tank!.Name)
                .ThenBy(fs => fs.FeedName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feeding schedules for user {UserId}", userId);
            return Enumerable.Empty<FeedingSchedule>();
        }
    }

    public async Task<FeedingSchedule?> GetScheduleByIdAsync(int id, string userId)
    {
        try
        {
            return await Context.FeedingSchedules
                .Include(fs => fs.Tank)
                .Include(fs => fs.FeedingRecords)
                .FirstOrDefaultAsync(fs => fs.Id == id && fs.Tank!.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feeding schedule {ScheduleId}", id);
            return null;
        }
    }

    public async Task<bool> CreateScheduleAsync(FeedingSchedule schedule, string userId)
    {
        try
        {
            // Verify the tank belongs to the user
            var tankExists = await Context.Tanks
                .AnyAsync(t => t.Id == schedule.TankId && t.UserId == userId);

            if (!tankExists)
            {
                _logger.LogWarning("Tank {TankId} not found or doesn't belong to user {UserId}", schedule.TankId, userId);
                return false;
            }

            schedule.CreatedDate = DateTime.UtcNow;
            Context.FeedingSchedules.Add(schedule);
            await Context.SaveChangesAsync();

            _logger.LogInformation("Created feeding schedule {ScheduleId} for tank {TankId}", schedule.Id, schedule.TankId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feeding schedule for tank {TankId}", schedule.TankId);
            return false;
        }
    }

    public async Task<bool> UpdateScheduleAsync(FeedingSchedule schedule, string userId)
    {
        try
        {
            var existingSchedule = await GetScheduleByIdAsync(schedule.Id, userId);
            if (existingSchedule == null)
            {
                _logger.LogWarning("Feeding schedule {ScheduleId} not found", schedule.Id);
                return false;
            }

            existingSchedule.FeedType = schedule.FeedType;
            existingSchedule.FeedName = schedule.FeedName;
            existingSchedule.Brand = schedule.Brand;
            existingSchedule.Amount = schedule.Amount;
            existingSchedule.Unit = schedule.Unit;
            existingSchedule.FeedingTimes = schedule.FeedingTimes;
            existingSchedule.IsActive = schedule.IsActive;
            existingSchedule.Notes = schedule.Notes;
            existingSchedule.LastModified = DateTime.UtcNow;

            await Context.SaveChangesAsync();

            _logger.LogInformation("Updated feeding schedule {ScheduleId}", schedule.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feeding schedule {ScheduleId}", schedule.Id);
            return false;
        }
    }

    public async Task<bool> DeleteScheduleAsync(int id, string userId)
    {
        try
        {
            var schedule = await GetScheduleByIdAsync(id, userId);
            if (schedule == null)
            {
                _logger.LogWarning("Feeding schedule {ScheduleId} not found", id);
                return false;
            }

            Context.FeedingSchedules.Remove(schedule);
            await Context.SaveChangesAsync();

            _logger.LogInformation("Deleted feeding schedule {ScheduleId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feeding schedule {ScheduleId}", id);
            return false;
        }
    }

    public async Task<bool> ToggleScheduleActiveAsync(int id, string userId)
    {
        try
        {
            var schedule = await GetScheduleByIdAsync(id, userId);
            if (schedule == null)
            {
                _logger.LogWarning("Feeding schedule {ScheduleId} not found", id);
                return false;
            }

            schedule.IsActive = !schedule.IsActive;
            schedule.LastModified = DateTime.UtcNow;
            await Context.SaveChangesAsync();

            _logger.LogInformation("Toggled feeding schedule {ScheduleId} active status to {IsActive}", id, schedule.IsActive);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling feeding schedule {ScheduleId}", id);
            return false;
        }
    }

    #endregion

    #region Feeding Record Methods

    public async Task<IEnumerable<FeedingRecord>> GetRecordsForTankAsync(int tankId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = Context.FeedingRecords
                .Include(fr => fr.Tank)
                .Include(fr => fr.FeedingSchedule)
                .Include(fr => fr.User)
                .Where(fr => fr.TankId == tankId);

            if (startDate.HasValue)
                query = query.Where(fr => fr.FedDateTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(fr => fr.FedDateTime <= endDate.Value);

            return await query
                .OrderByDescending(fr => fr.FedDateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feeding records for tank {TankId}", tankId);
            return Enumerable.Empty<FeedingRecord>();
        }
    }

    public async Task<IEnumerable<FeedingRecord>> GetRecordsForUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = Context.FeedingRecords
                .Include(fr => fr.Tank)
                .Include(fr => fr.FeedingSchedule)
                .Include(fr => fr.User)
                .Where(fr => fr.Tank!.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(fr => fr.FedDateTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(fr => fr.FedDateTime <= endDate.Value);

            return await query
                .OrderByDescending(fr => fr.FedDateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feeding records for user {UserId}", userId);
            return Enumerable.Empty<FeedingRecord>();
        }
    }

    public async Task<FeedingRecord?> GetRecordByIdAsync(int id, string userId)
    {
        try
        {
            return await Context.FeedingRecords
                .Include(fr => fr.Tank)
                .Include(fr => fr.FeedingSchedule)
                .Include(fr => fr.User)
                .FirstOrDefaultAsync(fr => fr.Id == id && fr.Tank!.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feeding record {RecordId}", id);
            return null;
        }
    }

    public async Task<bool> CreateRecordAsync(FeedingRecord record, string userId)
    {
        try
        {
            // Verify the tank belongs to the user
            var tankExists = await Context.Tanks
                .AnyAsync(t => t.Id == record.TankId && t.UserId == userId);

            if (!tankExists)
            {
                _logger.LogWarning("Tank {TankId} not found or doesn't belong to user {UserId}", record.TankId, userId);
                return false;
            }

            record.UserId = userId;
            record.CreatedDate = DateTime.UtcNow;
            Context.FeedingRecords.Add(record);
            await Context.SaveChangesAsync();

            _logger.LogInformation("Created feeding record {RecordId} for tank {TankId}", record.Id, record.TankId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feeding record for tank {TankId}", record.TankId);
            return false;
        }
    }

    public async Task<bool> UpdateRecordAsync(FeedingRecord record, string userId)
    {
        try
        {
            var existingRecord = await GetRecordByIdAsync(record.Id, userId);
            if (existingRecord == null)
            {
                _logger.LogWarning("Feeding record {RecordId} not found", record.Id);
                return false;
            }

            existingRecord.FeedType = record.FeedType;
            existingRecord.FeedName = record.FeedName;
            existingRecord.Brand = record.Brand;
            existingRecord.Amount = record.Amount;
            existingRecord.Unit = record.Unit;
            existingRecord.FedDateTime = record.FedDateTime;
            existingRecord.Notes = record.Notes;

            await Context.SaveChangesAsync();

            _logger.LogInformation("Updated feeding record {RecordId}", record.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feeding record {RecordId}", record.Id);
            return false;
        }
    }

    public async Task<bool> DeleteRecordAsync(int id, string userId)
    {
        try
        {
            var record = await GetRecordByIdAsync(id, userId);
            if (record == null)
            {
                _logger.LogWarning("Feeding record {RecordId} not found", id);
                return false;
            }

            Context.FeedingRecords.Remove(record);
            await Context.SaveChangesAsync();

            _logger.LogInformation("Deleted feeding record {RecordId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feeding record {RecordId}", id);
            return false;
        }
    }

    #endregion

    #region Helper Methods

    public async Task<IEnumerable<FeedingSchedule>> GetActiveSchedulesForTankAsync(int tankId)
    {
        try
        {
            return await Context.FeedingSchedules
                .Include(fs => fs.Tank)
                .Where(fs => fs.TankId == tankId && fs.IsActive)
                .OrderBy(fs => fs.FeedName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active feeding schedules for tank {TankId}", tankId);
            return Enumerable.Empty<FeedingSchedule>();
        }
    }

    public async Task<IEnumerable<FeedingRecord>> GetTodaysFeedingsForTankAsync(int tankId)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            return await Context.FeedingRecords
                .Include(fr => fr.Tank)
                .Include(fr => fr.FeedingSchedule)
                .Where(fr => fr.TankId == tankId && fr.FedDateTime >= today && fr.FedDateTime < tomorrow)
                .OrderBy(fr => fr.FedDateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's feedings for tank {TankId}", tankId);
            return Enumerable.Empty<FeedingRecord>();
        }
    }

    public async Task<Dictionary<int, List<TimeSpan>>> GetUpcomingFeedingsForUserAsync(string userId)
    {
        try
        {
            var schedules = await Context.FeedingSchedules
                .Include(fs => fs.Tank)
                .Where(fs => fs.Tank!.UserId == userId && fs.IsActive)
                .ToListAsync();

            var result = new Dictionary<int, List<TimeSpan>>();
            var now = DateTime.UtcNow.TimeOfDay;

            foreach (var schedule in schedules)
            {
                var upcomingTimes = schedule.ParsedFeedingTimes
                    .Where(t => t > now)
                    .ToList();

                if (upcomingTimes.Any())
                {
                    if (!result.ContainsKey(schedule.TankId))
                        result[schedule.TankId] = new List<TimeSpan>();

                    result[schedule.TankId].AddRange(upcomingTimes);
                }
            }

            // Sort the times for each tank
            foreach (var key in result.Keys)
            {
                result[key] = result[key].OrderBy(t => t).ToList();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming feedings for user {UserId}", userId);
            return new Dictionary<int, List<TimeSpan>>();
        }
    }

    public async Task<Dictionary<int, int>> GetFeedingStreaksForUserAsync(string userId)
    {
        try
        {
            var tanks = await Context.Tanks
                .Where(t => t.UserId == userId)
                .Select(t => t.Id)
                .ToListAsync();

            var result = new Dictionary<int, int>();
            var today = DateTime.UtcNow.Date;

            foreach (var tankId in tanks)
            {
                var streak = 0;
                var currentDate = today;

                // Check backwards day by day
                while (true)
                {
                    var nextDay = currentDate.AddDays(1);
                    var hasFeedingThisDay = await Context.FeedingRecords
                        .AnyAsync(fr => fr.TankId == tankId &&
                                      fr.FedDateTime >= currentDate &&
                                      fr.FedDateTime < nextDay);

                    if (!hasFeedingThisDay)
                        break;

                    streak++;
                    currentDate = currentDate.AddDays(-1);

                    // Prevent infinite loops
                    if (streak > 365)
                        break;
                }

                result[tankId] = streak;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feeding streaks for user {UserId}", userId);
            return new Dictionary<int, int>();
        }
    }

    public async Task<bool> RecordScheduledFeedingAsync(int scheduleId, TimeSpan scheduledTime, string userId, string? notes = null)
    {
        try
        {
            var schedule = await GetScheduleByIdAsync(scheduleId, userId);
            if (schedule == null)
            {
                _logger.LogWarning("Feeding schedule {ScheduleId} not found", scheduleId);
                return false;
            }

            var record = new FeedingRecord
            {
                TankId = schedule.TankId,
                FeedingScheduleId = scheduleId,
                FeedType = schedule.FeedType,
                FeedName = schedule.FeedName,
                Brand = schedule.Brand,
                Amount = schedule.Amount,
                Unit = schedule.Unit,
                FedDateTime = DateTime.UtcNow,
                WasScheduled = true,
                ScheduledTime = scheduledTime,
                Notes = notes,
                UserId = userId
            };

            return await CreateRecordAsync(record, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording scheduled feeding for schedule {ScheduleId}", scheduleId);
            return false;
        }
    }

    #endregion
}
