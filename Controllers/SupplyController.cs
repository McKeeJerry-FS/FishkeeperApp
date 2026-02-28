using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class SupplyController : Controller
{
    private readonly ISupplyService _supplyService;
    private readonly ITankService _tankService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<SupplyController> _logger;

    public SupplyController(
        ISupplyService supplyService,
        ITankService tankService,
        UserManager<AppUser> userManager,
        ILogger<SupplyController> logger)
    {
        _supplyService = supplyService;
        _tankService = tankService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Supply
    public async Task<IActionResult> Index(int? tankId, SupplyCategory? category, string? status)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            List<SupplyItem> supplies;

            if (tankId.HasValue)
            {
                supplies = await _supplyService.GetSuppliesByTankAsync(tankId.Value, userId);
                var tank = await _tankService.GetTankByIdAsync(tankId.Value, userId);
                ViewBag.TankName = tank?.Name;
                ViewBag.TankId = tankId.Value;
            }
            else if (category.HasValue)
            {
                supplies = await _supplyService.GetSuppliesByCategoryAsync(userId, category.Value);
                ViewBag.CategoryFilter = category.Value;
            }
            else
            {
                supplies = await _supplyService.GetSuppliesByUserAsync(userId);
            }

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(status))
            {
                supplies = status.ToLower() switch
                {
                    "low" => supplies.Where(s => s.Status == StockStatus.LowStock).ToList(),
                    "out" => supplies.Where(s => s.Status == StockStatus.OutOfStock).ToList(),
                    _ => supplies
                };
                ViewBag.StatusFilter = status;
            }

            // Calculate summary statistics
            ViewBag.TotalItems = supplies.Count;
            ViewBag.LowStockCount = supplies.Count(s => s.Status == StockStatus.LowStock);
            ViewBag.OutOfStockCount = supplies.Count(s => s.Status == StockStatus.OutOfStock);
            ViewBag.TotalValue = supplies
                .Where(s => s.LastPurchasePrice.HasValue)
                .Sum(s => (decimal)s.CurrentQuantity * (s.LastPurchasePrice ?? 0));

            await LoadViewDataAsync(userId);
            return View(supplies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading supplies");
            TempData["ErrorMessage"] = "An error occurred while loading supplies.";
            return RedirectToAction("Index", "Home");
        }
    }

    // GET: Supply/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var supply = await _supplyService.GetSupplyByIdAsync(id, userId);
            if (supply == null)
            {
                TempData["ErrorMessage"] = "Supply item not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(supply);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading supply details for ID {id}");
            TempData["ErrorMessage"] = "An error occurred while loading supply details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Supply/Create
    public async Task<IActionResult> Create(int? tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var supply = new SupplyItem
            {
                TankId = tankId,
                UserId = userId,
                EnableLowStockAlert = true,
                IsActive = true,
                Unit = "units"
            };

            await LoadViewDataAsync(userId);

            if (tankId.HasValue)
            {
                var tank = await _tankService.GetTankByIdAsync(tankId.Value, userId);
                ViewBag.TankName = tank?.Name;
            }

            return View(supply);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading supply creation form");
            TempData["ErrorMessage"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Supply/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplyItem supply, IFormFile? ImageFile)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }


            supply.UserId = userId;

            // Handle image upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    await ImageFile.CopyToAsync(ms);
                    supply.ImageData = ms.ToArray();
                    supply.ImageType = ImageFile.ContentType;
                }
            }

            // Validate tank ownership if tank is specified
            if (supply.TankId.HasValue)
            {
                var tank = await _tankService.GetTankByIdAsync(supply.TankId.Value, userId);
                if (tank == null)
                {
                    ModelState.AddModelError("TankId", "Invalid tank selected.");
                }
            }

            if (ModelState.IsValid)
            {
                await _supplyService.CreateSupplyAsync(supply);
                TempData["SuccessMessage"] = $"Supply item '{supply.Name}' has been added successfully.";

                if (supply.Status == StockStatus.LowStock || supply.Status == StockStatus.OutOfStock)
                {
                    TempData["WarningMessage"] = $"Note: '{supply.Name}' is currently {supply.Status.ToString().ToLower().Replace("stock", " stock")}.";
                }

                return RedirectToAction(nameof(Index));
            }

            // Log validation errors for debugging
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogWarning($"Validation error in {state.Key}: {error.ErrorMessage}");
                }
            }

            await LoadViewDataAsync(userId);
            return View(supply);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supply item");
            TempData["ErrorMessage"] = "An error occurred while creating the supply item.";
            await LoadViewDataAsync(_userManager.GetUserId(User)!);
            return View(supply);
        }
    }

    // GET: Supply/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var supply = await _supplyService.GetSupplyByIdAsync(id, userId);
            if (supply == null)
            {
                TempData["ErrorMessage"] = "Supply item not found.";
                return RedirectToAction(nameof(Index));
            }

            await LoadViewDataAsync(userId);
            return View(supply);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading supply for editing: {id}");
            TempData["ErrorMessage"] = "An error occurred while loading the supply item.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Supply/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SupplyItem supply)
    {
        if (id != supply.Id)
        {
            return BadRequest();
        }

        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            supply.UserId = userId;

            // Validate tank ownership if tank is specified
            if (supply.TankId.HasValue)
            {
                var tank = await _tankService.GetTankByIdAsync(supply.TankId.Value, userId);
                if (tank == null)
                {
                    ModelState.AddModelError("TankId", "Invalid tank selected.");
                }
            }

            if (ModelState.IsValid)
            {
                var updated = await _supplyService.UpdateSupplyAsync(supply, userId);
                if (updated == null)
                {
                    TempData["ErrorMessage"] = "Supply item not found or you don't have permission to edit it.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] = $"Supply item '{supply.Name}' has been updated successfully.";

                if (updated.Status == StockStatus.LowStock || updated.Status == StockStatus.OutOfStock)
                {
                    TempData["WarningMessage"] = $"Note: '{supply.Name}' is currently {updated.Status.ToString().ToLower().Replace("stock", " stock")}.";
                }

                return RedirectToAction(nameof(Details), new { id = supply.Id });
            }

            await LoadViewDataAsync(userId);
            return View(supply);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating supply: {id}");
            TempData["ErrorMessage"] = "An error occurred while updating the supply item.";
            await LoadViewDataAsync(_userManager.GetUserId(User)!);
            return View(supply);
        }
    }

    // GET: Supply/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var supply = await _supplyService.GetSupplyByIdAsync(id, userId);
            if (supply == null)
            {
                TempData["ErrorMessage"] = "Supply item not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(supply);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading supply for deletion: {id}");
            TempData["ErrorMessage"] = "An error occurred while loading the supply item.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Supply/Delete/5
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

            var supply = await _supplyService.GetSupplyByIdAsync(id, userId);
            if (supply == null)
            {
                TempData["ErrorMessage"] = "Supply item not found.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _supplyService.DeleteSupplyAsync(id, userId);
            if (result)
            {
                TempData["SuccessMessage"] = $"Supply item '{supply.Name}' has been deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete the supply item.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting supply: {id}");
            TempData["ErrorMessage"] = "An error occurred while deleting the supply item.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Supply/UpdateQuantity/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int id, double quantity, string? notes)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _supplyService.UpdateQuantityAsync(id, userId, quantity, notes);
            if (result)
            {
                TempData["SuccessMessage"] = "Quantity updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update quantity.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating quantity for supply: {id}");
            TempData["ErrorMessage"] = "An error occurred while updating the quantity.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // POST: Supply/AddStock/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStock(int id, double amount, decimal? price)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _supplyService.AddQuantityAsync(id, userId, amount, price);
            if (result)
            {
                TempData["SuccessMessage"] = $"Added {amount} to stock successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add to stock.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding stock for supply: {id}");
            TempData["ErrorMessage"] = "An error occurred while adding stock.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // POST: Supply/RemoveStock/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveStock(int id, double amount)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _supplyService.RemoveQuantityAsync(id, userId, amount);
            if (result)
            {
                TempData["SuccessMessage"] = $"Removed {amount} from stock successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to remove from stock.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing stock for supply: {id}");
            TempData["ErrorMessage"] = "An error occurred while removing stock.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // GET: Supply/LowStock
    public async Task<IActionResult> LowStock()
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var lowStockItems = await _supplyService.GetLowStockItemsAsync(userId);
            var outOfStockItems = await _supplyService.GetOutOfStockItemsAsync(userId);
            var expiringItems = await _supplyService.GetExpiringSoonItemsAsync(userId, 30);

            ViewBag.LowStockCount = lowStockItems.Count;
            ViewBag.OutOfStockCount = outOfStockItems.Count;
            ViewBag.ExpiringCount = expiringItems.Count;
            ViewBag.ExpiringItems = expiringItems;

            await LoadViewDataAsync(userId);
            return View(lowStockItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading low stock items");
            TempData["ErrorMessage"] = "An error occurred while loading low stock items.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Supply/Search
    public async Task<IActionResult> Search(string searchTerm)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var supplies = await _supplyService.SearchSuppliesAsync(userId, searchTerm);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.ResultCount = supplies.Count;

            await LoadViewDataAsync(userId);
            return View("Index", supplies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching supplies with term: {searchTerm}");
            TempData["ErrorMessage"] = "An error occurred while searching.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task LoadViewDataAsync(string userId)
    {
        var tanks = await _tankService.GetAllTanksAsync(userId);
        ViewBag.Tanks = new SelectList(tanks, "Id", "Name");

        ViewBag.Categories = Enum.GetValues(typeof(SupplyCategory))
            .Cast<SupplyCategory>()
            .Select(c => new SelectListItem
            {
                Value = ((int)c).ToString(),
                Text = c.ToString()
            })
            .ToList();

        ViewBag.Units = new List<string>
        {
            "units", "ml", "liters", "grams", "kg", "oz", "lbs",
            "tablets", "capsules", "doses", "gallons", "test strips"
        };
    }
}
