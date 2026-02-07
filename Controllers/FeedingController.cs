using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;
using System.Text.Json;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class FeedingController : Controller
{
    private readonly IFeedingService _feedingService;
    private readonly ITankService _tankService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<FeedingController> _logger;

    public FeedingController(
        IFeedingService feedingService,
        ITankService tankService,
        UserManager<AppUser> userManager,
        ILogger<FeedingController> logger)
    {
        _feedingService = feedingService;
        _tankService = tankService;
        _userManager = userManager;
        _logger = logger;
    }

    #region Feeding Schedule Actions

    // GET: Feeding/Index
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var schedules = await _feedingService.GetSchedulesForUserAsync(userId);
        var upcomingFeedings = await _feedingService.GetUpcomingFeedingsForUserAsync(userId);

        ViewBag.UpcomingFeedings = upcomingFeedings;

        return View(schedules);
    }

    // GET: Feeding/CreateSchedule
    public async Task<IActionResult> CreateSchedule(int? tankId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await PopulateTanksDropdown(userId, tankId);

        var model = new FeedingSchedule
        {
            TankId = tankId ?? 0,
            Unit = "grams",
            IsActive = true
        };

        return View(model);
    }

    // POST: Feeding/CreateSchedule
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSchedule(FeedingSchedule schedule, string feedingTimesJson)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!string.IsNullOrEmpty(feedingTimesJson))
        {
            schedule.FeedingTimes = feedingTimesJson;
        }

        if (ModelState.IsValid)
        {
            var success = await _feedingService.CreateScheduleAsync(schedule, userId);

            if (success)
            {
                TempData["Success"] = "Feeding schedule created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to create feeding schedule.");
        }

        await PopulateTanksDropdown(userId, schedule.TankId);
        return View(schedule);
    }

    // GET: Feeding/EditSchedule/5
    public async Task<IActionResult> EditSchedule(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var schedule = await _feedingService.GetScheduleByIdAsync(id, userId);
        if (schedule == null)
            return NotFound();

        await PopulateTanksDropdown(userId, schedule.TankId);

        return View(schedule);
    }

    // POST: Feeding/EditSchedule/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSchedule(int id, FeedingSchedule schedule, string feedingTimesJson)
    {
        if (id != schedule.Id)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!string.IsNullOrEmpty(feedingTimesJson))
        {
            schedule.FeedingTimes = feedingTimesJson;
        }

        if (ModelState.IsValid)
        {
            var success = await _feedingService.UpdateScheduleAsync(schedule, userId);

            if (success)
            {
                TempData["Success"] = "Feeding schedule updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to update feeding schedule.");
        }

        await PopulateTanksDropdown(userId, schedule.TankId);
        return View(schedule);
    }

    // GET: Feeding/DeleteSchedule/5
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var schedule = await _feedingService.GetScheduleByIdAsync(id, userId);
        if (schedule == null)
            return NotFound();

        return View(schedule);
    }

    // POST: Feeding/DeleteSchedule/5
    [HttpPost, ActionName("DeleteSchedule")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteScheduleConfirmed(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _feedingService.DeleteScheduleAsync(id, userId);

        if (success)
        {
            TempData["Success"] = "Feeding schedule deleted successfully!";
        }
        else
        {
            TempData["Error"] = "Failed to delete feeding schedule.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Feeding/ToggleSchedule/5
    [HttpPost]
    public async Task<IActionResult> ToggleSchedule(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _feedingService.ToggleScheduleActiveAsync(id, userId);

        return Json(new { success });
    }

    #endregion

    #region Feeding Record Actions

    // GET: Feeding/Records
    public async Task<IActionResult> Records(int? tankId, DateTime? startDate, DateTime? endDate)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        IEnumerable<FeedingRecord> records;

        if (tankId.HasValue)
        {
            records = await _feedingService.GetRecordsForTankAsync(tankId.Value, startDate, endDate);
        }
        else
        {
            records = await _feedingService.GetRecordsForUserAsync(userId, startDate, endDate);
        }

        var tanks = await _tankService.GetAllTanksAsync(userId);
        ViewBag.Tanks = new SelectList(tanks, "Id", "Name", tankId);
        ViewBag.SelectedTankId = tankId;
        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

        return View(records);
    }

    // GET: Feeding/RecordFeeding
    public async Task<IActionResult> RecordFeeding(int? tankId, int? scheduleId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var model = new FeedingRecord
        {
            FedDateTime = DateTime.UtcNow,
            Unit = "grams"
        };

        // If schedule is provided, pre-fill from schedule
        if (scheduleId.HasValue)
        {
            var schedule = await _feedingService.GetScheduleByIdAsync(scheduleId.Value, userId);
            if (schedule != null)
            {
                model.TankId = schedule.TankId;
                model.FeedingScheduleId = scheduleId.Value;
                model.FeedType = schedule.FeedType;
                model.FeedName = schedule.FeedName;
                model.Brand = schedule.Brand;
                model.Amount = schedule.Amount;
                model.Unit = schedule.Unit;
                model.WasScheduled = true;
            }
        }
        else if (tankId.HasValue)
        {
            model.TankId = tankId.Value;
        }

        await PopulateTanksDropdown(userId, model.TankId);

        return View(model);
    }

    // POST: Feeding/RecordFeeding
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordFeeding(FeedingRecord record)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (ModelState.IsValid)
        {
            var success = await _feedingService.CreateRecordAsync(record, userId);

            if (success)
            {
                TempData["Success"] = "Feeding recorded successfully!";
                return RedirectToAction(nameof(Records), new { tankId = record.TankId });
            }

            ModelState.AddModelError("", "Failed to record feeding.");
        }

        await PopulateTanksDropdown(userId, record.TankId);
        return View(record);
    }

    // POST: Feeding/RecordScheduledFeeding
    [HttpPost]
    public async Task<IActionResult> RecordScheduledFeeding(int scheduleId, string scheduledTime, string? notes)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (TimeSpan.TryParse(scheduledTime, out var time))
        {
            var success = await _feedingService.RecordScheduledFeedingAsync(scheduleId, time, userId, notes);
            return Json(new { success });
        }

        return Json(new { success = false, message = "Invalid time format" });
    }

    // GET: Feeding/EditRecord/5
    public async Task<IActionResult> EditRecord(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var record = await _feedingService.GetRecordByIdAsync(id, userId);
        if (record == null)
            return NotFound();

        await PopulateTanksDropdown(userId, record.TankId);

        return View(record);
    }

    // POST: Feeding/EditRecord/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRecord(int id, FeedingRecord record)
    {
        if (id != record.Id)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (ModelState.IsValid)
        {
            var success = await _feedingService.UpdateRecordAsync(record, userId);

            if (success)
            {
                TempData["Success"] = "Feeding record updated successfully!";
                return RedirectToAction(nameof(Records), new { tankId = record.TankId });
            }

            ModelState.AddModelError("", "Failed to update feeding record.");
        }

        await PopulateTanksDropdown(userId, record.TankId);
        return View(record);
    }

    // GET: Feeding/DeleteRecord/5
    public async Task<IActionResult> DeleteRecord(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var record = await _feedingService.GetRecordByIdAsync(id, userId);
        if (record == null)
            return NotFound();

        return View(record);
    }

    // POST: Feeding/DeleteRecord/5
    [HttpPost, ActionName("DeleteRecord")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRecordConfirmed(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var record = await _feedingService.GetRecordByIdAsync(id, userId);
        var tankId = record?.TankId;

        var success = await _feedingService.DeleteRecordAsync(id, userId);

        if (success)
        {
            TempData["Success"] = "Feeding record deleted successfully!";
        }
        else
        {
            TempData["Error"] = "Failed to delete feeding record.";
        }

        return RedirectToAction(nameof(Records), new { tankId });
    }

    #endregion

    #region Dashboard Actions

    // GET: Feeding/Dashboard
    public async Task<IActionResult> Dashboard(int? tankId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var tanks = await _tankService.GetAllTanksAsync(userId);

        if (!tankId.HasValue && tanks.Any())
        {
            tankId = tanks.First().Id;
        }

        if (tankId.HasValue)
        {
            ViewBag.ActiveSchedules = await _feedingService.GetActiveSchedulesForTankAsync(tankId.Value);
            ViewBag.TodaysFeedings = await _feedingService.GetTodaysFeedingsForTankAsync(tankId.Value);
            ViewBag.SelectedTank = tanks.FirstOrDefault(t => t.Id == tankId.Value);
        }

        ViewBag.Tanks = new SelectList(tanks, "Id", "Name", tankId);
        ViewBag.FeedingStreaks = await _feedingService.GetFeedingStreaksForUserAsync(userId);

        return View();
    }

    #endregion

    #region Helper Methods

    private async Task PopulateTanksDropdown(string userId, int? selectedTankId = null)
    {
        var tanks = await _tankService.GetAllTanksAsync(userId);
        ViewBag.Tanks = new SelectList(tanks, "Id", "Name", selectedTankId);
    }

    #endregion
}
