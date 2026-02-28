
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class TankController : Controller
{
    private readonly ITankService _tankService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<TankController> _logger;
    private readonly IImageService _imageService;
    private readonly IQuarantineCareAdvisorService _quarantineCareAdvisor;

    public TankController(
        ITankService tankService,
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        ILogger<TankController> logger,
        IImageService imageService,
        IQuarantineCareAdvisorService quarantineCareAdvisor)
    {
        _tankService = tankService;
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _imageService = imageService;
        _quarantineCareAdvisor = quarantineCareAdvisor;
    }

    // POST: Tank/MarkMilestoneComplete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkMilestoneComplete(int milestoneId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var milestone = await _context.TankMilestones.Include(m => m.Tank).FirstOrDefaultAsync(m => m.Id == milestoneId && m.Tank.UserId == userId);
        if (milestone == null)
        {
            return NotFound();
        }

        milestone.IsManuallyCompleted = true;
        milestone.ManualCompletedDate = DateTime.UtcNow;
        milestone.IsCompleted = true;
        milestone.CompletedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Milestone marked as completed manually.";
        return RedirectToAction("Details", new { id = milestone.TankId });
    }

    // POST: Tank/ResetMilestone/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetMilestone(int milestoneId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var milestone = await _context.TankMilestones.Include(m => m.Tank).FirstOrDefaultAsync(m => m.Id == milestoneId && m.Tank.UserId == userId);
        if (milestone == null)
        {
            return NotFound();
        }

        milestone.IsManuallyCompleted = false;
        milestone.ManualCompletedDate = null;
        milestone.IsCompleted = false;
        milestone.CompletedDate = null;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Milestone reset to automatic tracking.";
        return RedirectToAction("Details", new { id = milestone.TankId });
    }

// ...existing code...
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to Tank Index");
                return Unauthorized();
            }

            _logger.LogInformation("Loading tanks for user {UserId}", userId);
            var tanks = await _tankService.GetAllTanksAsync(userId);
            _logger.LogInformation("Found {TankCount} tanks for user", tanks?.Count() ?? 0);

            // Convert image data for each tank
            var tankImages = new Dictionary<int, string>();
            foreach (var tank in tanks ?? Enumerable.Empty<Tank>())
            {
                try
                {
                    var imageUrl = _imageService.ConvertByteArrayToFile(
                        tank.ImageData,
                        tank.ImageType,
                        Models.Enums.DefaultImage.TankImage
                    );

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        tankImages[tank.Id] = imageUrl;
                        _logger.LogInformation("Tank {TankId} has image. Type: {ImageType}, DataLength: {DataLength}",
                            tank.Id, tank.ImageType, tank.ImageData?.Length ?? 0);
                    }
                    else
                    {
                        tankImages[tank.Id] = "/img/journal.png";
                        _logger.LogInformation("Tank {TankId} has no image data, using default image", tank.Id);
                    }
                }
                catch (Exception imgEx)
                {
                    _logger.LogError(imgEx, "Error converting image for tank {TankId}", tank.Id);
                    tankImages[tank.Id] = "/img/journal.png";
                }
            }
            ViewData["TankImages"] = tankImages;

            _logger.LogInformation("Returning Tank Index view with {ImageCount} images", tankImages.Count);
            return View(tanks ?? new List<Tank>());
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

            // Get latest water test
            var latestWaterTest = await _context.WaterTests
                .Where(w => w.TankId == id && w.Tank!.UserId == userId)
                .OrderByDescending(w => w.Timestamp)
                .FirstOrDefaultAsync();

            ViewData["LatestWaterTest"] = latestWaterTest;

            // Convert image data to displayable format
            var tankImage = _imageService.ConvertByteArrayToFile(
                tank.ImageData,
                tank.ImageType,
                Models.Enums.DefaultImage.TankImage
            );

            ViewData["TankImage"] = tankImage;

            if (!string.IsNullOrEmpty(tankImage))
            {
                _logger.LogInformation("Tank {TankId} details showing image. Type: {ImageType}, DataLength: {DataLength}",
                    id, tank.ImageType, tank.ImageData?.Length ?? 0);
            }
            else
            {
                _logger.LogInformation("Tank {TankId} details has no image data", id);
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
    public async Task<IActionResult> Create([Bind("Name,Type,VolumeGallons,StartDate,Notes,ImageFile,IsNewTank,IsQuarantineTank,QuarantineStartDate,QuarantineEndDate,QuarantinePurpose,QuarantineStatus,TreatmentProtocol")] Tank tank)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Remove UserId from ModelState since it's set by the service
            ModelState.Remove("UserId");

            // Remove quarantine-related fields from validation if not a quarantine tank
            if (!tank.IsQuarantineTank)
            {
                ModelState.Remove("QuarantinePurpose");
                ModelState.Remove("TreatmentProtocol");
                ModelState.Remove("QuarantineStatus");
                ModelState.Remove("QuarantineStartDate");
                ModelState.Remove("QuarantineEndDate");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Tank creation failed - ModelState is invalid");
                foreach (var key in ModelState.Keys)
                {
                    var modelState = ModelState[key];
                    if (modelState != null)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            _logger.LogWarning("Validation error for {Key}: {ErrorMessage}", key, error.ErrorMessage);
                        }
                    }
                }
                return View(tank);
            }

            // Handle image upload
            if (tank.ImageFile != null)
            {
                _logger.LogInformation("Processing image upload. FileName: {FileName}, ContentType: {ContentType}, Size: {Size}",
                    tank.ImageFile.FileName, tank.ImageFile.ContentType, tank.ImageFile.Length);

                tank.ImageData = await _imageService.ConvertFileToByteArrayAsync(tank.ImageFile);
                tank.ImageType = tank.ImageFile.ContentType;

                _logger.LogInformation("Image converted successfully. Data length: {Length}", tank.ImageData?.Length ?? 0);
            }

            var createdTank = await _tankService.CreateTankAsync(tank, userId);
            _logger.LogInformation("Tank created with ID: {TankId}", createdTank.Id);
            TempData["Success"] = "Tank created successfully!";
            return RedirectToAction(nameof(Details), new { id = createdTank.Id });
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
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,VolumeGallons,StartDate,Notes,ImageFile,IsNewTank,IsQuarantineTank,QuarantineStartDate,QuarantineEndDate,QuarantinePurpose,QuarantineStatus,TreatmentProtocol")] Tank tank)
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

            // Remove UserId from ModelState since it's set by the service
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (tank.ImageFile != null)
                {
                    _logger.LogInformation("Processing image upload for edit. FileName: {FileName}, ContentType: {ContentType}, Size: {Size}",
                        tank.ImageFile.FileName, tank.ImageFile.ContentType, tank.ImageFile.Length);

                    tank.ImageData = await _imageService.ConvertFileToByteArrayAsync(tank.ImageFile);
                    tank.ImageType = "image/jpeg"; // Always set to JPEG, since service always outputs JPEG

                    _logger.LogInformation("Image converted successfully. Data length: {Length}", tank.ImageData?.Length ?? 0);
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

    // GET: Tank/QuarantineDashboard/5
    public async Task<IActionResult> QuarantineDashboard(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var tank = await _context.Tanks
                .Include(t => t.Livestock)
                .Include(t => t.WaterTests)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (tank == null)
            {
                TempData["Error"] = "Tank not found.";
                return RedirectToAction(nameof(Index));
            }

            // Calculate quarantine progress
            var daysInQuarantine = tank.QuarantineStartDate.HasValue
                ? (DateTime.Now - tank.QuarantineStartDate.Value).Days
                : 0;

            var remainingDays = 0;
            var progressPercentage = 0.0;
            var isOverdue = false;

            if (tank.QuarantineEndDate.HasValue && tank.QuarantineStartDate.HasValue)
            {
                var totalDays = (tank.QuarantineEndDate.Value - tank.QuarantineStartDate.Value).Days;
                remainingDays = (tank.QuarantineEndDate.Value - DateTime.Now).Days;
                progressPercentage = totalDays > 0 ? Math.Min(100, (daysInQuarantine * 100.0 / totalDays)) : 0;
                isOverdue = remainingDays < 0;
            }

            // Get recent water tests (last 14 days for quarantine monitoring)
            var recentWaterTests = tank.WaterTests
                .Where(wt => wt.Timestamp >= DateTime.Now.AddDays(-14))
                .OrderByDescending(wt => wt.Timestamp)
                .ToList();

            var latestTest = recentWaterTests.FirstOrDefault();

            // Check for critical water parameters
            var waterQualityAlerts = new List<string>();
            var hasCriticalParameters = false;

            if (latestTest != null)
            {
                if (latestTest.Ammonia > 0.25)
                {
                    waterQualityAlerts.Add($"Ammonia is elevated at {latestTest.Ammonia} ppm (should be 0)");
                    hasCriticalParameters = true;
                }
                if (latestTest.Nitrite > 0.25)
                {
                    waterQualityAlerts.Add($"Nitrite is elevated at {latestTest.Nitrite} ppm (should be 0)");
                    hasCriticalParameters = true;
                }
                if (latestTest.Nitrate > 40)
                {
                    waterQualityAlerts.Add($"Nitrate is high at {latestTest.Nitrate} ppm (recommended < 40)");
                }
                if (latestTest.Temperature < 72 || latestTest.Temperature > 82)
                {
                    waterQualityAlerts.Add($"Temperature is {latestTest.Temperature}°F (recommended 74-80°F)");
                }
            }
            else
            {
                waterQualityAlerts.Add("No recent water tests - testing recommended");
            }

            // Get recent dosing records
            var recentDosing = await _context.DosingRecords
                .Where(d => d.TankId == id && d.Timestamp >= DateTime.Now.AddDays(-14))
                .OrderByDescending(d => d.Timestamp)
                .Take(20)
                .ToListAsync();

            // Get feeding schedules
            var feedingSchedules = await _context.FeedingSchedules
                .Where(fs => fs.TankId == id)
                .ToListAsync();

            // Get recent feeding records
            var recentFeedings = await _context.FeedingRecords
                .Where(fr => fr.TankId == id && fr.FedDateTime >= DateTime.Now.AddDays(-7))
                .OrderByDescending(fr => fr.FedDateTime)
                .Take(20)
                .ToListAsync();

            // Get recent maintenance
            var recentMaintenance = await _context.MaintenanceLogs
                .Where(ml => ml.TankId == id && ml.Timestamp >= DateTime.Now.AddDays(-14))
                .OrderByDescending(ml => ml.Timestamp)
                .Take(10)
                .ToListAsync();

            var lastWaterChange = recentMaintenance
                .FirstOrDefault(ml => ml.Type == MaintenanceType.WaterChange);

            var daysSinceWaterChange = lastWaterChange != null
                ? (DateTime.Now - lastWaterChange.Timestamp).Days
                : 999;

            // Prepare chart data
            var chartData = recentWaterTests.OrderBy(wt => wt.Timestamp).ToList();
            var chartLabels = chartData.Select(wt => wt.Timestamp.ToString("MM/dd")).ToList();

            // Generate AI-powered care recommendations
            var aiRecommendations = await _quarantineCareAdvisor.AnalyzeQuarantineConditionsAsync(
                tank,
                recentWaterTests,
                recentDosing,
                recentMaintenance,
                tank.Livestock.ToList());

            var viewModel = new Models.ViewModels.QuarantineDashboardViewModel
            {
                Tank = tank,
                DaysInQuarantine = daysInQuarantine,
                RemainingDays = remainingDays,
                ProgressPercentage = progressPercentage,
                IsOverdue = isOverdue,
                QuarantinedLivestock = tank.Livestock.ToList(),
                LatestWaterTest = latestTest,
                RecentWaterTests = recentWaterTests,
                HasCriticalParameters = hasCriticalParameters,
                WaterQualityAlerts = waterQualityAlerts,
                ChartLabels = chartLabels,
                PHData = chartData.Select(wt => wt.PH).ToList(),
                TemperatureData = chartData.Select(wt => wt.Temperature).ToList(),
                AmmoniaData = chartData.Select(wt => wt.Ammonia).ToList(),
                NitriteData = chartData.Select(wt => wt.Nitrite).ToList(),
                NitrateData = chartData.Select(wt => wt.Nitrate).ToList(),
                SalinityData = chartData.Select(wt => wt.Salinity).ToList(),
                ActiveTreatments = recentDosing.Where(d => d.Timestamp >= DateTime.Now.AddDays(-7)).ToList(),
                RecentDosing = recentDosing,
                FeedingSchedules = feedingSchedules,
                RecentFeedings = recentFeedings,
                RecentMaintenance = recentMaintenance,
                LastWaterChange = lastWaterChange,
                DaysSinceWaterChange = daysSinceWaterChange,
                NeedsWaterTest = !recentWaterTests.Any() || recentWaterTests.First().Timestamp < DateTime.Now.AddDays(-3),
                NeedsWaterChange = daysSinceWaterChange > 7,
                HasMissedDosing = recentDosing.Any(d => d.Timestamp < DateTime.Now.AddDays(-1)),
                AIRecommendations = aiRecommendations
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quarantine dashboard for tank ID: {TankId}", id);
            TempData["Error"] = "An error occurred while loading the quarantine dashboard.";
            return RedirectToAction(nameof(Index));
        }
    }
}
