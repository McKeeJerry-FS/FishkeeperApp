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
    private readonly IImageService _imageService;

    public TankController(
        ITankService tankService,
        UserManager<AppUser> userManager,
        ILogger<TankController> logger,
        IImageService imageService)
    {
        _tankService = tankService;
        _userManager = userManager;
        _logger = logger;
        _imageService = imageService;
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
            
            // Convert image data for each tank
            var tankImages = new Dictionary<int, string>();
            foreach (var tank in tanks)
            {
                tankImages[tank.Id] = _imageService.ConvertByteArrayToFile(
                    tank.ImageData,
                    tank.ImageType,
                    Models.Enums.DefaultImage.TankImage
                ) ?? string.Empty;
            }
            ViewData["TankImages"] = tankImages;

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

            // Convert image data to displayable format
            ViewData["TankImage"] = _imageService.ConvertByteArrayToFile(
                tank.ImageData,
                tank.ImageType,
                Models.Enums.DefaultImage.TankImage
            );

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
    public async Task<IActionResult> Create([Bind("Name,Type,VolumeGallons,StartDate,Notes,ImageFile")] Tank tank)
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
                // Handle image upload
                if (tank.ImageFile != null)
                {
                    tank.ImageData = await _imageService.ConvertFileToByteArrayAsync(tank.ImageFile);
                    tank.ImageType = tank.ImageFile.ContentType;
                }

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
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,VolumeGallons,StartDate,Notes,ImageFile")] Tank tank)
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
                // Handle image upload
                if (tank.ImageFile != null)
                {
                    tank.ImageData = await _imageService.ConvertFileToByteArrayAsync(tank.ImageFile);
                    tank.ImageType = tank.ImageFile.ContentType;
                }
                else
                {
                    // Preserve existing image if no new image is uploaded
                    var existingTank = await _tankService.GetTankByIdAsync(id, userId);
                    if (existingTank != null)
                    {
                        tank.ImageData = existingTank.ImageData;
                        tank.ImageType = existingTank.ImageType;
                    }
                }

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
