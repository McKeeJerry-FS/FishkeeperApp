using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class ShoppingListController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ShoppingListController> _logger;

    public ShoppingListController(
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        ILogger<ShoppingListController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: ShoppingList
    public async Task<IActionResult> Index(int? tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var query = _context.ShoppingListItems
                .Include(s => s.Tank)
                .Where(s => s.Tank!.UserId == userId);

            if (tankId.HasValue)
            {
                query = query.Where(s => s.TankId == tankId.Value);
                var tank = await _context.Tanks.FindAsync(tankId.Value);
                ViewBag.TankName = tank?.Name;
                ViewBag.TankId = tankId.Value;
            }

            var items = await query
                .OrderBy(s => s.IsPurchased)
                .ThenByDescending(s => s.Priority)
                .ThenBy(s => s.DateAdded)
                .ToListAsync();

            // Get user's tanks for the filter dropdown
            var userTanks = await _context.Tanks
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewBag.UserTanks = userTanks;

            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shopping list items");
            TempData["Error"] = "An error occurred while retrieving the shopping list.";
            return View(new List<ShoppingListItem>());
        }
    }

    // GET: ShoppingList/Create
    public async Task<IActionResult> Create(int? tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var tanks = await _context.Tanks
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewBag.TankId = new SelectList(tanks, "Id", "Name", tankId);

            var model = new ShoppingListItem();
            if (tankId.HasValue)
            {
                model.TankId = tankId.Value;
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create form");
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: ShoppingList/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ShoppingListItem item)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Verify tank belongs to user
            var tank = await _context.Tanks.FindAsync(item.TankId);
            if (tank == null || tank.UserId != userId)
            {
                TempData["Error"] = "Invalid tank selected.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                item.DateAdded = DateTime.UtcNow;
                _context.ShoppingListItems.Add(item);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"'{item.Name}' has been added to your shopping list!";
                return RedirectToAction(nameof(Index), new { tankId = item.TankId });
            }

            var tanks = await _context.Tanks
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Name)
                .ToListAsync();
            ViewBag.TankId = new SelectList(tanks, "Id", "Name", item.TankId);

            return View(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shopping list item");
            TempData["Error"] = "An error occurred while adding the item.";
            return View(item);
        }
    }

    // GET: ShoppingList/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var item = await _context.ShoppingListItems
                .Include(s => s.Tank)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null || item.Tank?.UserId != userId)
            {
                TempData["Error"] = "Shopping list item not found.";
                return RedirectToAction(nameof(Index));
            }

            var tanks = await _context.Tanks
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Name)
                .ToListAsync();
            ViewBag.TankId = new SelectList(tanks, "Id", "Name", item.TankId);

            return View(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit form");
            TempData["Error"] = "An error occurred while loading the item.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: ShoppingList/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ShoppingListItem item)
    {
        if (id != item.Id)
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

            var existingItem = await _context.ShoppingListItems
                .Include(s => s.Tank)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingItem == null || existingItem.Tank?.UserId != userId)
            {
                TempData["Error"] = "Shopping list item not found.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                existingItem.ItemType = item.ItemType;
                existingItem.Name = item.Name;
                existingItem.Brand = item.Brand;
                existingItem.ModelOrSpecies = item.ModelOrSpecies;
                existingItem.EstimatedCost = item.EstimatedCost;
                existingItem.Quantity = item.Quantity;
                existingItem.Notes = item.Notes;
                existingItem.Priority = item.Priority;
                existingItem.SupplyCategory = item.SupplyCategory;
                existingItem.EquipmentType = item.EquipmentType;
                existingItem.PurchaseLink = item.PurchaseLink;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Shopping list item updated successfully!";
                return RedirectToAction(nameof(Index), new { tankId = existingItem.TankId });
            }

            var tanks = await _context.Tanks
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Name)
                .ToListAsync();
            ViewBag.TankId = new SelectList(tanks, "Id", "Name", item.TankId);

            return View(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shopping list item");
            TempData["Error"] = "An error occurred while updating the item.";
            return View(item);
        }
    }

    // POST: ShoppingList/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var item = await _context.ShoppingListItems
                .Include(s => s.Tank)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null || item.Tank?.UserId != userId)
            {
                TempData["Error"] = "Shopping list item not found.";
                return RedirectToAction(nameof(Index));
            }

            var tankId = item.TankId;
            _context.ShoppingListItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{item.Name}' has been removed from your shopping list.";
            return RedirectToAction(nameof(Index), new { tankId = tankId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shopping list item");
            TempData["Error"] = "An error occurred while deleting the item.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: ShoppingList/MarkPurchased/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPurchased(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var item = await _context.ShoppingListItems
                .Include(s => s.Tank)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null || item.Tank?.UserId != userId)
            {
                TempData["Error"] = "Shopping list item not found.";
                return RedirectToAction(nameof(Index));
            }

            item.IsPurchased = true;
            item.DatePurchased = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{item.Name}' has been marked as purchased!";
            return RedirectToAction(nameof(Index), new { tankId = item.TankId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking item as purchased");
            TempData["Error"] = "An error occurred while updating the item.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: ShoppingList/AddToInventory/5
    public async Task<IActionResult> AddToInventory(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var item = await _context.ShoppingListItems
                .Include(s => s.Tank)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null || item.Tank?.UserId != userId)
            {
                TempData["Error"] = "Shopping list item not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ShoppingListItem = item;
            return View(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading add to inventory form");
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: ShoppingList/AddToSupplies/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToSupplies(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var item = await _context.ShoppingListItems
                .Include(s => s.Tank)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null || item.Tank?.UserId != userId)
            {
                TempData["Error"] = "Shopping list item not found.";
                return RedirectToAction(nameof(Index));
            }

            if (item.ItemType != ShoppingListItemType.Supply)
            {
                TempData["Error"] = "This item is not a supply item.";
                return RedirectToAction(nameof(Index), new { tankId = item.TankId });
            }

            // Create a new supply item
            var supplyItem = new SupplyItem
            {
                UserId = userId,
                TankId = item.TankId,
                Name = item.Name,
                Brand = item.Brand,
                Description = item.Notes,
                Category = item.SupplyCategory ?? SupplyCategory.Other,
                CurrentQuantity = item.Quantity,
                MinimumQuantity = 1,
                Unit = "unit"
            };

            _context.SupplyItems.Add(supplyItem);

            // Mark as purchased and remove from shopping list
            _context.ShoppingListItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{item.Name}' has been added to your supplies!";
            return RedirectToAction("Index", "Supply");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to supplies");
            TempData["Error"] = "An error occurred while adding the item to supplies.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: ShoppingList/AddToLivestock/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToLivestock(int id, string livestockType)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var item = await _context.ShoppingListItems
                .Include(s => s.Tank)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null || item.Tank?.UserId != userId)
            {
                TempData["Error"] = "Shopping list item not found.";
                return RedirectToAction(nameof(Index));
            }

            if (item.ItemType != ShoppingListItemType.Livestock)
            {
                TempData["Error"] = "This item is not a livestock item.";
                return RedirectToAction(nameof(Index), new { tankId = item.TankId });
            }

            // Mark as purchased
            item.IsPurchased = true;
            item.DatePurchased = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{item.Name}' marked as purchased. Please add the livestock details manually.";
            TempData["Info"] = "Redirecting to Livestock page to complete the entry...";

            // Redirect to livestock controller with pre-filled data
            return RedirectToAction("Create", "Livestock", new
            {
                tankId = item.TankId,
                name = item.Name,
                species = item.ModelOrSpecies
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to livestock");
            TempData["Error"] = "An error occurred while processing the livestock item.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: ShoppingList/AddToEquipment/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToEquipment(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var item = await _context.ShoppingListItems
                .Include(s => s.Tank)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null || item.Tank?.UserId != userId)
            {
                TempData["Error"] = "Shopping list item not found.";
                return RedirectToAction(nameof(Index));
            }

            if (item.ItemType != ShoppingListItemType.Equipment)
            {
                TempData["Error"] = "This item is not an equipment item.";
                return RedirectToAction(nameof(Index), new { tankId = item.TankId });
            }

            // Mark as purchased
            item.IsPurchased = true;
            item.DatePurchased = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{item.Name}' marked as purchased. Please add the equipment details manually.";
            TempData["Info"] = "Redirecting to Equipment page to complete the entry...";

            // Redirect to equipment controller with pre-filled data
            return RedirectToAction("Create", "Equipment", new
            {
                tankId = item.TankId,
                equipmentType = "",
                brand = item.Brand,
                model = item.ModelOrSpecies
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to equipment");
            TempData["Error"] = "An error occurred while processing the equipment item.";
            return RedirectToAction(nameof(Index));
        }
    }
}
