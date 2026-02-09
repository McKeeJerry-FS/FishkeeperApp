using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class BreedingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ITankService _tankService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IImageService _imageService;
    private readonly IBreedingWaterAnalysisService _waterAnalysisService;
    private readonly ILogger<BreedingController> _logger;

    public BreedingController(
        ApplicationDbContext context,
        ITankService tankService,
        UserManager<AppUser> userManager,
        IImageService imageService,
        IBreedingWaterAnalysisService waterAnalysisService,
        ILogger<BreedingController> logger)
    {
        _context = context;
        _tankService = tankService;
        _userManager = userManager;
        _imageService = imageService;
        _waterAnalysisService = waterAnalysisService;
        _logger = logger;
    }

    // GET: Breeding
    public async Task<IActionResult> Index(int? tankId, bool? activeOnly)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            IQueryable<BreedingPair> query = _context.BreedingPairs
                .Include(b => b.Tank)
                .Include(b => b.Parent1)
                .Include(b => b.Parent2)
                .Include(b => b.BreedingAttempts)
                .Where(b => b.UserId == userId);

            if (tankId.HasValue)
            {
                query = query.Where(b => b.TankId == tankId.Value);
                var tank = await _tankService.GetTankByIdAsync(tankId.Value, userId);
                ViewBag.TankName = tank?.Name;
                ViewBag.TankId = tankId.Value;
            }

            if (activeOnly == true)
            {
                query = query.Where(b => b.IsActive);
            }

            var pairs = await query.OrderByDescending(b => b.DatePaired).ToListAsync();

            // Get tanks for filter dropdown
            var tanks = await _context.Tanks
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewBag.Tanks = new SelectList(tanks, "Id", "Name");
            ViewBag.ActiveOnly = activeOnly ?? false;

            return View(pairs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving breeding pairs");
            TempData["Error"] = "An error occurred while retrieving breeding pairs.";
            ViewBag.Tanks = new SelectList(Enumerable.Empty<Tank>(), "Id", "Name");
            ViewBag.ActiveOnly = false;
            return View(new List<BreedingPair>());
        }
    }

    // GET: Breeding/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var pair = await _context.BreedingPairs
                .Include(b => b.Tank)
                .Include(b => b.Parent1)
                .Include(b => b.Parent2)
                .Include(b => b.BreedingAttempts)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (pair == null)
            {
                return NotFound();
            }

            return View(pair);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving breeding pair details for ID: {PairId}", id);
            TempData["Error"] = "An error occurred while retrieving breeding pair details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Breeding/WaterConditions/5
    public async Task<IActionResult> WaterConditions(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var pair = await _context.BreedingPairs
                .Include(b => b.Tank)
                .Include(b => b.Parent1)
                .Include(b => b.Parent2)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (pair == null)
            {
                return NotFound();
            }

            // Get the latest water test for this tank
            var latestWaterTest = await _context.WaterTests
                .Where(w => w.TankId == pair.TankId)
                .OrderByDescending(w => w.Timestamp)
                .FirstOrDefaultAsync();

            // Analyze water conditions
            var viewModel = _waterAnalysisService.AnalyzeWaterConditions(pair, latestWaterTest);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing water conditions for breeding pair ID: {PairId}", id);
            TempData["Error"] = "An error occurred while analyzing water conditions.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // GET: Breeding/Create
    public async Task<IActionResult> Create(int? tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await PopulateDropdownsAsync(userId, tankId);

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create breeding pair view");
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Breeding/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BreedingPair pair, IFormFile? imageFile)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(userId, pair.TankId);
                return View(pair);
            }

            // Verify tank belongs to user
            var tank = await _tankService.GetTankByIdAsync(pair.TankId, userId);
            if (tank == null)
            {
                TempData["Error"] = "Invalid tank selected.";
                await PopulateDropdownsAsync(userId, pair.TankId);
                return View(pair);
            }

            pair.UserId = userId;
            pair.CreatedDate = DateTime.Now;

            // Handle image upload
            if (imageFile != null)
            {
                var imageData = await _imageService.ConvertFileToByteArrayAsync(imageFile);
                var extension = Path.GetExtension(imageFile.FileName);
                var imagePath = _imageService.ConvertByteArrayToFile(imageData, extension, Models.Enums.DefaultImage.LivestockImage);
                pair.ImagePath = imagePath;
            }

            _context.BreedingPairs.Add(pair);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Breeding pair '{pair.PairName}' created successfully!";
            return RedirectToAction(nameof(Details), new { id = pair.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating breeding pair");
            TempData["Error"] = "An error occurred while creating the breeding pair.";
            await PopulateDropdownsAsync(_userManager.GetUserId(User)!, pair.TankId);
            return View(pair);
        }
    }

    // GET: Breeding/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var pair = await _context.BreedingPairs
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (pair == null)
            {
                return NotFound();
            }

            await PopulateDropdownsAsync(userId, pair.TankId, pair.Parent1Id, pair.Parent2Id);

            return View(pair);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit breeding pair view for ID: {PairId}", id);
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Breeding/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BreedingPair pair, IFormFile? imageFile)
    {
        try
        {
            if (id != pair.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var existingPair = await _context.BreedingPairs
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (existingPair == null)
            {
                return NotFound();
            }

            ModelState.Remove("User");
            ModelState.Remove("Tank");
            ModelState.Remove("Parent1");
            ModelState.Remove("Parent2");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(userId, pair.TankId, pair.Parent1Id, pair.Parent2Id);
                return View(pair);
            }

            // Update properties
            existingPair.TankId = pair.TankId;
            existingPair.PairName = pair.PairName;
            existingPair.Parent1Id = pair.Parent1Id;
            existingPair.Parent2Id = pair.Parent2Id;
            existingPair.Species = pair.Species;
            existingPair.BreedingType = pair.BreedingType;
            existingPair.IsActive = pair.IsActive;
            existingPair.DatePaired = pair.DatePaired;
            existingPair.BreedingSetup = pair.BreedingSetup;
            existingPair.WaterConditions = pair.WaterConditions;
            existingPair.DietConditioning = pair.DietConditioning;
            existingPair.BehaviorNotes = pair.BehaviorNotes;
            existingPair.Notes = pair.Notes;
            existingPair.LastUpdated = DateTime.Now;

            // Handle image upload
            if (imageFile != null)
            {
                var imageData = await _imageService.ConvertFileToByteArrayAsync(imageFile);
                var extension = Path.GetExtension(imageFile.FileName);
                var imagePath = _imageService.ConvertByteArrayToFile(imageData, extension, Models.Enums.DefaultImage.LivestockImage);
                existingPair.ImagePath = imagePath;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Breeding pair '{pair.PairName}' updated successfully!";
            return RedirectToAction(nameof(Details), new { id = pair.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating breeding pair ID: {PairId}", id);
            TempData["Error"] = "An error occurred while updating the breeding pair.";
            await PopulateDropdownsAsync(_userManager.GetUserId(User)!, pair.TankId, pair.Parent1Id, pair.Parent2Id);
            return View(pair);
        }
    }

    // GET: Breeding/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var pair = await _context.BreedingPairs
                .Include(b => b.Tank)
                .Include(b => b.Parent1)
                .Include(b => b.Parent2)
                .Include(b => b.BreedingAttempts)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (pair == null)
            {
                return NotFound();
            }

            return View(pair);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading delete breeding pair view for ID: {PairId}", id);
            TempData["Error"] = "An error occurred while loading the delete confirmation.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Breeding/Delete/5
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

            var pair = await _context.BreedingPairs
                .Include(b => b.BreedingAttempts)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (pair == null)
            {
                return NotFound();
            }

            _context.BreedingPairs.Remove(pair);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Breeding pair '{pair.PairName}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting breeding pair ID: {PairId}", id);
            TempData["Error"] = "An error occurred while deleting the breeding pair.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Breeding/AddAttempt/5
    public async Task<IActionResult> AddAttempt(int pairId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var pair = await _context.BreedingPairs
                .Include(b => b.Tank)
                .Include(b => b.Parent1)
                .Include(b => b.Parent2)
                .FirstOrDefaultAsync(b => b.Id == pairId && b.UserId == userId);

            if (pair == null)
            {
                return NotFound();
            }

            ViewBag.BreedingPair = pair;
            ViewBag.StatusOptions = GetStatusOptions();

            var attempt = new BreedingAttempt
            {
                BreedingPairId = pairId,
                UserId = userId
            };

            return View(attempt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading add attempt view for pair ID: {PairId}", pairId);
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Details), new { id = pairId });
        }
    }

    // POST: Breeding/AddAttempt
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAttempt(BreedingAttempt attempt, IFormFile? imageFile)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            ModelState.Remove("User");
            ModelState.Remove("BreedingPair");

            if (!ModelState.IsValid)
            {
                var pair = await _context.BreedingPairs
                    .Include(b => b.Tank)
                    .Include(b => b.Parent1)
                    .Include(b => b.Parent2)
                    .FirstOrDefaultAsync(b => b.Id == attempt.BreedingPairId);
                ViewBag.BreedingPair = pair;
                ViewBag.StatusOptions = GetStatusOptions();
                return View(attempt);
            }

            attempt.UserId = userId;
            attempt.CreatedDate = DateTime.Now;

            // Handle image upload
            if (imageFile != null)
            {
                var imageData = await _imageService.ConvertFileToByteArrayAsync(imageFile);
                var extension = Path.GetExtension(imageFile.FileName);
                var imagePath = _imageService.ConvertByteArrayToFile(imageData, extension, Models.Enums.DefaultImage.LivestockImage);
                attempt.ImagePath = imagePath;
            }

            _context.BreedingAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Breeding attempt added successfully!";
            return RedirectToAction(nameof(Details), new { id = attempt.BreedingPairId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding breeding attempt");
            TempData["Error"] = "An error occurred while adding the breeding attempt.";
            return RedirectToAction(nameof(Details), new { id = attempt.BreedingPairId });
        }
    }

    // GET: Breeding/EditAttempt/5
    public async Task<IActionResult> EditAttempt(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var attempt = await _context.BreedingAttempts
                .Include(a => a.BreedingPair)
                    .ThenInclude(b => b.Tank)
                .Include(a => a.BreedingPair.Parent1)
                .Include(a => a.BreedingPair.Parent2)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (attempt == null)
            {
                return NotFound();
            }

            ViewBag.BreedingPair = attempt.BreedingPair;
            ViewBag.StatusOptions = GetStatusOptions();

            return View(attempt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit attempt view for ID: {AttemptId}", id);
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Breeding/EditAttempt/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAttempt(int id, BreedingAttempt attempt, IFormFile? imageFile)
    {
        try
        {
            if (id != attempt.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var existingAttempt = await _context.BreedingAttempts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (existingAttempt == null)
            {
                return NotFound();
            }

            ModelState.Remove("User");
            ModelState.Remove("BreedingPair");

            if (!ModelState.IsValid)
            {
                var pair = await _context.BreedingPairs
                    .Include(b => b.Tank)
                    .Include(b => b.Parent1)
                    .Include(b => b.Parent2)
                    .FirstOrDefaultAsync(b => b.Id == attempt.BreedingPairId);
                ViewBag.BreedingPair = pair;
                ViewBag.StatusOptions = GetStatusOptions();
                return View(attempt);
            }

            // Update properties
            existingAttempt.AttemptDate = attempt.AttemptDate;
            existingAttempt.Status = attempt.Status;
            existingAttempt.SpawningObserved = attempt.SpawningObserved;
            existingAttempt.SpawningDateTime = attempt.SpawningDateTime;
            existingAttempt.EggsLaid = attempt.EggsLaid;
            existingAttempt.EggsFertilized = attempt.EggsFertilized;
            existingAttempt.HatchingDate = attempt.HatchingDate;
            existingAttempt.NumberHatched = attempt.NumberHatched;
            existingAttempt.OffspringCount = attempt.OffspringCount;
            existingAttempt.SurvivalRatePercent = attempt.SurvivalRatePercent;
            existingAttempt.CurrentSurvivors = attempt.CurrentSurvivors;
            existingAttempt.WaterTemperature = attempt.WaterTemperature;
            existingAttempt.PhLevel = attempt.PhLevel;
            existingAttempt.Hardness = attempt.Hardness;
            existingAttempt.BreedingTrigger = attempt.BreedingTrigger;
            existingAttempt.BehaviorObservations = attempt.BehaviorObservations;
            existingAttempt.ParentalCareNotes = attempt.ParentalCareNotes;
            existingAttempt.FeedingRegimen = attempt.FeedingRegimen;
            existingAttempt.Challenges = attempt.Challenges;
            existingAttempt.Notes = attempt.Notes;
            existingAttempt.LastUpdated = DateTime.Now;

            // Handle image upload
            if (imageFile != null)
            {
                var imageData = await _imageService.ConvertFileToByteArrayAsync(imageFile);
                var extension = Path.GetExtension(imageFile.FileName);
                var imagePath = _imageService.ConvertByteArrayToFile(imageData, extension, Models.Enums.DefaultImage.LivestockImage);
                existingAttempt.ImagePath = imagePath;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Breeding attempt updated successfully!";
            return RedirectToAction(nameof(Details), new { id = attempt.BreedingPairId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating breeding attempt ID: {AttemptId}", id);
            TempData["Error"] = "An error occurred while updating the breeding attempt.";
            return RedirectToAction(nameof(Details), new { id = attempt.BreedingPairId });
        }
    }

    // POST: Breeding/DeleteAttempt/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttempt(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var attempt = await _context.BreedingAttempts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (attempt == null)
            {
                return NotFound();
            }

            var pairId = attempt.BreedingPairId;

            _context.BreedingAttempts.Remove(attempt);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Breeding attempt deleted successfully.";
            return RedirectToAction(nameof(Details), new { id = pairId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting breeding attempt ID: {AttemptId}", id);
            TempData["Error"] = "An error occurred while deleting the breeding attempt.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task PopulateDropdownsAsync(string userId, int? selectedTankId = null, int? selectedParent1Id = null, int? selectedParent2Id = null)
    {
        var tanks = await _context.Tanks
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync();

        ViewBag.TankId = new SelectList(tanks, "Id", "Name", selectedTankId);

        // Get all livestock for parent selection
        var livestock = await _context.Livestock
            .Include(l => l.Tank)
            .Where(l => tanks.Select(t => t.Id).Contains(l.TankId))
            .OrderBy(l => l.Name)
            .ToListAsync();

        ViewBag.Parent1Id = new SelectList(
            livestock.Select(l => new
            {
                l.Id,
                DisplayName = $"{l.Name} ({l.Species}) - {l.Tank?.Name}"
            }),
            "Id",
            "DisplayName",
            selectedParent1Id
        );

        ViewBag.Parent2Id = new SelectList(
            livestock.Select(l => new
            {
                l.Id,
                DisplayName = $"{l.Name} ({l.Species}) - {l.Tank?.Name}"
            }),
            "Id",
            "DisplayName",
            selectedParent2Id
        );

        ViewBag.BreedingTypes = new List<string>
        {
            "Egg Layer",
            "Live Bearer",
            "Bubble Nester",
            "Substrate Spawner",
            "Mouth Brooder",
            "Scatter Spawner",
            "Cave Spawner",
            "Other"
        };
    }

    private List<string> GetStatusOptions()
    {
        return new List<string>
        {
            "In Progress",
            "Eggs Laid",
            "Fertilized",
            "Hatching",
            "Fry Born",
            "Successful",
            "Failed",
            "Abandoned"
        };
    }
}
