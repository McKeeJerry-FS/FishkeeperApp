using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;
using AquaHub.MVC.Utilities;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class GrowthRecordController : Controller
{
    private readonly IGrowthRecordService _growthRecordService;
    private readonly ILivestockService _livestockService;
    private readonly ITankService _tankService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<GrowthRecordController> _logger;
    private readonly IWebHostEnvironment _environment;

    public GrowthRecordController(
        IGrowthRecordService growthRecordService,
        ILivestockService livestockService,
        ITankService tankService,
        UserManager<AppUser> userManager,
        ILogger<GrowthRecordController> logger,
        IWebHostEnvironment environment)
    {
        _growthRecordService = growthRecordService;
        _livestockService = livestockService;
        _tankService = tankService;
        _userManager = userManager;
        _logger = logger;
        _environment = environment;
    }

    // GET: GrowthRecord
    public async Task<IActionResult> Index(int? livestockId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            List<GrowthRecord> growthRecords;

            if (livestockId.HasValue)
            {
                growthRecords = await _growthRecordService.GetGrowthRecordsForLivestockAsync(livestockId.Value, userId);
                var livestock = await _livestockService.GetLivestockByIdAsync(livestockId.Value, userId);
                ViewBag.LivestockName = livestock?.Name;
                ViewBag.LivestockId = livestockId.Value;
            }
            else
            {
                growthRecords = await _growthRecordService.GetRecentGrowthRecordsAsync(userId, 50);
            }

            return View(growthRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving growth records");
            TempData["Error"] = "An error occurred while retrieving growth records.";
            return View(new List<GrowthRecord>());
        }
    }

    // GET: GrowthRecord/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var growthRecord = await _growthRecordService.GetGrowthRecordByIdAsync(id, userId);
            if (growthRecord == null)
            {
                return NotFound();
            }

            return View(growthRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving growth record details for ID: {GrowthRecordId}", id);
            TempData["Error"] = "An error occurred while retrieving growth record details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: GrowthRecord/Statistics/5
    public async Task<IActionResult> Statistics(int livestockId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var statistics = await _growthRecordService.GetGrowthStatisticsAsync(livestockId, userId);
            if (statistics == null)
            {
                TempData["Warning"] = "No growth statistics available for this livestock yet.";
                return RedirectToAction(nameof(Index), new { livestockId });
            }

            var livestock = await _livestockService.GetLivestockByIdAsync(livestockId, userId);
            ViewBag.LivestockName = livestock?.Name;
            ViewBag.LivestockId = livestockId;

            return View(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving growth statistics for livestock ID: {LivestockId}", livestockId);
            TempData["Error"] = "An error occurred while retrieving growth statistics.";
            return RedirectToAction(nameof(Index), new { livestockId });
        }
    }

    // GET: GrowthRecord/Create
    public async Task<IActionResult> Create(int? livestockId, int? tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Populate livestock dropdown
            await PopulateLivestockDropdownAsync(userId, livestockId, tankId);

            // Pre-populate with current date
            var growthRecord = new GrowthRecord
            {
                MeasurementDate = DateTime.Now,
                LivestockId = livestockId ?? 0
            };

            // If a specific livestock is provided, set the name in ViewBag
            if (livestockId.HasValue)
            {
                var livestock = await _livestockService.GetLivestockByIdAsync(livestockId.Value, userId);
                if (livestock == null)
                {
                    return NotFound();
                }
                ViewBag.LivestockName = livestock.Name;
            }

            return View(growthRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create growth record view");
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction("Index", "Livestock");
        }
    }

    // POST: GrowthRecord/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("LivestockId,MeasurementDate,LengthInches,WeightGrams,DiameterInches,HeightInches,HealthCondition,ColorVibrancy,Notes")] GrowthRecord growthRecord,
        IFormFile? ImageFile,
        IFormFile? CameraImageFile)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                var selectedImage = ImageFile ?? CameraImageFile;
                if (selectedImage != null)
                {
                    growthRecord.PhotoPath = await ImageStorageHelper.SaveImageAsync(
                        _environment,
                        selectedImage,
                        "images",
                        "growth-records");

                    if (string.IsNullOrWhiteSpace(growthRecord.PhotoPath))
                    {
                        ModelState.AddModelError(string.Empty, "The image could not be uploaded.");
                        throw new InvalidOperationException("Image upload failed for growth record creation.");
                    }
                }

                var createdGrowthRecord = await _growthRecordService.AddGrowthRecordAsync(growthRecord, userId);
                TempData["Success"] = "Growth record added successfully!";
                return RedirectToAction(nameof(Index), new { livestockId = growthRecord.LivestockId });
            }

            // Re-populate dropdown on validation failure
            await PopulateLivestockDropdownAsync(userId, growthRecord.LivestockId, null);

            var livestock = await _livestockService.GetLivestockByIdAsync(growthRecord.LivestockId, userId);
            ViewBag.LivestockName = livestock?.Name;

            return View(growthRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating growth record");
            TempData["Error"] = "An error occurred while adding the growth record.";
            return View(growthRecord);
        }
    }

    // GET: GrowthRecord/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var growthRecord = await _growthRecordService.GetGrowthRecordByIdAsync(id, userId);
            if (growthRecord == null)
            {
                return NotFound();
            }

            var livestock = await _livestockService.GetLivestockByIdAsync(growthRecord.LivestockId, userId);
            ViewBag.LivestockName = livestock?.Name;

            return View(growthRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving growth record for edit, ID: {GrowthRecordId}", id);
            TempData["Error"] = "An error occurred while retrieving the growth record.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: GrowthRecord/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,LivestockId,MeasurementDate,LengthInches,WeightGrams,DiameterInches,HeightInches,HealthCondition,ColorVibrancy,Notes")] GrowthRecord growthRecord,
        IFormFile? ImageFile,
        IFormFile? CameraImageFile)
    {
        if (id != growthRecord.Id)
        {
            return NotFound();
        }

        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var existingGrowthRecord = await _growthRecordService.GetGrowthRecordByIdAsync(id, userId);
            if (existingGrowthRecord == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                growthRecord.PhotoPath = existingGrowthRecord.PhotoPath;

                var selectedImage = ImageFile ?? CameraImageFile;
                if (selectedImage != null)
                {
                    var previousPhotoPath = existingGrowthRecord.PhotoPath;
                    growthRecord.PhotoPath = await ImageStorageHelper.SaveImageAsync(
                        _environment,
                        selectedImage,
                        "images",
                        "growth-records");

                    if (string.IsNullOrWhiteSpace(growthRecord.PhotoPath))
                    {
                        ModelState.AddModelError(string.Empty, "The image could not be uploaded.");
                        throw new InvalidOperationException("Image upload failed for growth record update.");
                    }

                    if (!string.Equals(previousPhotoPath, growthRecord.PhotoPath, StringComparison.OrdinalIgnoreCase))
                    {
                        ImageStorageHelper.DeleteImageIfExists(_environment, previousPhotoPath);
                    }
                }

                await _growthRecordService.UpdateGrowthRecordAsync(growthRecord, userId);
                TempData["Success"] = "Growth record updated successfully!";
                return RedirectToAction(nameof(Index), new { livestockId = growthRecord.LivestockId });
            }

            growthRecord.PhotoPath = existingGrowthRecord.PhotoPath;

            var livestock = await _livestockService.GetLivestockByIdAsync(growthRecord.LivestockId, userId);
            ViewBag.LivestockName = livestock?.Name;

            return View(growthRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating growth record ID: {GrowthRecordId}", id);
            TempData["Error"] = "An error occurred while updating the growth record.";
            return View(growthRecord);
        }
    }

    // GET: GrowthRecord/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var growthRecord = await _growthRecordService.GetGrowthRecordByIdAsync(id, userId);
            if (growthRecord == null)
            {
                return NotFound();
            }

            return View(growthRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving growth record for deletion, ID: {GrowthRecordId}", id);
            TempData["Error"] = "An error occurred while retrieving the growth record.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: GrowthRecord/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var growthRecord = await _growthRecordService.GetGrowthRecordByIdAsync(id, userId);
            var livestockId = growthRecord?.LivestockId;

            var result = await _growthRecordService.DeleteGrowthRecordAsync(id, userId);
            if (result)
            {
                TempData["Success"] = "Growth record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete growth record.";
            }

            if (livestockId.HasValue)
            {
                return RedirectToAction(nameof(Index), new { livestockId });
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting growth record ID: {GrowthRecordId}", id);
            TempData["Error"] = "An error occurred while deleting the growth record.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task PopulateLivestockDropdownAsync(string userId, int? selectedLivestockId = null, int? tankId = null)
    {
        List<Livestock> livestock;

        if (tankId.HasValue)
        {
            // Get livestock for a specific tank
            livestock = await _livestockService.GetLivestockByTankAsync(tankId.Value, userId);
        }
        else
        {
            // Get all livestock for the user
            livestock = await _livestockService.GetAllLivestockAsync(userId);
        }

        ViewBag.LivestockList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            livestock.Select(l => new
            {
                l.Id,
                DisplayName = $"{l.Name} ({l.Species})"
            }),
            "Id",
            "DisplayName",
            selectedLivestockId
        );
    }
}
