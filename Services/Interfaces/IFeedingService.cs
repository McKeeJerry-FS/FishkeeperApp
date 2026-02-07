using AquaHub.MVC.Models;

namespace AquaHub.MVC.Services.Interfaces;

public interface IFeedingService
{
    // Feeding Schedule methods
    Task<IEnumerable<FeedingSchedule>> GetSchedulesForTankAsync(int tankId);
    Task<IEnumerable<FeedingSchedule>> GetSchedulesForUserAsync(string userId);
    Task<FeedingSchedule?> GetScheduleByIdAsync(int id, string userId);
    Task<bool> CreateScheduleAsync(FeedingSchedule schedule, string userId);
    Task<bool> UpdateScheduleAsync(FeedingSchedule schedule, string userId);
    Task<bool> DeleteScheduleAsync(int id, string userId);
    Task<bool> ToggleScheduleActiveAsync(int id, string userId);

    // Feeding Record methods
    Task<IEnumerable<FeedingRecord>> GetRecordsForTankAsync(int tankId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<FeedingRecord>> GetRecordsForUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<FeedingRecord?> GetRecordByIdAsync(int id, string userId);
    Task<bool> CreateRecordAsync(FeedingRecord record, string userId);
    Task<bool> UpdateRecordAsync(FeedingRecord record, string userId);
    Task<bool> DeleteRecordAsync(int id, string userId);

    // Helper methods
    Task<IEnumerable<FeedingSchedule>> GetActiveSchedulesForTankAsync(int tankId);
    Task<IEnumerable<FeedingRecord>> GetTodaysFeedingsForTankAsync(int tankId);
    Task<Dictionary<int, List<TimeSpan>>> GetUpcomingFeedingsForUserAsync(string userId);
    Task<Dictionary<int, int>> GetFeedingStreaksForUserAsync(string userId);
    Task<bool> RecordScheduledFeedingAsync(int scheduleId, TimeSpan scheduledTime, string userId, string? notes = null);
}
