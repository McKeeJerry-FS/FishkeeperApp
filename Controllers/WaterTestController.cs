using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class WaterTestController : Controller
{
    private readonly IWaterTestService _waterTestService;
    private readonly ITankService _tankService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<WaterTestController> _logger;

    public WaterTestController(
        IWaterTestService waterTestService,
        ITankService tankService,
        UserManager<AppUser> userManager,
        ILogger<WaterTestController> logger)
    {
        _waterTestService = waterTestService;
        _tankService = tankService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: WaterTest
    public async Task<IActionResult> Index(int? tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            List<WaterTest> waterTests;
            if (tankId.HasValue)
            {
                waterTests = await _waterTestService.GetWaterTestsByTankAsync(tankId.Value, userId);
                var tank = await _tankService.GetTankByIdAsync(tankId.Value, userId);
                ViewBag.TankName = tank?.Name;
                ViewBag.TankId = tankId.Value;
            }
            else
            {
                waterTests = await _waterTestService.GetAllWaterTestsAsync(userId);
            }

            return View(waterTests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving water tests");
            TempData["Error"] = "An error occurred while retrieving water tests.";
            return View(new List<WaterTest>());
        }
    }

    // GET: WaterTest/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var waterTest = await _waterTestService.GetWaterTestByIdAsync(id, userId);
            if (waterTest == null)
            {
                return NotFound();
            }

            return View(waterTest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving water test details for ID: {WaterTestId}", id);
            TempData["Error"] = "An error occurred while retrieving water test details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: WaterTest/Trends/5
    public async Task<IActionResult> Trends(int tankId, DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var trends = await _waterTestService.GetParameterTrendsAsync(tankId, userId, startDate, endDate);
            if (trends == null)
            {
                return NotFound();
            }

            return View(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parameter trends for tank ID: {TankId}", tankId);
            TempData["Error"] = "An error occurred while retrieving parameter trends.";
            return RedirectToAction(nameof(Index), new { tankId });
        }
    }

    // GET: WaterTest/Create
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
            var waterTest = new WaterTest
            {
                Timestamp = DateTime.Now
            };

            return View(waterTest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create water test view");
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: WaterTest/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Timestamp,Temperature,PH,Ammonia,Nitrite,Nitrate,Phosphate,Alkalinity,Calcium,Magnesium,Salinity,GH,KH,TDS")] WaterTest waterTest,
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
                var createdWaterTest = await _waterTestService.CreateWaterTestAsync(waterTest, tankId, userId);
                TempData["Success"] = "Water test recorded successfully!";
                return RedirectToAction(nameof(Index), new { tankId });
            }

            await PopulateTanksDropdown(userId);
            ViewBag.SelectedTankId = tankId;
            return View(waterTest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating water test");
            TempData["Error"] = "An error occurred while recording the water test.";
            await PopulateTanksDropdown(_userManager.GetUserId(User)!);
            return View(waterTest);
        }
    }

    // GET: WaterTest/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var waterTest = await _waterTestService.GetWaterTestByIdAsync(id, userId);
            if (waterTest == null)
            {
                return NotFound();
            }

            await PopulateTanksDropdown(userId);
            return View(waterTest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving water test for edit, ID: {WaterTestId}", id);
            TempData["Error"] = "An error occurred while retrieving the water test.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: WaterTest/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Timestamp,Temperature,PH,Ammonia,Nitrite,Nitrate,Phosphate,Alkalinity,Calcium,Magnesium,Salinity,GH,KH,TDS")] WaterTest waterTest)
    {
        if (id != waterTest.Id)
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
                await _waterTestService.UpdateWaterTestAsync(waterTest, userId);
                TempData["Success"] = "Water test updated successfully!";
                return RedirectToAction(nameof(Details), new { id = waterTest.Id });
            }

            await PopulateTanksDropdown(userId);
            return View(waterTest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating water test ID: {WaterTestId}", id);
            TempData["Error"] = "An error occurred while updating the water test.";
            await PopulateTanksDropdown(_userManager.GetUserId(User)!);
            return View(waterTest);
        }
    }

    // GET: WaterTest/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var waterTest = await _waterTestService.GetWaterTestByIdAsync(id, userId);
            if (waterTest == null)
            {
                return NotFound();
            }

            return View(waterTest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving water test for deletion, ID: {WaterTestId}", id);
            TempData["Error"] = "An error occurred while retrieving the water test.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: WaterTest/Delete/5
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

            var result = await _waterTestService.DeleteWaterTestAsync(id, userId);
            if (result)
            {
                TempData["Success"] = "Water test deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete water test.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting water test ID: {WaterTestId}", id);
            TempData["Error"] = "An error occurred while deleting the water test.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task PopulateTanksDropdown(string userId)
    {
        var tanks = await _tankService.GetAllTanksAsync(userId);
        ViewBag.Tanks = new SelectList(tanks, "Id", "Name");
    }
}
