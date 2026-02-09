using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class FraggingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ITankService _tankService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IImageService _imageService;
    private readonly ILogger<FraggingController> _logger;

    public FraggingController(
        ApplicationDbContext context,
        ITankService tankService,
        UserManager<AppUser> userManager,
        IImageService imageService,
        ILogger<FraggingController> logger)
    {
        _context = context;
        _tankService = tankService;
        _userManager = userManager;
        _imageService = imageService;
        _logger = logger;
    }

    // Check if user has any saltwater/reef tanks
    private async Task<bool> HasReefTanksAsync(string userId)
    {
        var tanks = await _tankService.GetAllTanksAsync(userId);
        return tanks.Any(t => t.Type == AquariumType.Saltwater ||
                             t.Type == AquariumType.Reef ||
                             t.Type == AquariumType.NanoReef);
    }

    // GET: Fragging
    public async Task<IActionResult> Index(int? tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Check if user has reef tanks
            if (!await HasReefTanksAsync(userId))
            {
                TempData["Info"] = "Coral fragging is only available for Saltwater, Reef, or Nano-Reef tanks. Please create one first.";
                return RedirectToAction("Index", "Tank");
            }

            IQueryable<CoralFrag> query = _context.CoralFrags
                .Include(f => f.Tank)
                .Include(f => f.ParentCoral)
                .Where(f => f.UserId == userId);

            if (tankId.HasValue)
            {
                query = query.Where(f => f.TankId == tankId.Value);
                var tank = await _tankService.GetTankByIdAsync(tankId.Value, userId);
                ViewBag.TankName = tank?.Name;
                ViewBag.TankId = tankId.Value;
            }

            var frags = await query.OrderByDescending(f => f.FragDate).ToListAsync();

            // Get tanks for filter dropdown
            var reefTanks = await _context.Tanks
                .Where(t => t.UserId == userId &&
                           (t.Type == AquariumType.Saltwater ||
                            t.Type == AquariumType.Reef ||
                            t.Type == AquariumType.NanoReef))
                .ToListAsync();

            ViewBag.Tanks = new SelectList(reefTanks, "Id", "Name");

            return View(frags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving coral frags");
            TempData["Error"] = "An error occurred while retrieving coral frags.";
            return View(new List<CoralFrag>());
        }
    }

    // GET: Fragging/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var frag = await _context.CoralFrags
                .Include(f => f.Tank)
                .Include(f => f.ParentCoral)
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (frag == null)
            {
                return NotFound();
            }

            return View(frag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving frag details for ID: {FragId}", id);
            TempData["Error"] = "An error occurred while retrieving frag details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Fragging/Create
    public async Task<IActionResult> Create(int? tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!await HasReefTanksAsync(userId))
            {
                TempData["Info"] = "Coral fragging is only available for Saltwater, Reef, or Nano-Reef tanks. Please create one first.";
                return RedirectToAction("Index", "Tank");
            }

            await PopulateDropdownsAsync(userId, tankId);

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create frag view");
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Fragging/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CoralFrag frag, IFormFile? imageFile)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Remove User from ModelState validation since it's set here
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(userId, frag.TankId);
                return View(frag);
            }

            // Verify tank belongs to user and is a reef tank
            var tank = await _tankService.GetTankByIdAsync(frag.TankId, userId);
            if (tank == null || !(tank.Type == AquariumType.Saltwater ||
                                 tank.Type == AquariumType.Reef ||
                                 tank.Type == AquariumType.NanoReef))
            {
                TempData["Error"] = "Invalid tank selected. Please select a Saltwater, Reef, or Nano-Reef tank.";
                await PopulateDropdownsAsync(userId, frag.TankId);
                return View(frag);
            }

            frag.UserId = userId;
            frag.CreatedDate = DateTime.Now;

            // Handle image upload
            if (imageFile != null)
            {
                var imageData = await _imageService.ConvertFileToByteArrayAsync(imageFile);
                var extension = Path.GetExtension(imageFile.FileName);
                var imagePath = _imageService.ConvertByteArrayToFile(imageData, extension, Models.Enums.DefaultImage.LivestockImage);
                frag.ImagePath = imagePath;
            }

            _context.CoralFrags.Add(frag);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Coral frag '{frag.FragName}' created successfully!";
            return RedirectToAction(nameof(Index), new { tankId = frag.TankId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating coral frag");
            TempData["Error"] = "An error occurred while creating the coral frag.";
            await PopulateDropdownsAsync(_userManager.GetUserId(User)!, frag.TankId);
            return View(frag);
        }
    }

    // GET: Fragging/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var frag = await _context.CoralFrags
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (frag == null)
            {
                return NotFound();
            }

            await PopulateDropdownsAsync(userId, frag.TankId, frag.ParentCoralId);

            return View(frag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit frag view for ID: {FragId}", id);
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Fragging/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CoralFrag frag, IFormFile? imageFile)
    {
        try
        {
            if (id != frag.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var existingFrag = await _context.CoralFrags
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (existingFrag == null)
            {
                return NotFound();
            }

            // Remove validation for navigation properties
            ModelState.Remove("User");
            ModelState.Remove("Tank");
            ModelState.Remove("ParentCoral");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(userId, frag.TankId, frag.ParentCoralId);
                return View(frag);
            }

            // Verify tank
            var tank = await _tankService.GetTankByIdAsync(frag.TankId, userId);
            if (tank == null || !(tank.Type == AquariumType.Saltwater ||
                                 tank.Type == AquariumType.Reef ||
                                 tank.Type == AquariumType.NanoReef))
            {
                TempData["Error"] = "Invalid tank selected.";
                await PopulateDropdownsAsync(userId, frag.TankId, frag.ParentCoralId);
                return View(frag);
            }

            // Update properties
            existingFrag.TankId = frag.TankId;
            existingFrag.ParentCoralId = frag.ParentCoralId;
            existingFrag.FragDate = frag.FragDate;
            existingFrag.FragName = frag.FragName;
            existingFrag.CoralSpecies = frag.CoralSpecies;
            existingFrag.InitialSize = frag.InitialSize;
            existingFrag.NumberOfPolyps = frag.NumberOfPolyps;
            existingFrag.Coloration = frag.Coloration;
            existingFrag.FraggingMethod = frag.FraggingMethod;
            existingFrag.FraggingNotes = frag.FraggingNotes;
            existingFrag.IsEncrusted = frag.IsEncrusted;
            existingFrag.EncrustmentDate = frag.EncrustmentDate;
            existingFrag.IsReadyForSale = frag.IsReadyForSale;
            existingFrag.SaleDate = frag.SaleDate;
            existingFrag.SalePrice = frag.SalePrice;
            existingFrag.SoldTo = frag.SoldTo;
            existingFrag.Status = frag.Status;
            existingFrag.CurrentSize = frag.CurrentSize;
            existingFrag.LastMeasurementDate = frag.LastMeasurementDate;
            existingFrag.LocationInTank = frag.LocationInTank;
            existingFrag.MountedOn = frag.MountedOn;
            existingFrag.Notes = frag.Notes;
            existingFrag.LastUpdated = DateTime.Now;

            // Handle image upload
            if (imageFile != null)
            {
                var imageData = await _imageService.ConvertFileToByteArrayAsync(imageFile);
                var extension = Path.GetExtension(imageFile.FileName);
                var imagePath = _imageService.ConvertByteArrayToFile(imageData, extension, Models.Enums.DefaultImage.LivestockImage);
                existingFrag.ImagePath = imagePath;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Coral frag '{frag.FragName}' updated successfully!";
            return RedirectToAction(nameof(Details), new { id = frag.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating coral frag ID: {FragId}", id);
            TempData["Error"] = "An error occurred while updating the coral frag.";
            await PopulateDropdownsAsync(_userManager.GetUserId(User)!, frag.TankId, frag.ParentCoralId);
            return View(frag);
        }
    }

    // GET: Fragging/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var frag = await _context.CoralFrags
                .Include(f => f.Tank)
                .Include(f => f.ParentCoral)
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (frag == null)
            {
                return NotFound();
            }

            return View(frag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading delete frag view for ID: {FragId}", id);
            TempData["Error"] = "An error occurred while loading the delete confirmation.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Fragging/Delete/5
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

            var frag = await _context.CoralFrags
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (frag == null)
            {
                return NotFound();
            }

            var tankId = frag.TankId;

            _context.CoralFrags.Remove(frag);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Coral frag '{frag.FragName}' deleted successfully.";
            return RedirectToAction(nameof(Index), new { tankId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting coral frag ID: {FragId}", id);
            TempData["Error"] = "An error occurred while deleting the coral frag.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task PopulateDropdownsAsync(string userId, int? selectedTankId = null, int? selectedCoralId = null)
    {
        // Get only reef-compatible tanks
        var reefTanks = await _context.Tanks
            .Where(t => t.UserId == userId &&
                       (t.Type == AquariumType.Saltwater ||
                        t.Type == AquariumType.Reef ||
                        t.Type == AquariumType.NanoReef))
            .OrderBy(t => t.Name)
            .ToListAsync();

        ViewBag.TankId = new SelectList(reefTanks, "Id", "Name", selectedTankId);

        // Get corals from the selected tank or all reef tanks if no tank is selected
        var tankIds = reefTanks.Select(t => t.Id).ToList();
        var corals = await _context.Corals
            .Include(c => c.Tank)
            .Where(c => tankIds.Contains(c.TankId))
            .ToListAsync();

        ViewBag.ParentCoralId = new SelectList(
            corals.Select(c => new
            {
                c.Id,
                DisplayName = $"{c.Name} ({c.Tank?.Name})"
            }),
            "Id",
            "DisplayName",
            selectedCoralId
        );

        // Populate status options
        ViewBag.StatusOptions = new List<string>
        {
            "Growing",
            "Encrusted",
            "Ready",
            "Sold",
            "Lost",
            "Traded"
        };

        // Populate fragging methods
        ViewBag.FraggingMethods = new List<string>
        {
            "Saw",
            "Bone Cutters",
            "Break",
            "Laser",
            "Scalpel",
            "Twist/Snap",
            "Other"
        };

        // Populate mounting options
        ViewBag.MountingOptions = new List<string>
        {
            "Frag Plug",
            "Rock",
            "Disc",
            "Tile",
            "Magnetic Mount",
            "Other"
        };
    }
}
