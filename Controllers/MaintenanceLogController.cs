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
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<MaintenanceLogController> _logger;

    public MaintenanceLogController(
        IMaintenanceLogService maintenanceLogService,
        ITankService tankService,
        UserManager<AppUser> userManager,
        ILogger<MaintenanceLogController> logger)
    {
        _maintenanceLogService = maintenanceLogService;
        _tankService = tankService;
        _userManager = userManager;
        _logger = logger;
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
            _logger.LogError(ex, "Error retrieving maintenance logs");
            TempData["Error"] = "An error occurred while retrieving maintenance logs.";
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
        [Bind("Timestamp,Type,WaterChangePercent,Notes")] MaintenanceLog maintenanceLog,
        int tankId)
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
                var createdMaintenanceLog = await _maintenanceLogService.CreateMaintenanceLogAsync(maintenanceLog, tankId, userId);
                TempData["Success"] = "Maintenance log recorded successfully!";
                return RedirectToAction(nameof(Index), new { tankId });
            }

            await PopulateTanksDropdown(userId);
            ViewBag.SelectedTankId = tankId;
            return View(maintenanceLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating maintenance log");
            TempData["Error"] = "An error occurred while recording the maintenance log.";
            await PopulateTanksDropdown(_userManager.GetUserId(User)!);
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
}
