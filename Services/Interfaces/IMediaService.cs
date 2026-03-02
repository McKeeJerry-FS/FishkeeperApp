using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Services.Interfaces;

public interface IMediaService
{
    Task<MediaItem?> UploadMediaAsync(IFormFile file, string userId, string title, string? description = null,
        MediaCategory category = MediaCategory.General, int? tankId = null, int? journalEntryId = null, string? tags = null);

    Task<MediaItem?> GetMediaByIdAsync(int id, string userId);
    Task<List<MediaItem>> GetAllMediaForUserAsync(string userId);
    Task<List<MediaItem>> GetMediaForTankAsync(int tankId, string userId);
    Task<List<MediaItem>> GetMediaForJournalEntryAsync(int journalEntryId, string userId);
    Task<List<MediaItem>> GetMediaByCategoryAsync(string userId, MediaCategory category);
    Task<List<MediaItem>> GetFavoriteMediaAsync(string userId);
    Task<List<MediaItem>> GetRecentMediaAsync(string userId, int count = 10);

    Task<bool> UpdateMediaDetailsAsync(int mediaId, string userId, string? title = null,
        string? description = null, string? tags = null, MediaCategory? category = null);
    Task<bool> ToggleFavoriteAsync(int mediaId, string userId);
    Task<bool> DeleteMediaAsync(int mediaId, string userId);

    Task<long> GetTotalStorageUsedAsync(string userId);
    Task<Dictionary<MediaType, int>> GetMediaCountByTypeAsync(string userId);
}