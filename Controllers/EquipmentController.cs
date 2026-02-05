using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class EquipmentController : Controller
{
    private readonly IEquipmentService _equipmentService;
    private readonly ITankService _tankService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<EquipmentController> _logger;

    public EquipmentController(
        IEquipmentService equipmentService,
        ITankService tankService,
        UserManager<AppUser> userManager,
        ILogger<EquipmentController> logger)
    {
        _equipmentService = equipmentService;
        _tankService = tankService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Equipment
    public async Task<IActionResult> Index(int? tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            List<Equipment> equipment;
            if (tankId.HasValue)
            {
                equipment = await _equipmentService.GetEquipmentByTankAsync(tankId.Value, userId);
                var tank = await _tankService.GetTankByIdAsync(tankId.Value, userId);
                ViewBag.TankName = tank?.Name;
                ViewBag.TankId = tankId.Value;
            }
            else
            {
                equipment = await _equipmentService.GetAllEquipmentAsync(userId);
            }

            return View(equipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving equipment");
            TempData["Error"] = "An error occurred while retrieving equipment.";
            return View(new List<Equipment>());
        }
    }

    // GET: Equipment/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var equipment = await _equipmentService.GetEquipmentByIdAsync(id, userId);
            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving equipment details for ID: {EquipmentId}", id);
            TempData["Error"] = "An error occurred while retrieving equipment details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Equipment/Dashboard/5
    public async Task<IActionResult> Dashboard(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var dashboard = await _equipmentService.GetEquipmentDashboardAsync(id, userId);
            if (dashboard == null)
            {
                return NotFound();
            }

            return View(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving equipment dashboard for ID: {EquipmentId}", id);
            TempData["Error"] = "An error occurred while retrieving equipment dashboard.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Equipment/Create
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
            _logger.LogError(ex, "Error loading create equipment view");
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Equipment/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int tankId, string equipmentType, string? brand)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // If brand is null/empty, this is the first POST (equipment type selection)
            // Just show the form with the equipment type
            if (string.IsNullOrEmpty(brand))
            {
                await PopulateTanksDropdown(userId);
                ViewBag.EquipmentType = equipmentType;
                ViewBag.SelectedTankId = tankId > 0 ? tankId : (int?)null;
                return View();
            }

            // Validate tankId for actual equipment creation
            if (tankId <= 0)
            {
                ModelState.AddModelError("TankId", "Please select a tank");
                await PopulateTanksDropdown(userId);
                ViewBag.EquipmentType = equipmentType;
                return View();
            }

            // Create the appropriate equipment type based on selection
            Equipment equipment = equipmentType switch
            {
                "Filter" => new Filter(),
                "Heater" => new Heater(),
                "Light" => new Light(),
                "ProteinSkimmer" => new ProteinSkimmer(),
                "Pump" => new Pump(),
                "Chiller" => new Chiller(),
                "WaveMaker" => new WaveMaker(),
                "AutoTopOff" => new AutoTopOff(),
                "DosingPump" => new DosingPump(),
                "Reactor" => new Reactor(),
                "AutoFeeder" => new AutoFeeder(),
                "CO2System" => new CO2System(),
                "DigitalWaterTester" => new DigitalWaterTester(),
                "ReverseOsmosisSystem" => new ReverseOsmosisSystem(),
                "UVSterilizer" => new UVSterilizer(),
                _ => throw new ArgumentException("Invalid equipment type")
            };

            equipment.TankId = tankId;

            // Bind common properties
            await TryUpdateModelAsync(equipment, "",
                e => e.Brand,
                e => e.Model,
                e => e.InstalledOn);

            // Bind type-specific properties
            switch (equipment)
            {
                case Filter filter:
                    await TryUpdateModelAsync(filter, "",
                        f => f.Type,
                        f => f.FlowRate,
                        f => f.Media,
                        f => f.LastMaintenanceDate);
                    break;

                case Heater heater:
                    await TryUpdateModelAsync(heater, "",
                        h => h.MinTemperature,
                        h => h.MaxTemperature);
                    break;

                case Light light:
                    await TryUpdateModelAsync(light, "",
                        l => l.Wattage,
                        l => l.Spectrum,
                        l => l.IsDimmable,
                        l => l.IntensityPercent,
                        l => l.Schedule);
                    break;

                case ProteinSkimmer skimmer:
                    await TryUpdateModelAsync(skimmer, "",
                        s => s.Capacity,
                        s => s.Type,
                        s => s.AirIntake,
                        s => s.CupFillLevel);
                    break;

                case Pump pump:
                    await TryUpdateModelAsync(pump, "",
                        p => p.FlowRate,
                        p => p.MaxHeadHeight,
                        p => p.PowerConsumption,
                        p => p.IsVariableSpeed);
                    break;

                case Chiller chiller:
                    await TryUpdateModelAsync(chiller, "",
                        c => c.MinTemperature,
                        c => c.MaxTemperature,
                        c => c.TargetTemperature,
                        c => c.BTUCapacity,
                        c => c.MaxTankSize);
                    break;

                case WaveMaker waveMaker:
                    await TryUpdateModelAsync(waveMaker, "",
                        w => w.FlowRate,
                        w => w.IsControllable,
                        w => w.WavePattern,
                        w => w.IntensityPercent,
                        w => w.PowerConsumption);
                    break;

                case AutoTopOff ato:
                    await TryUpdateModelAsync(ato, "",
                        a => a.ReservoirCapacity,
                        a => a.PumpRate,
                        a => a.SensorType,
                        a => a.LastReservoirRefill);
                    break;

                case DosingPump dosingPump:
                    await TryUpdateModelAsync(dosingPump, "",
                        d => d.NumberOfChannels,
                        d => d.SolutionType,
                        d => d.DoseAmount,
                        d => d.DosingSchedule,
                        d => d.LastRefillDate);
                    break;

                case Reactor reactor:
                    await TryUpdateModelAsync(reactor, "",
                        r => r.ReactorType,
                        r => r.FlowRate,
                        r => r.MediaType,
                        r => r.LastMediaChange,
                        r => r.MediaCapacity);
                    break;

                case AutoFeeder autoFeeder:
                    await TryUpdateModelAsync(autoFeeder, "",
                        a => a.FeedingsPerDay,
                        a => a.FeedingSchedule,
                        a => a.PortionSize,
                        a => a.HopperCapacity,
                        a => a.LastRefillDate);
                    break;

                case CO2System co2System:
                    await TryUpdateModelAsync(co2System, "",
                        c => c.TargetPH,
                        c => c.CurrentPH,
                        c => c.RegulatorType,
                        c => c.DiffuserType);
                    break;

                case DigitalWaterTester tester:
                    await TryUpdateModelAsync(tester, "",
                        t => t.ParametersMonitored,
                        t => t.HasWifiConnectivity,
                        t => t.HasAlerts,
                        t => t.LastCalibrationDate,
                        t => t.CalibrationIntervalDays);
                    break;

                case ReverseOsmosisSystem ros:
                    await TryUpdateModelAsync(ros, "",
                        r => r.GPDRating,
                        r => r.NumberOfStages,
                        r => r.LastMembraneChange,
                        r => r.LastFilterChange,
                        r => r.MembraneLifespanMonths,
                        r => r.WasteWaterRatio);
                    break;

                case UVSterilizer uvSterilizer:
                    await TryUpdateModelAsync(uvSterilizer, "",
                        u => u.Wattage,
                        u => u.FlowRate,
                        u => u.BulbInstalledDate,
                        u => u.BulbLifespanHours,
                        u => u.MaxTankSize);
                    break;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var createdEquipment = await _equipmentService.CreateEquipmentAsync(equipment, tankId, userId);
                    TempData["Success"] = $"{equipmentType} added successfully!";
                    return RedirectToAction(nameof(Details), new { id = createdEquipment.Id });
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Unauthorized tank access for equipment creation");
                    TempData["Error"] = ex.Message;
                    await PopulateTanksDropdown(userId);
                    ViewBag.SelectedTankId = tankId;
                    ViewBag.EquipmentType = equipmentType;
                    return View();
                }
            }

            // Log validation errors
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogWarning($"Validation error in {state.Key}: {error.ErrorMessage}");
                }
            }

            await PopulateTanksDropdown(userId);
            ViewBag.SelectedTankId = tankId;
            ViewBag.EquipmentType = equipmentType;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating equipment. Type: {EquipmentType}, TankId: {TankId}", equipmentType, tankId);
            TempData["Error"] = $"An error occurred while adding the equipment: {ex.Message}";
            await PopulateTanksDropdown(_userManager.GetUserId(User)!);
            ViewBag.EquipmentType = equipmentType;
            ViewBag.SelectedTankId = tankId > 0 ? tankId : (int?)null;
            return View();
        }
    }

    // GET: Equipment/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var equipment = await _equipmentService.GetEquipmentByIdAsync(id, userId);
            if (equipment == null)
            {
                return NotFound();
            }

            await PopulateTanksDropdown(userId);
            return View(equipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving equipment for edit, ID: {EquipmentId}", id);
            TempData["Error"] = "An error occurred while retrieving the equipment.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Equipment/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,Brand,Model,PurchaseDate,PurchasePrice,WarrantyExpirationDate,Notes")] Equipment equipment)
    {
        if (id != equipment.Id)
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
                await _equipmentService.UpdateEquipmentAsync(equipment, userId);
                TempData["Success"] = "Equipment updated successfully!";
                return RedirectToAction(nameof(Details), new { id = equipment.Id });
            }

            await PopulateTanksDropdown(userId);
            return View(equipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating equipment ID: {EquipmentId}", id);
            TempData["Error"] = "An error occurred while updating the equipment.";
            await PopulateTanksDropdown(_userManager.GetUserId(User)!);
            return View(equipment);
        }
    }

    // GET: Equipment/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var equipment = await _equipmentService.GetEquipmentByIdAsync(id, userId);
            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving equipment for deletion, ID: {EquipmentId}", id);
            TempData["Error"] = "An error occurred while retrieving the equipment.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Equipment/Delete/5
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

            var result = await _equipmentService.DeleteEquipmentAsync(id, userId);
            if (result)
            {
                TempData["Success"] = "Equipment deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete equipment.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting equipment ID: {EquipmentId}", id);
            TempData["Error"] = "An error occurred while deleting the equipment.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task PopulateTanksDropdown(string userId)
    {
        var tanks = await _tankService.GetAllTanksAsync(userId);
        ViewBag.Tanks = new SelectList(tanks, "Id", "Name");
    }

    // API endpoint to get equipment by tank for AJAX calls
    [HttpGet]
    public async Task<IActionResult> GetEquipmentByTank(int tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var equipment = await _equipmentService.GetEquipmentByTankAsync(tankId, userId);
            var equipmentList = equipment.Select(e => new
            {
                id = e.Id,
                brand = e.Brand,
                model = e.Model
            }).ToList();

            return Json(equipmentList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving equipment for tank ID: {TankId}", tankId);
            return Json(new List<object>());
        }
    }
}
