using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class TankController : Controller
{
    private readonly ITankService _tankService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<TankController> _logger;

    public TankController(
        ITankService tankService,
        UserManager<AppUser> userManager,
        ILogger<TankController> logger)
    {
        _tankService = tankService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Tank
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var tanks = await _tankService.GetAllTanksAsync(userId);
            return View(tanks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tanks");
            TempData["Error"] = "An error occurred while retrieving your tanks.";
            return View(new List<Tank>());
        }
    }

    // GET: Tank/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var tank = await _tankService.GetTankByIdAsync(id, userId);
            if (tank == null)
            {
                return NotFound();
            }

            return View(tank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tank details for ID: {TankId}", id);
            TempData["Error"] = "An error occurred while retrieving tank details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Tank/Dashboard/5
    public async Task<IActionResult> Dashboard(int id, int? month, int? year)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var currentMonth = month ?? DateTime.Now.Month;
            var currentYear = year ?? DateTime.Now.Year;

            var dashboard = await _tankService.GetTankDashboardAsync(id, userId, currentMonth, currentYear);
            if (dashboard == null)
            {
                return NotFound();
            }

            return View(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tank dashboard for ID: {TankId}", id);
            TempData["Error"] = "An error occurred while retrieving tank dashboard.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Tank/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Tank/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Type,VolumeGallons,VolumeUnitLiters,Length,Width,Height,Substrate,SetupDate,Notes,ImageURL")] Tank tank)
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
                var createdTank = await _tankService.CreateTankAsync(tank, userId);
                TempData["Success"] = "Tank created successfully!";
                return RedirectToAction(nameof(Details), new { id = createdTank.Id });
            }

            return View(tank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tank");
            TempData["Error"] = "An error occurred while creating the tank.";
            return View(tank);
        }
    }

    // GET: Tank/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var tank = await _tankService.GetTankByIdAsync(id, userId);
            if (tank == null)
            {
                return NotFound();
            }

            return View(tank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tank for edit, ID: {TankId}", id);
            TempData["Error"] = "An error occurred while retrieving the tank.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Tank/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,VolumeGallons,VolumeUnitLiters,Length,Width,Height,Substrate,SetupDate,Notes,ImageURL")] Tank tank)
    {
        if (id != tank.Id)
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
                await _tankService.UpdateTankAsync(tank, userId);
                TempData["Success"] = "Tank updated successfully!";
                return RedirectToAction(nameof(Details), new { id = tank.Id });
            }

            return View(tank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tank ID: {TankId}", id);
            TempData["Error"] = "An error occurred while updating the tank.";
            return View(tank);
        }
    }

    // GET: Tank/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var tank = await _tankService.GetTankByIdAsync(id, userId);
            if (tank == null)
            {
                return NotFound();
            }

            return View(tank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tank for deletion, ID: {TankId}", id);
            TempData["Error"] = "An error occurred while retrieving the tank.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Tank/Delete/5
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

            var result = await _tankService.DeleteTankAsync(id, userId);
            if (result)
            {
                TempData["Success"] = "Tank deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete tank.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tank ID: {TankId}", id);
            TempData["Error"] = "An error occurred while deleting the tank.";
            return RedirectToAction(nameof(Index));
        }
    }
}
