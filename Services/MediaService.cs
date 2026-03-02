using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class MediaService : IMediaService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MediaService> _logger;
    private readonly string _mediaFolder = "uploads/media";

    public MediaService(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<MediaService> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    public async Task<MediaItem?> UploadMediaAsync(IFormFile file, string userId, string title,
        string? description = null, MediaCategory category = MediaCategory.General,
        int? tankId = null, int? journalEntryId = null, string? tags = null)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
                return null;

            // Determine media type
            var contentType = file.ContentType.ToLower();
            var mediaType = contentType.StartsWith("video/") ? MediaType.Video : MediaType.Image;

            // Create upload directory
            var uploadPath = Path.Combine(_environment.WebRootPath, _mediaFolder);
            Directory.CreateDirectory(uploadPath);

            // Generate unique filename
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create media item
            var mediaItem = new MediaItem
            {
                Title = title,
                Description = description,
                FilePath = $"/{_mediaFolder}/{uniqueFileName}",
                MediaType = mediaType,
                Category = category,
                FileSize = file.Length,
                ContentType = file.ContentType,
                UserId = userId,
                TankId = tankId,
                JournalEntryId = journalEntryId,
                Tags = tags,
                UploadedAt = DateTime.UtcNow
            };

            _context.MediaItems.Add(mediaItem);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Media uploaded: {Title} for user {UserId}", title, userId);
            return mediaItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading media");
            return null;
        }
    }

    public async Task<MediaItem?> GetMediaByIdAsync(int id, string userId)
    {
        return await _context.MediaItems
            .Include(m => m.Tank)
            .Include(m => m.JournalEntry)
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
    }

    public async Task<List<MediaItem>> GetAllMediaForUserAsync(string userId)
    {
        return await _context.MediaItems
            .Where(m => m.UserId == userId)
            .Include(m => m.Tank)
            .Include(m => m.JournalEntry)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<MediaItem>> GetMediaForTankAsync(int tankId, string userId)
    {
        return await _context.MediaItems
            .Where(m => m.TankId == tankId && m.UserId == userId)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<MediaItem>> GetMediaForJournalEntryAsync(int journalEntryId, string userId)
    {
        return await _context.MediaItems
            .Where(m => m.JournalEntryId == journalEntryId && m.UserId == userId)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<MediaItem>> GetMediaByCategoryAsync(string userId, MediaCategory category)
    {
        return await _context.MediaItems
            .Where(m => m.UserId == userId && m.Category == category)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<MediaItem>> GetFavoriteMediaAsync(string userId)
    {
        return await _context.MediaItems
            .Where(m => m.UserId == userId && m.IsFavorite)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<MediaItem>> GetRecentMediaAsync(string userId, int count = 10)
    {
        return await _context.MediaItems
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.UploadedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> UpdateMediaDetailsAsync(int mediaId, string userId, string? title = null,
        string? description = null, string? tags = null, MediaCategory? category = null)
    {
        var media = await _context.MediaItems.FirstOrDefaultAsync(m => m.Id == mediaId && m.UserId == userId);
        if (media == null) return false;

        if (title != null) media.Title = title;
        if (description != null) media.Description = description;
        if (tags != null) media.Tags = tags;
        if (category.HasValue) media.Category = category.Value;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleFavoriteAsync(int mediaId, string userId)
    {
        var media = await _context.MediaItems.FirstOrDefaultAsync(m => m.Id == mediaId && m.UserId == userId);
        if (media == null) return false;

        media.IsFavorite = !media.IsFavorite;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMediaAsync(int mediaId, string userId)
    {
        var media = await _context.MediaItems.FirstOrDefaultAsync(m => m.Id == mediaId && m.UserId == userId);
        if (media == null) return false;

        // Delete physical file
        var filePath = Path.Combine(_environment.WebRootPath, media.FilePath.TrimStart('/'));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.MediaItems.Remove(media);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<long> GetTotalStorageUsedAsync(string userId)
    {
        return await _context.MediaItems
            .Where(m => m.UserId == userId)
            .SumAsync(m => m.FileSize);
    }

    public async Task<Dictionary<MediaType, int>> GetMediaCountByTypeAsync(string userId)
    {
        return await _context.MediaItems
            .Where(m => m.UserId == userId)
            .GroupBy(m => m.MediaType)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }
}
