using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class MaintenanceLogController : Controller
{
    private readonly IMaintenanceLogService _maintenanceLogService;
    private readonly ITankService _tankService;
    private readonly ISupplyService _supplyService;
    private readonly IReminderService _reminderService;
    private readonly IEquipmentService _equipmentService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<MaintenanceLogController> _logger;
    private readonly IImageService _imageService;

    public MaintenanceLogController(
        IMaintenanceLogService maintenanceLogService,
        ITankService tankService,
        ISupplyService supplyService,
        IReminderService reminderService,
        IEquipmentService equipmentService,
        UserManager<AppUser> userManager,
        ILogger<MaintenanceLogController> logger,
        IImageService imageService)
    {
        _maintenanceLogService = maintenanceLogService;
        _tankService = tankService;
        _supplyService = supplyService;
        _reminderService = reminderService;
        _equipmentService = equipmentService;
        _userManager = userManager;
        _logger = logger;
        _imageService = imageService;
    }

    // GET: MaintenanceLog
    public async Task<IActionResult> Index(int? tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            List<MaintenanceLog> maintenanceLogs;
            if (tankId.HasValue)
            {
                maintenanceLogs = await _maintenanceLogService.GetMaintenanceLogsByTankAsync(tankId.Value, userId);
                var tank = await _tankService.GetTankByIdAsync(tankId.Value, userId);
                ViewBag.TankName = tank?.Name;
                ViewBag.TankId = tankId.Value;
            }
            else
            {
                maintenanceLogs = await _maintenanceLogService.GetAllMaintenanceLogsAsync(userId);
            }

            return View(maintenanceLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving maintenance logs: {ex.Message}\n{ex.StackTrace}");
            TempData["Error"] = $"An error occurred while retrieving maintenance logs: {ex.Message}";
            return View(new List<MaintenanceLog>());
        }
    }

    // GET: MaintenanceLog/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var maintenanceLog = await _maintenanceLogService.GetMaintenanceLogByIdAsync(id, userId);
            if (maintenanceLog == null)
            {
                return NotFound();
            }

            // Convert image data to displayable format
            var maintenanceImage = _imageService.ConvertByteArrayToFile(
                maintenanceLog.ImageData,
                maintenanceLog.ImageType,
                Models.Enums.DefaultImage.MaintenanceLogImage
            );

            ViewData["MaintenanceImage"] = maintenanceImage;

            return View(maintenanceLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving maintenance log details for ID: {MaintenanceLogId}", id);
            TempData["Error"] = "An error occurred while retrieving maintenance log details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: MaintenanceLog/Create
    public async Task<IActionResult> Create(int? tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await PopulateTanksDropdown(userId);
            await PopulateSuppliesDropdown(userId);

            if (tankId.HasValue)
            {
                ViewBag.SelectedTankId = tankId.Value;
            }

            // Pre-populate with current date
            var maintenanceLog = new MaintenanceLog
            {
                Timestamp = DateTime.Now
            };

            return View(maintenanceLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create maintenance log view");
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: MaintenanceLog/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Timestamp,Type,WaterChangePercent,Notes,SupplyItemId,AmountUsed")] MaintenanceLog maintenanceLog,
        int tankId,
        string? filterMediaType = null,
        bool createFilterReminder = false,
        int filterReminderFrequency = 14,
        int filterReminderDays = 3,
        bool filterEmailNotification = false,
        string? equipmentType = null,
        int? specificEquipmentId = null,
        string? maintenanceActivity = null)
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
                // Add filter media type to notes if provided
                if (!string.IsNullOrEmpty(filterMediaType))
                {
                    maintenanceLog.Notes = string.IsNullOrEmpty(maintenanceLog.Notes)
                        ? $"Filter Media: {filterMediaType}"
                        : $"{maintenanceLog.Notes}\n\nFilter Media: {filterMediaType}";
                }

                // Add equipment maintenance info to notes if provided
                if (!string.IsNullOrEmpty(equipmentType) || specificEquipmentId.HasValue)
                {
                    var equipmentInfo = new System.Text.StringBuilder();

                    if (specificEquipmentId.HasValue)
                    {
                        var equipment = await _equipmentService.GetEquipmentByIdAsync(specificEquipmentId.Value, userId);
                        if (equipment != null)
                        {
                            equipmentInfo.AppendLine($"Equipment: {equipment.Brand} {equipment.Model}");
                        }
                    }
                    else if (!string.IsNullOrEmpty(equipmentType))
                    {
                        equipmentInfo.AppendLine($"Equipment Type: {equipmentType}");
                    }

                    if (!string.IsNullOrEmpty(maintenanceActivity))
                    {
                        equipmentInfo.AppendLine($"Activity: {maintenanceActivity}");
                    }

                    maintenanceLog.Notes = string.IsNullOrEmpty(maintenanceLog.Notes)
                        ? equipmentInfo.ToString()
                        : $"{maintenanceLog.Notes}\n\n{equipmentInfo}";
                }

                var createdMaintenanceLog = await _maintenanceLogService.CreateMaintenanceLogAsync(
                    maintenanceLog,
                    tankId,
                    userId,
                    maintenanceLog.SupplyItemId,
                    maintenanceLog.AmountUsed);

                var successMessage = "Maintenance log recorded successfully!";

                if (maintenanceLog.SupplyItemId.HasValue && maintenanceLog.AmountUsed.HasValue)
                {
                    successMessage += " Supply inventory updated.";
                }

                // Create filter cleaning reminder if requested
                if (createFilterReminder && maintenanceLog.Type == Models.Enums.MaintenanceType.FilterCleaning)
                {
                    var reminder = new Reminder
                    {
                        UserId = userId,
                        TankId = tankId,
                        Title = $"Filter Cleaning - {filterMediaType ?? "Media Change"}",
                        Description = $"Time to clean/replace filter media. Last changed on {maintenanceLog.Timestamp:MMM dd, yyyy}",
                        Type = Models.Enums.ReminderType.Maintenance,
                        Frequency = Models.Enums.ReminderFrequency.Custom,
                        NextDueDate = maintenanceLog.Timestamp.AddDays(filterReminderFrequency),
                        NotificationHoursBefore = filterReminderDays * 24,
                        SendEmailNotification = filterEmailNotification,
                        IsActive = true
                    };

                    await _reminderService.CreateReminderAsync(reminder);
                    successMessage += " Reminder created for next filter change.";
                }

                TempData["Success"] = successMessage;
                return RedirectToAction(nameof(Index), new { tankId });
            }

            await PopulateTanksDropdown(userId);
            await PopulateSuppliesDropdown(userId);
            ViewBag.SelectedTankId = tankId;
            return View(maintenanceLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating maintenance log");
            TempData["Error"] = "An error occurred while recording the maintenance log.";
            await PopulateTanksDropdown(_userManager.GetUserId(User)!);
            await PopulateSuppliesDropdown(_userManager.GetUserId(User)!);
            return View(maintenanceLog);
        }
    }

    // GET: MaintenanceLog/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var maintenanceLog = await _maintenanceLogService.GetMaintenanceLogByIdAsync(id, userId);
            if (maintenanceLog == null)
            {
                return NotFound();
            }

            await PopulateTanksDropdown(userId);
            return View(maintenanceLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving maintenance log for edit, ID: {MaintenanceLogId}", id);
            TempData["Error"] = "An error occurred while retrieving the maintenance log.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: MaintenanceLog/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Timestamp,Type,WaterChangePercent,Notes")] MaintenanceLog maintenanceLog)
    {
        if (id != maintenanceLog.Id)
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

            if (ModelState.IsValid)
            {
                await _maintenanceLogService.UpdateMaintenanceLogAsync(maintenanceLog, userId);
                TempData["Success"] = "Maintenance log updated successfully!";
                return RedirectToAction(nameof(Details), new { id = maintenanceLog.Id });
            }

            await PopulateTanksDropdown(userId);
            return View(maintenanceLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating maintenance log ID: {MaintenanceLogId}", id);
            TempData["Error"] = "An error occurred while updating the maintenance log.";
            await PopulateTanksDropdown(_userManager.GetUserId(User)!);
            return View(maintenanceLog);
        }
    }

    // GET: MaintenanceLog/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var maintenanceLog = await _maintenanceLogService.GetMaintenanceLogByIdAsync(id, userId);
            if (maintenanceLog == null)
            {
                return NotFound();
            }

            return View(maintenanceLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving maintenance log for deletion, ID: {MaintenanceLogId}", id);
            TempData["Error"] = "An error occurred while retrieving the maintenance log.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: MaintenanceLog/Delete/5
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

            var result = await _maintenanceLogService.DeleteMaintenanceLogAsync(id, userId);
            if (result)
            {
                TempData["Success"] = "Maintenance log deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete maintenance log.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting maintenance log ID: {MaintenanceLogId}", id);
            TempData["Error"] = "An error occurred while deleting the maintenance log.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task PopulateTanksDropdown(string userId)
    {
        var tanks = await _tankService.GetAllTanksAsync(userId);
        ViewBag.Tanks = new SelectList(tanks, "Id", "Name");
    }

    private async Task PopulateSuppliesDropdown(string userId)
    {
        var supplies = await _supplyService.GetSuppliesByUserAsync(userId);
        // Filter to relevant categories for maintenance (medications, treatments, additives, etc.)
        var relevantSupplies = supplies
            .Where(s => s.Category == Models.Enums.SupplyCategory.Medications
                     || s.Category == Models.Enums.SupplyCategory.WaterTreatment
                     || s.Category == Models.Enums.SupplyCategory.Supplements
                     || s.Category == Models.Enums.SupplyCategory.TestKits
                     || s.Category == Models.Enums.SupplyCategory.Chemicals)
            .Select(s => new
            {
                s.Id,
                DisplayName = $"{s.Name} ({s.CurrentQuantity} {s.Unit}) - {s.Category}"
            })
            .ToList();
        ViewBag.Supplies = new SelectList(relevantSupplies, "Id", "DisplayName");
    }
}
