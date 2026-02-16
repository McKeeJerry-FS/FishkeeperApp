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

            // Populate species enums for JavaScript
            ViewBag.FreshwaterFishTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.FreshwaterFishType));
            ViewBag.SaltwaterFishTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.SaltwaterFishType));
            ViewBag.CoralTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.CoralType));
            ViewBag.PlantTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.PlantType));
            ViewBag.FreshwaterInvertebrateTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.FreshwaterInvertebrateType));
            ViewBag.InvertebrateTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.InvertebrateType));

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
        _logger.LogInformation("Create POST called - TankId: {TankId}, LivestockType: {LivestockType}", tankId, livestockType);

        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt - no userId");
                return Unauthorized();
            }

            _logger.LogInformation("User authenticated - UserId: {UserId}", userId);

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

            // Set TankId before model binding to satisfy required field validation
            livestock.TankId = tankId;
            _logger.LogInformation("Livestock object created and TankId set: {TankId}", tankId);

            // Remove tankId and livestockType from ModelState to prevent binding conflicts
            ModelState.Remove("tankId");
            ModelState.Remove("livestockType");

            // Bind common properties
            await TryUpdateModelAsync(livestock, "",
                l => l.Name,
                l => l.Species,
                l => l.AddedOn,
                l => l.Notes);

            _logger.LogInformation("Model binding complete - Name: {Name}, Species: {Species}", livestock.Name, livestock.Species);

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

            _logger.LogInformation("About to validate ModelState. IsValid: {IsValid}", ModelState.IsValid);
            
            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is valid, creating livestock in database. TankId: {TankId}, UserId: {UserId}", tankId, userId);
                
                // Verify tank exists before calling service
                var tankExists = await _tankService.GetTankByIdAsync(tankId, userId);
                if (tankExists == null)
                {
                    _logger.LogError("Tank not found or doesn't belong to user. TankId: {TankId}, UserId: {UserId}", tankId, userId);
                    TempData["Error"] = $"Tank with ID {tankId} not found or you don't have permission to access it.";
                    await PopulateTanksDropdown(userId);
                    ViewBag.SelectedTankId = tankId;
                    return View(livestock);
                }
                _logger.LogInformation("Tank verified - Name: {TankName}, Owner: {OwnerId}", tankExists.Name, tankExists.UserId);
                
                var createdLivestock = await _livestockService.CreateLivestockAsync(livestock, tankId, userId);
                _logger.LogInformation("Livestock created successfully with ID: {Id}", createdLivestock.Id);
                TempData["Success"] = $"{livestockType} added successfully!";
                return RedirectToAction(nameof(Details), new { id = createdLivestock.Id });
            }

            // Log ModelState errors for debugging
            _logger.LogWarning("ModelState validation failed for livestock creation");
            var errorMessages = new List<string>();
            foreach (var modelState in ModelState)
            {
                foreach (var error in modelState.Value.Errors)
                {
                    var errorMsg = !string.IsNullOrEmpty(error.ErrorMessage)
                        ? error.ErrorMessage
                        : error.Exception?.Message ?? "Unknown error";
                    _logger.LogWarning("ModelState error for {Key}: {ErrorMessage}", modelState.Key, errorMsg);
                    errorMessages.Add($"{modelState.Key}: {errorMsg}");
                }
            }
            TempData["Error"] = "Validation failed: " + string.Join("; ", errorMessages);

            await PopulateTanksDropdown(userId);
            ViewBag.SelectedTankId = tankId;

            // Repopulate species enums for JavaScript
            ViewBag.FreshwaterFishTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.FreshwaterFishType));
            ViewBag.SaltwaterFishTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.SaltwaterFishType));
            ViewBag.CoralTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.CoralType));
            ViewBag.PlantTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.PlantType));
            ViewBag.FreshwaterInvertebrateTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.FreshwaterInvertebrateType));
            ViewBag.InvertebrateTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.InvertebrateType));

            return View(livestock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating livestock");
            TempData["Error"] = $"An error occurred while adding the livestock: {ex.Message}";
            await PopulateTanksDropdown(_userManager.GetUserId(User)!);

            // Repopulate species enums for JavaScript
            ViewBag.FreshwaterFishTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.FreshwaterFishType));
            ViewBag.SaltwaterFishTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.SaltwaterFishType));
            ViewBag.CoralTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.CoralType));
            ViewBag.PlantTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.PlantType));
            ViewBag.FreshwaterInvertebrateTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.FreshwaterInvertebrateType));
            ViewBag.InvertebrateTypes = Enum.GetNames(typeof(AquaHub.MVC.Models.Enums.InvertebrateType));

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
