using System;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.ViewModels;
using AquaHub.MVC.Services.Interfaces;
using AquaHub.MVC.Utilities;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class TankService : ITankService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<TankService> _logger;

    public TankService(
        ApplicationDbContext context,
        IFileUploadService fileUploadService,
        ILogger<TankService> logger)
    {
        _context = context;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<List<Tank>> GetAllTanksAsync(string userId)
    {
        return await _logger.LogExecutionTimeAsync(
            "GetAllTanks",
            async () =>
            {
                using (_logger.BeginUserScope(userId))
                {
                    _logger.LogDebug(
                        LoggingConstants.EventIds.TankServiceOperationStarted,
                        "Fetching all tanks for user");

                    var tanks = await _context.Tanks
                        .Where(t => t.UserId == userId)
                        .Include(t => t.WaterTests)
                        .Include(t => t.Livestock)
                        .Include(t => t.MaintenanceLogs)
                        .Include(t => t.ShoppingListItems)
                        .Include(t => t.Filters)
                        .Include(t => t.Lights)
                        .Include(t => t.Heaters)
                        .Include(t => t.ProteinSkimmers)
                        .ToListAsync();

                    _logger.LogInformation(
                        LoggingConstants.EventIds.TankServiceOperationCompleted,
                        "Retrieved {TankCount} tanks for user",
                        tanks.Count);

                    return tanks;
                }
            },
            LoggingConstants.EventIds.TankServiceOperationStarted,
            new Dictionary<string, object> { [LoggingConstants.Properties.UserId] = userId });
    }

    public async Task<Tank?> GetTankByIdAsync(int id, string userId)
    {
        return await _logger.LogExecutionTimeAsync(
            "GetTankById",
            async () =>
            {
                using (_logger.BeginUserScope(userId))
                using (_logger.BeginTankScope(id))
                {
                    _logger.LogDebug(
                        LoggingConstants.EventIds.TankServiceOperationStarted,
                        "Fetching tank by ID");

                    var tank = await _context.Tanks
                        .Where(t => t.UserId == userId)
                        .Include(t => t.WaterTests)
                        .Include(t => t.Livestock)
                        .Include(t => t.MaintenanceLogs)
                        .Include(t => t.ShoppingListItems)
                        .Include(t => t.Filters)
                        .Include(t => t.Lights)
                        .Include(t => t.Heaters)
                        .Include(t => t.ProteinSkimmers)
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (tank == null)
                    {
                        _logger.LogWarning(
                            "Tank not found or access denied for tank ID {TankId}",
                            id);
                    }
                    else
                    {
                        _logger.LogInformation(
                            LoggingConstants.EventIds.TankServiceOperationCompleted,
                            "Successfully retrieved tank: {TankName}",
                            tank.Name);
                    }

                    return tank;
                }
            },
            LoggingConstants.EventIds.TankServiceOperationStarted,
            new Dictionary<string, object>
            {
                [LoggingConstants.Properties.UserId] = userId,
                [LoggingConstants.Properties.TankId] = id
            });
    }

    public async Task<Tank> CreateTankAsync(Tank tank, string userId)
    {
        return await _logger.LogExecutionTimeAsync(
            "CreateTank",
            async () =>
            {
                using (_logger.BeginUserScope(userId))
                {
                    _logger.LogInformation(
                        LoggingConstants.EventIds.TankServiceOperationStarted,
                        "Creating new tank: {TankName}, Type: {TankType}, Volume: {VolumeGallons}gal",
                        tank.Name,
                        tank.Type,
                        tank.VolumeGallons);

                    tank.UserId = userId;

                    // Normalize StartDate to UTC midnight to avoid timezone issues
                    tank.StartDate = DateTime.SpecifyKind(tank.StartDate.Date, DateTimeKind.Utc);

                    // Ensure navigation properties are initialized
                    if (tank.ShoppingListItems == null)
                    {
                        tank.ShoppingListItems = new List<ShoppingListItem>();
                    }

                    _context.Tanks.Add(tank);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        LoggingConstants.EventIds.TankCreated,
                        "Tank created successfully with ID: {TankId}",
                        tank.Id);

                    return tank;
                }
            },
            LoggingConstants.EventIds.TankServiceOperationStarted,
            new Dictionary<string, object>
            {
                [LoggingConstants.Properties.UserId] = userId,
                ["TankName"] = tank.Name,
                ["TankType"] = tank.Type.ToString()
            });
    }

    public async Task<Tank> UpdateTankAsync(Tank tank, string userId)
    {
        return await _logger.LogExecutionTimeAsync(
            "UpdateTank",
            async () =>
            {
                using (_logger.BeginUserScope(userId))
                using (_logger.BeginTankScope(tank.Id, tank.Name))
                {
                    _logger.LogInformation(
                        LoggingConstants.EventIds.TankServiceOperationStarted,
                        "Updating tank");

                    // Verify ownership
                    var existingTank = await _context.Tanks
                        .FirstOrDefaultAsync(t => t.Id == tank.Id && t.UserId == userId);

                    if (existingTank == null)
                    {
                        _logger.LogWarning(
                            LoggingConstants.EventIds.UnauthorizedAccess,
                            "Unauthorized update attempt for tank ID {TankId}",
                            tank.Id);
                        throw new UnauthorizedAccessException("You don't have permission to update this tank.");
                    }

                    var changes = new List<string>();
                    if (existingTank.Name != tank.Name)
                        changes.Add($"Name: {existingTank.Name} → {tank.Name}");
                    if (existingTank.VolumeGallons != tank.VolumeGallons)
                        changes.Add($"Volume: {existingTank.VolumeGallons} → {tank.VolumeGallons}gal");
                    if (existingTank.Type != tank.Type)
                        changes.Add($"Type: {existingTank.Type} → {tank.Type}");

                    existingTank.Name = tank.Name;
                    existingTank.VolumeGallons = tank.VolumeGallons;
                    existingTank.Type = tank.Type;
                    // Normalize StartDate to UTC midnight to avoid timezone issues
                    existingTank.StartDate = DateTime.SpecifyKind(tank.StartDate.Date, DateTimeKind.Utc);
                    existingTank.Notes = tank.Notes;
                    existingTank.ImagePath = tank.ImagePath;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        LoggingConstants.EventIds.TankUpdated,
                        "Tank updated successfully. Changes: {Changes}",
                        string.Join(", ", changes));

                    return existingTank;
                }
            },
            LoggingConstants.EventIds.TankServiceOperationStarted,
            new Dictionary<string, object>
            {
                [LoggingConstants.Properties.UserId] = userId,
                [LoggingConstants.Properties.TankId] = tank.Id
            });
    }

    public async Task<bool> DeleteTankAsync(int id, string userId)
    {
        return await _logger.LogExecutionTimeAsync(
            "DeleteTank",
            async () =>
            {
                using (_logger.BeginUserScope(userId))
                using (_logger.BeginTankScope(id))
                {
                    _logger.LogInformation(
                        LoggingConstants.EventIds.TankServiceOperationStarted,
                        "Deleting tank");

                    var tank = await _context.Tanks
                        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                    if (tank == null)
                    {
                        _logger.LogWarning("Tank not found for deletion");
                        return false;
                    }

                    // Delete associated image if exists
                    if (!string.IsNullOrEmpty(tank.ImagePath))
                    {
                        _logger.LogDebug("Deleting tank image: {ImagePath}", tank.ImagePath);
                        await _fileUploadService.DeleteImageAsync(tank.ImagePath);
                    }

                    _context.Tanks.Remove(tank);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        LoggingConstants.EventIds.TankDeleted,
                        "Tank deleted successfully: {TankName}",
                        tank.Name);

                    return true;
                }
            },
            LoggingConstants.EventIds.TankServiceOperationStarted,
            new Dictionary<string, object>
            {
                [LoggingConstants.Properties.UserId] = userId,
                [LoggingConstants.Properties.TankId] = id
            });
    }

    public async Task<TankDashboardViewModel> GetTankDashboardAsync(int tankId, string userId, int month, int year)
    {
        return await _logger.LogExecutionTimeAsync(
            "GetTankDashboard",
            async () =>
            {
                using (_logger.BeginUserScope(userId))
                using (_logger.BeginTankScope(tankId))
                {
                    _logger.LogDebug(
                        LoggingConstants.EventIds.TankServiceOperationStarted,
                        "Loading tank dashboard for month: {Month}/{Year}",
                        month,
                        year);

                    var tank = await _context.Tanks
                        .Where(t => t.Id == tankId && t.UserId == userId)
                        .Include(t => t.WaterTests)
                        .Include(t => t.Livestock)
                        .Include(t => t.MaintenanceLogs)
                        .Include(t => t.Filters)
                        .Include(t => t.Lights)
                        .Include(t => t.Heaters)
                        .Include(t => t.ProteinSkimmers)
                        .FirstOrDefaultAsync();

                    if (tank == null)
                    {
                        _logger.LogWarning("Tank not found for dashboard");
                        return new TankDashboardViewModel();
                    }

                    _logger.LogDebug(
                        "Building dashboard for tank: {TankName}, WaterTests: {TestCount}, Livestock: {LivestockCount}",
                        tank.Name,
                        tank.WaterTests?.Count ?? 0,
                        tank.Livestock?.Count ?? 0);

                    var viewModel = new TankDashboardViewModel
                    {
                        Tank = tank,
                        SelectedMonth = month,
                        SelectedYear = year
                    };

                    // Get most recent water test
                    viewModel.MostRecentWaterTest = tank.WaterTests
                        .OrderByDescending(wt => wt.Timestamp)
                        .FirstOrDefault();

                    // Get recent water tests (last 10)
                    viewModel.RecentWaterTests = tank.WaterTests
                        .OrderByDescending(wt => wt.Timestamp)
                        .Take(10)
                        .ToList();

                    // Get water tests for selected month
                    var startDate = new DateTime(year, month, 1);
                    var endDate = startDate.AddMonths(1);

                    var monthTests = tank.WaterTests
                        .Where(wt => wt.Timestamp >= startDate && wt.Timestamp < endDate)
                        .OrderBy(wt => wt.Timestamp)
                        .ToList();

                    _logger.LogDebug("Found {TestCount} water tests for selected month", monthTests.Count);

                    // Build chart data
                    viewModel.ChartLabels = monthTests.Select(wt => wt.Timestamp.ToString("MMM dd")).ToList();
                    viewModel.PHData = monthTests.Select(wt => wt.PH).ToList();
                    viewModel.TemperatureData = monthTests.Select(wt => wt.Temperature).ToList();
                    viewModel.AmmoniaData = monthTests.Select(wt => wt.Ammonia).ToList();
                    viewModel.NitriteData = monthTests.Select(wt => wt.Nitrite).ToList();
                    viewModel.NitrateData = monthTests.Select(wt => wt.Nitrate).ToList();

                    // Reef-specific
                    viewModel.SalinityData = monthTests.Select(wt => wt.Salinity).ToList();
                    viewModel.AlkalinityData = monthTests.Select(wt => wt.Alkalinity).ToList();
                    viewModel.CalciumData = monthTests.Select(wt => wt.Calcium).ToList();
                    viewModel.MagnesiumData = monthTests.Select(wt => wt.Magnesium).ToList();
                    viewModel.PhosphateData = monthTests.Select(wt => wt.Phosphate).ToList();

                    // Freshwater-specific
                    viewModel.GHData = monthTests.Select(wt => wt.GH).ToList();
                    viewModel.KHData = monthTests.Select(wt => wt.KH).ToList();
                    viewModel.TDSData = monthTests.Select(wt => wt.TDS).ToList();

                    // Build available months dropdown
                    var allTests = tank.WaterTests.OrderBy(wt => wt.Timestamp).ToList();
                    if (allTests.Any())
                    {
                        var firstTest = allTests.First().Timestamp;
                        var lastTest = allTests.Last().Timestamp;

                        var current = new DateTime(firstTest.Year, firstTest.Month, 1);
                        var end = new DateTime(lastTest.Year, lastTest.Month, 1);

                        while (current <= end)
                        {
                            viewModel.AvailableMonths.Add(new MonthYearOption
                            {
                                Month = current.Month,
                                Year = current.Year,
                                DisplayText = current.ToString("MMMM yyyy")
                            });
                            current = current.AddMonths(1);
                        }
                    }

                    // Get equipment needing maintenance - query all equipment types separately
                    var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
                    var equipmentNeedingMaintenance = new List<Equipment>();

                    equipmentNeedingMaintenance.AddRange(
                        await _context.Filters
                            .Where(e => e.TankId == tankId && e.InstalledOn < sixMonthsAgo)
                            .ToListAsync()
                    );
                    equipmentNeedingMaintenance.AddRange(
                        await _context.Lights
                            .Where(e => e.TankId == tankId && e.InstalledOn < sixMonthsAgo)
                            .ToListAsync()
                    );
                    equipmentNeedingMaintenance.AddRange(
                        await _context.Heaters
                            .Where(e => e.TankId == tankId && e.InstalledOn < sixMonthsAgo)
                            .ToListAsync()
                    );
                    equipmentNeedingMaintenance.AddRange(
                        await _context.ProteinSkimmers
                            .Where(e => e.TankId == tankId && e.InstalledOn < sixMonthsAgo)
                            .ToListAsync()
                    );

                    viewModel.EquipmentNeedingMaintenance = equipmentNeedingMaintenance;

                    if (equipmentNeedingMaintenance.Any())
                    {
                        _logger.LogInformation(
                            "{EquipmentCount} equipment items need maintenance",
                            equipmentNeedingMaintenance.Count);
                    }

                    // Recent maintenance
                    viewModel.RecentMaintenance = tank.MaintenanceLogs
                        .OrderByDescending(m => m.Timestamp)
                        .Take(5)
                        .ToList();

                    // Get upcoming reminders for this tank (next 7 days)
                    var nextWeek = DateTime.UtcNow.AddDays(7);
                    viewModel.UpcomingReminders = await _context.Reminders
                        .Where(r => r.UserId == userId &&
                                   r.IsActive &&
                                   r.TankId == tankId &&
                                   r.NextDueDate <= nextWeek)
                        .OrderBy(r => r.NextDueDate)
                        .Take(5)
                        .ToListAsync();

                    // Get recent notifications for this tank (last 10)
                    viewModel.RecentNotifications = await _context.Notifications
                        .Where(n => n.UserId == userId && n.TankId == tankId)
                        .OrderByDescending(n => n.CreatedAt)
                        .Take(10)
                        .ToListAsync();

                    // Count unread notifications for this tank
                    viewModel.UnreadNotificationCount = await _context.Notifications
                        .CountAsync(n => n.UserId == userId && n.TankId == tankId && !n.IsRead);

                    // Get latest journal entry for this tank
                    viewModel.LatestJournalEntry = await _context.JournalEntries
                        .Where(j => j.TankId == tankId)
                        .OrderByDescending(j => j.Timestamp)
                        .FirstOrDefaultAsync();

                    _logger.LogInformation(
                        LoggingConstants.EventIds.TankServiceOperationCompleted,
                        "Dashboard loaded successfully with {ReminderCount} upcoming reminders and {NotificationCount} unread notifications",
                        viewModel.UpcomingReminders?.Count ?? 0,
                        viewModel.UnreadNotificationCount);

                    return viewModel;
                }
            },
            LoggingConstants.EventIds.TankServiceOperationStarted,
            new Dictionary<string, object>
            {
                [LoggingConstants.Properties.UserId] = userId,
                [LoggingConstants.Properties.TankId] = tankId
            });
    }
}
