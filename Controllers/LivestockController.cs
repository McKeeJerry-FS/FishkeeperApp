using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class LivestockController : Controller
{
    private readonly ILivestockService _livestockService;
    private readonly ITankService _tankService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<LivestockController> _logger;

    public LivestockController(
        ILivestockService livestockService,
        ITankService tankService,
        UserManager<AppUser> userManager,
        ILogger<LivestockController> logger)
    {
        _livestockService = livestockService;
        _tankService = tankService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Livestock
    public async Task<IActionResult> Index(int? tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            List<Livestock> livestock;
            if (tankId.HasValue)
            {
                livestock = await _livestockService.GetLivestockByTankAsync(tankId.Value, userId);
                var tank = await _tankService.GetTankByIdAsync(tankId.Value, userId);
                ViewBag.TankName = tank?.Name;
                ViewBag.TankId = tankId.Value;
            }
            else
            {
                livestock = await _livestockService.GetAllLivestockAsync(userId);
            }

            return View(livestock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving livestock");
            TempData["Error"] = "An error occurred while retrieving livestock.";
            return View(new List<Livestock>());
        }
    }

    // GET: Livestock/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var livestock = await _livestockService.GetLivestockByIdAsync(id, userId);
            if (livestock == null)
            {
                return NotFound();
            }

            return View(livestock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving livestock details for ID: {LivestockId}", id);
            TempData["Error"] = "An error occurred while retrieving livestock details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Livestock/Dashboard/5
    public async Task<IActionResult> Dashboard(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var dashboard = await _livestockService.GetLivestockDashboardAsync(id, userId);
            if (dashboard == null)
            {
                return NotFound();
            }

            return View(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving livestock dashboard for ID: {LivestockId}", id);
            TempData["Error"] = "An error occurred while retrieving livestock dashboard.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Livestock/Create
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

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create livestock view");
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Livestock/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int tankId, string livestockType)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Create the appropriate livestock type based on selection
            Livestock livestock = livestockType switch
            {
                "FreshwaterFish" => new FreshwaterFish(),
                "SaltwaterFish" => new SaltwaterFish(),
                "Coral" => new Coral(),
                "Plant" => new Plant(),
                "FreshwaterInvertebrate" => new FreshwaterInvertebrate(),
                "SaltwaterInvertebrate" => new SaltwaterInvertebrate(),
                _ => throw new ArgumentException("Invalid livestock type")
            };

            // Bind common properties
            await TryUpdateModelAsync(livestock, "",
                l => l.Name,
                l => l.Species,
                l => l.AddedOn,
                l => l.Notes);

            // Bind type-specific properties
            switch (livestock)
            {
                case FreshwaterFish ff:
                    await TryUpdateModelAsync(ff, "",
                        f => f.AdultSize,
                        f => f.Coloration,
                        f => f.Temperament,
                        f => f.ActivityLevel,
                        f => f.MinTankSize,
                        f => f.SwimmingRegion,
                        f => f.IsSchooling,
                        f => f.RecommendedSchoolSize);
                    break;

                case SaltwaterFish sf:
                    await TryUpdateModelAsync(sf, "",
                        f => f.AdultSize,
                        f => f.Coloration,
                        f => f.Temperament,
                        f => f.ActivityLevel,
                        f => f.MinTankSize,
                        f => f.SwimmingRegion,
                        f => f.IsSchooling,
                        f => f.RecommendedSchoolSize,
                        f => f.IsReefSafe);
                    break;

                case Coral coral:
                    await TryUpdateModelAsync(coral, "",
                        c => c.ColonySize,
                        c => c.Coloration,
                        c => c.LightingNeeds,
                        c => c.FlowNeeds,
                        c => c.Placement,
                        c => c.GrowthRate,
                        c => c.CareLevel);
                    break;

                case Plant plant:
                    await TryUpdateModelAsync(plant, "",
                        p => p.MaxHeight,
                        p => p.GrowthRate,
                        p => p.Coloration,
                        p => p.Placement,
                        p => p.LightingRequirement,
                        p => p.CareLevel,
                        p => p.RequiresCO2);
                    break;

                case FreshwaterInvertebrate fi:
                    await TryUpdateModelAsync(fi, "",
                        i => i.AdultSize,
                        i => i.Coloration,
                        i => i.Behavior,
                        i => i.MinTankSize,
                        i => i.IsPlantSafe);
                    break;

                case SaltwaterInvertebrate si:
                    await TryUpdateModelAsync(si, "",
                        i => i.AdultSize,
                        i => i.Coloration,
                        i => i.Behavior,
                        i => i.MinTankSize,
                        i => i.IsReefSafe);
                    break;
            }

            if (ModelState.IsValid)
            {
                var createdLivestock = await _livestockService.CreateLivestockAsync(livestock, tankId, userId);
                TempData["Success"] = $"{livestockType} added successfully!";
                return RedirectToAction(nameof(Details), new { id = createdLivestock.Id });
            }

            await PopulateTanksDropdown(userId);
            ViewBag.SelectedTankId = tankId;
            return View(livestock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating livestock");
            TempData["Error"] = "An error occurred while adding the livestock.";
            await PopulateTanksDropdown(_userManager.GetUserId(User)!);
            return View();
        }
    }

    // GET: Livestock/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var livestock = await _livestockService.GetLivestockByIdAsync(id, userId);
            if (livestock == null)
            {
                return NotFound();
            }

            await PopulateTanksDropdown(userId);
            return View(livestock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving livestock for edit, ID: {LivestockId}", id);
            TempData["Error"] = "An error occurred while retrieving the livestock.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Livestock/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Species,DateAdded,PurchasePrice,Notes,IsAlive,ImageURL")] Livestock livestock)
    {
        if (id != livestock.Id)
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
                await _livestockService.UpdateLivestockAsync(livestock, userId);
                TempData["Success"] = "Livestock updated successfully!";
                return RedirectToAction(nameof(Details), new { id = livestock.Id });
            }

            await PopulateTanksDropdown(userId);
            return View(livestock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating livestock ID: {LivestockId}", id);
            TempData["Error"] = "An error occurred while updating the livestock.";
            await PopulateTanksDropdown(_userManager.GetUserId(User)!);
            return View(livestock);
        }
    }

    // GET: Livestock/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var livestock = await _livestockService.GetLivestockByIdAsync(id, userId);
            if (livestock == null)
            {
                return NotFound();
            }

            return View(livestock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving livestock for deletion, ID: {LivestockId}", id);
            TempData["Error"] = "An error occurred while retrieving the livestock.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Livestock/Delete/5
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

            var result = await _livestockService.DeleteLivestockAsync(id, userId);
            if (result)
            {
                TempData["Success"] = "Livestock deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete livestock.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting livestock ID: {LivestockId}", id);
            TempData["Error"] = "An error occurred while deleting the livestock.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task PopulateTanksDropdown(string userId)
    {
        var tanks = await _tankService.GetAllTanksAsync(userId);
        ViewBag.Tanks = new SelectList(tanks, "Id", "Name");
    }
}
