using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class MediaController : Controller
{
    private readonly IMediaService _mediaService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<MediaController> _logger;

    public MediaController(IMediaService mediaService, ApplicationDbContext context,
        UserManager<AppUser> userManager, ILogger<MediaController> logger)
    {
        _mediaService = mediaService;
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Media
    public async Task<IActionResult> Index(int? tankId, MediaCategory? category)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        List<MediaItem> mediaItems;

        if (tankId.HasValue)
        {
            mediaItems = await _mediaService.GetMediaForTankAsync(tankId.Value, userId);
            var tank = await _context.Tanks.FindAsync(tankId.Value);
            ViewBag.TankName = tank?.Name;
        }
        else if (category.HasValue)
        {
            mediaItems = await _mediaService.GetMediaByCategoryAsync(userId, category.Value);
            ViewBag.CategoryFilter = category.Value;
        }
        else
        {
            mediaItems = await _mediaService.GetAllMediaForUserAsync(userId);
        }

        // Get statistics
        ViewBag.TotalStorage = await _mediaService.GetTotalStorageUsedAsync(userId);
        ViewBag.MediaCounts = await _mediaService.GetMediaCountByTypeAsync(userId);

        return View(mediaItems);
    }

    // GET: Media/Upload
    public async Task<IActionResult> Upload(int? tankId, int? journalEntryId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var tanks = await _context.Tanks
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync();

        ViewBag.TankId = new SelectList(tanks, "Id", "Name", tankId);
        ViewBag.Categories = new SelectList(Enum.GetValues(typeof(MediaCategory)));

        ViewBag.PreselectedTankId = tankId;
        ViewBag.PreselectedJournalEntryId = journalEntryId;

        return View();
    }

    // POST: Media/Upload
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file, string title, string? description,
        MediaCategory category, int? tankId, int? journalEntryId, string? tags)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Please select a file to upload.");
            return View();
        }

        var mediaItem = await _mediaService.UploadMediaAsync(file, userId, title, description,
            category, tankId, journalEntryId, tags);

        if (mediaItem == null)
        {
            TempData["ErrorMessage"] = "Failed to upload media.";
            return View();
        }

        TempData["SuccessMessage"] = "Media uploaded successfully!";

        if (tankId.HasValue)
            return RedirectToAction(nameof(Index), new { tankId });

        return RedirectToAction(nameof(Index));
    }

    // GET: Media/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var mediaItem = await _mediaService.GetMediaByIdAsync(id, userId);
        if (mediaItem == null)
            return NotFound();

        return View(mediaItem);
    }

    // POST: Media/ToggleFavorite/5
    [HttpPost]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _mediaService.ToggleFavoriteAsync(id, userId);
        return Json(new { success });
    }

    // GET: Media/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var mediaItem = await _mediaService.GetMediaByIdAsync(id, userId);
        if (mediaItem == null)
            return NotFound();

        ViewBag.Categories = new SelectList(Enum.GetValues(typeof(MediaCategory)), mediaItem.Category);
        return View(mediaItem);
    }

    // POST: Media/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string title, string? description,
        MediaCategory category, string? tags)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _mediaService.UpdateMediaDetailsAsync(id, userId, title, description, tags, category);

        if (!success)
        {
            TempData["ErrorMessage"] = "Failed to update media.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        TempData["SuccessMessage"] = "Media updated successfully!";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Media/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _mediaService.DeleteMediaAsync(id, userId);

        if (!success)
        {
            TempData["ErrorMessage"] = "Failed to delete media.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Media deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Media/Gallery
    public async Task<IActionResult> Gallery(int? tankId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        List<MediaItem> mediaItems;

        if (tankId.HasValue)
        {
            mediaItems = await _mediaService.GetMediaForTankAsync(tankId.Value, userId);
            var tank = await _context.Tanks.FindAsync(tankId.Value);
            ViewBag.TankName = tank?.Name;
        }
        else
        {
            mediaItems = await _mediaService.GetAllMediaForUserAsync(userId);
        }

        return View(mediaItems);
    }
}
