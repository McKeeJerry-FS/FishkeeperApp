using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class ReminderController : Controller
{
    private readonly IReminderService _reminderService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ReminderController> _logger;

    public ReminderController(
        IReminderService reminderService,
        UserManager<AppUser> userManager,
        ILogger<ReminderController> logger)
    {
        _reminderService = reminderService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Reminder
    public async Task<IActionResult> Index(string filter = "all")
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            List<Reminder> reminders;

            switch (filter.ToLower())
            {
                case "active":
                    reminders = await _reminderService.GetActiveRemindersAsync(userId);
                    ViewBag.FilterType = "Active";
                    break;
                case "upcoming":
                    reminders = await _reminderService.GetUpcomingRemindersAsync(userId, 168); // 7 days
                    ViewBag.FilterType = "Upcoming";
                    break;
                case "overdue":
                    reminders = await _reminderService.GetOverdueRemindersAsync(userId);
                    ViewBag.FilterType = "Overdue";
                    break;
                default:
                    reminders = await _reminderService.GetUserRemindersAsync(userId);
                    ViewBag.FilterType = "All";
                    break;
            }

            ViewBag.CurrentFilter = filter;
            return View(reminders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reminders");
            TempData["Error"] = "An error occurred while retrieving reminders.";
            return View(new List<Reminder>());
        }
    }

    // GET: Reminder/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var reminder = await _reminderService.GetReminderByIdAsync(id, userId);
            if (reminder == null)
            {
                return NotFound();
            }

            return View(reminder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reminder details for ID: {ReminderId}", id);
            TempData["Error"] = "An error occurred while retrieving reminder details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Reminder/Create
    public IActionResult Create()
    {
        // Pre-populate with current date
        var reminder = new Reminder
        {
            NextDueDate = DateTime.Now.AddDays(7),
            IsActive = true
        };

        return View(reminder);
    }

    // POST: Reminder/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,Type,Frequency,NextDueDate,IsActive")] Reminder reminder)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Remove UserId from ModelState since it's set by the controller
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                reminder.UserId = userId;
                var createdReminder = await _reminderService.CreateReminderAsync(reminder);
                TempData["Success"] = "Reminder created successfully!";
                return RedirectToAction(nameof(Details), new { id = createdReminder.Id });
            }

            return View(reminder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reminder");
            TempData["Error"] = "An error occurred while creating the reminder.";
            return View(reminder);
        }
    }

    // GET: Reminder/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var reminder = await _reminderService.GetReminderByIdAsync(id, userId);
            if (reminder == null)
            {
                return NotFound();
            }

            return View(reminder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reminder for edit, ID: {ReminderId}", id);
            TempData["Error"] = "An error occurred while retrieving the reminder.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Reminder/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,Description,Type,Frequency,NextDueDate,IsActive")] Reminder reminder)
    {
        if (id != reminder.Id)
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

            // Remove UserId from ModelState since it's set by the controller
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                reminder.UserId = userId;
                await _reminderService.UpdateReminderAsync(reminder);
                TempData["Success"] = "Reminder updated successfully!";
                return RedirectToAction(nameof(Details), new { id = reminder.Id });
            }

            return View(reminder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reminder ID: {ReminderId}", id);
            TempData["Error"] = "An error occurred while updating the reminder.";
            return View(reminder);
        }
    }

    // POST: Reminder/Complete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _reminderService.CompleteReminderAsync(id, userId);
            if (result)
            {
                TempData["Success"] = "Reminder marked as complete!";
            }
            else
            {
                TempData["Error"] = "Failed to complete reminder.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing reminder ID: {ReminderId}", id);
            TempData["Error"] = "An error occurred while completing the reminder.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Reminder/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var reminder = await _reminderService.GetReminderByIdAsync(id, userId);
            if (reminder == null)
            {
                return NotFound();
            }

            return View(reminder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reminder for deletion, ID: {ReminderId}", id);
            TempData["Error"] = "An error occurred while retrieving the reminder.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Reminder/Delete/5
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

            var result = await _reminderService.DeleteReminderAsync(id, userId);
            if (result)
            {
                TempData["Success"] = "Reminder deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete reminder.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reminder ID: {ReminderId}", id);
            TempData["Error"] = "An error occurred while deleting the reminder.";
            return RedirectToAction(nameof(Index));
        }
    }
}
