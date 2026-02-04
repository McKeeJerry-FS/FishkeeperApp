using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class SupplyService : BaseService, ISupplyService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SupplyService> _logger;

    public SupplyService(
        ApplicationDbContext context,
        INotificationService notificationService,
        ILogger<SupplyService> logger)
        : base(context)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    #region CRUD Operations

    public async Task<List<SupplyItem>> GetSuppliesByUserAsync(string userId)
    {
        return await _context.SupplyItems
            .Include(s => s.Tank)
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<List<SupplyItem>> GetSuppliesByTankAsync(int tankId, string userId)
    {
        return await _context.SupplyItems
            .Include(s => s.Tank)
            .Where(s => s.TankId == tankId && s.UserId == userId && s.IsActive)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<List<SupplyItem>> GetSuppliesByCategoryAsync(string userId, SupplyCategory category)
    {
        return await _context.SupplyItems
            .Include(s => s.Tank)
            .Where(s => s.UserId == userId && s.Category == category && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<SupplyItem?> GetSupplyByIdAsync(int id, string userId)
    {
        return await _context.SupplyItems
            .Include(s => s.Tank)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    public async Task<SupplyItem> CreateSupplyAsync(SupplyItem supply)
    {
        supply.CreatedAt = DateTime.UtcNow;
        _context.SupplyItems.Add(supply);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Created supply item: {supply.Name} for user {supply.UserId}");

        // Check if already low stock and send notification
        if (supply.EnableLowStockAlert && supply.Status == StockStatus.LowStock)
        {
            await SendLowStockNotificationAsync(supply);
        }

        return supply;
    }

    public async Task<SupplyItem?> UpdateSupplyAsync(SupplyItem supply, string userId)
    {
        var existing = await GetSupplyByIdAsync(supply.Id, userId);
        if (existing == null)
        {
            return null;
        }

        var previousStatus = existing.Status;

        existing.Name = supply.Name;
        existing.Category = supply.Category;
        existing.Brand = supply.Brand;
        existing.Description = supply.Description;
        existing.TankId = supply.TankId;
        existing.CurrentQuantity = supply.CurrentQuantity;
        existing.MinimumQuantity = supply.MinimumQuantity;
        existing.OptimalQuantity = supply.OptimalQuantity;
        existing.Unit = supply.Unit;
        existing.PreferredVendor = supply.PreferredVendor;
        existing.LastPurchasePrice = supply.LastPurchasePrice;
        existing.LastPurchaseDate = supply.LastPurchaseDate;
        existing.ProductUrl = supply.ProductUrl;
        existing.AverageUsagePerWeek = supply.AverageUsagePerWeek;
        existing.LastUsedDate = supply.LastUsedDate;
        existing.ExpirationDate = supply.ExpirationDate;
        existing.StorageLocation = supply.StorageLocation;
        existing.Notes = supply.Notes;
        existing.EnableLowStockAlert = supply.EnableLowStockAlert;
        existing.IsActive = supply.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Updated supply item: {supply.Name} (ID: {supply.Id})");

        // Check if status changed to low stock
        if (supply.EnableLowStockAlert &&
            previousStatus != StockStatus.LowStock &&
            existing.Status == StockStatus.LowStock)
        {
            await SendLowStockNotificationAsync(existing);
        }

        return existing;
    }

    public async Task<bool> DeleteSupplyAsync(int id, string userId)
    {
        var supply = await GetSupplyByIdAsync(id, userId);
        if (supply == null)
        {
            return false;
        }

        // Soft delete
        supply.IsActive = false;
        supply.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Deleted supply item: {supply.Name} (ID: {id})");
        return true;
    }

    #endregion

    #region Inventory Management

    public async Task<bool> UpdateQuantityAsync(int id, string userId, double newQuantity, string? notes = null)
    {
        var supply = await GetSupplyByIdAsync(id, userId);
        if (supply == null)
        {
            return false;
        }

        var previousStatus = supply.Status;
        supply.CurrentQuantity = newQuantity;
        supply.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(notes))
        {
            supply.Notes = string.IsNullOrEmpty(supply.Notes)
                ? notes
                : $"{supply.Notes}\n{DateTime.UtcNow:yyyy-MM-dd}: {notes}";
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Updated quantity for {supply.Name}: {newQuantity} {supply.Unit}");

        // Check if status changed to low stock
        if (supply.EnableLowStockAlert &&
            previousStatus != StockStatus.LowStock &&
            supply.Status == StockStatus.LowStock)
        {
            await SendLowStockNotificationAsync(supply);
        }

        return true;
    }

    public async Task<bool> AddQuantityAsync(int id, string userId, double amountToAdd, decimal? purchasePrice = null)
    {
        var supply = await GetSupplyByIdAsync(id, userId);
        if (supply == null)
        {
            return false;
        }

        supply.CurrentQuantity += amountToAdd;
        supply.LastPurchaseDate = DateTime.UtcNow;
        supply.UpdatedAt = DateTime.UtcNow;

        if (purchasePrice.HasValue)
        {
            supply.LastPurchasePrice = purchasePrice.Value;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Added {amountToAdd} {supply.Unit} to {supply.Name}. New quantity: {supply.CurrentQuantity}");
        return true;
    }

    public async Task<bool> RemoveQuantityAsync(int id, string userId, double amountToRemove)
    {
        var supply = await GetSupplyByIdAsync(id, userId);
        if (supply == null)
        {
            return false;
        }

        var previousStatus = supply.Status;
        supply.CurrentQuantity = Math.Max(0, supply.CurrentQuantity - amountToRemove);
        supply.LastUsedDate = DateTime.UtcNow;
        supply.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Removed {amountToRemove} {supply.Unit} from {supply.Name}. New quantity: {supply.CurrentQuantity}");

        // Check if status changed to low stock
        if (supply.EnableLowStockAlert &&
            previousStatus != StockStatus.LowStock &&
            supply.Status == StockStatus.LowStock)
        {
            await SendLowStockNotificationAsync(supply);
        }

        return true;
    }

    #endregion

    #region Low Stock Alerts

    public async Task<List<SupplyItem>> GetLowStockItemsAsync(string userId)
    {
        var allSupplies = await GetSuppliesByUserAsync(userId);
        return allSupplies
            .Where(s => s.EnableLowStockAlert &&
                       (s.Status == StockStatus.LowStock || s.Status == StockStatus.OutOfStock))
            .OrderBy(s => s.CurrentQuantity)
            .ToList();
    }

    public async Task<List<SupplyItem>> GetOutOfStockItemsAsync(string userId)
    {
        var allSupplies = await GetSuppliesByUserAsync(userId);
        return allSupplies
            .Where(s => s.Status == StockStatus.OutOfStock)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToList();
    }

    public async Task<List<SupplyItem>> GetExpiringSoonItemsAsync(string userId, int daysAhead = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);

        return await _context.SupplyItems
            .Include(s => s.Tank)
            .Where(s => s.UserId == userId &&
                       s.IsActive &&
                       s.ExpirationDate.HasValue &&
                       s.ExpirationDate.Value <= cutoffDate &&
                       s.ExpirationDate.Value >= DateTime.UtcNow)
            .OrderBy(s => s.ExpirationDate)
            .ToListAsync();
    }

    private async Task SendLowStockNotificationAsync(SupplyItem supply)
    {
        try
        {
            var notification = new Notification
            {
                UserId = supply.UserId,
                Type = NotificationType.General,
                Title = $"Low Stock Alert: {supply.Name}",
                Message = $"Your {supply.Name} is running low. Current quantity: {supply.CurrentQuantity} {supply.Unit}. Minimum: {supply.MinimumQuantity} {supply.Unit}.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationService.CreateNotificationAsync(notification);
            _logger.LogInformation($"Sent low stock notification for {supply.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send low stock notification for {supply.Name}");
        }
    }

    #endregion

    #region Statistics

    public async Task<int> GetTotalSupplyCountAsync(string userId)
    {
        return await _context.SupplyItems
            .Where(s => s.UserId == userId && s.IsActive)
            .CountAsync();
    }

    public async Task<int> GetLowStockCountAsync(string userId)
    {
        var allSupplies = await GetSuppliesByUserAsync(userId);
        return allSupplies
            .Count(s => s.EnableLowStockAlert &&
                       (s.Status == StockStatus.LowStock || s.Status == StockStatus.OutOfStock));
    }

    public async Task<decimal> GetTotalInventoryValueAsync(string userId)
    {
        var supplies = await _context.SupplyItems
            .Where(s => s.UserId == userId && s.IsActive && s.LastPurchasePrice.HasValue)
            .ToListAsync();

        return supplies.Sum(s => (decimal)s.CurrentQuantity * (s.LastPurchasePrice ?? 0));
    }

    #endregion

    #region Search

    public async Task<List<SupplyItem>> SearchSuppliesAsync(string userId, string searchTerm)
    {
        searchTerm = searchTerm.ToLower();

        return await _context.SupplyItems
            .Include(s => s.Tank)
            .Where(s => s.UserId == userId &&
                       s.IsActive &&
                       (s.Name.ToLower().Contains(searchTerm) ||
                        (s.Brand != null && s.Brand.ToLower().Contains(searchTerm)) ||
                        (s.Description != null && s.Description.ToLower().Contains(searchTerm))))
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    #endregion

    #region Automatic Usage Tracking

    /// <summary>
    /// Automatically deduct supply quantity when used in dosing or maintenance.
    /// This is called when a DosingRecord is created to automatically track inventory.
    /// </summary>
    public async Task<bool> RecordSupplyUsageAsync(int supplyId, double amountUsed, string userId, string notes = "")
    {
        try
        {
            var supply = await GetSupplyByIdAsync(supplyId, userId);
            if (supply == null)
            {
                _logger.LogWarning($"Attempted to record usage for non-existent supply ID: {supplyId}");
                return false;
            }

            // Update quantity
            var previousStatus = supply.Status;
            supply.CurrentQuantity = Math.Max(0, supply.CurrentQuantity - amountUsed);
            supply.LastUsedDate = DateTime.UtcNow;
            supply.UpdatedAt = DateTime.UtcNow;

            // Update notes with usage log
            var usageNote = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm}: Used {amountUsed} {supply.Unit}";
            if (!string.IsNullOrEmpty(notes))
            {
                usageNote += $" - {notes}";
            }
            
            supply.Notes = string.IsNullOrEmpty(supply.Notes) 
                ? usageNote 
                : $"{supply.Notes}\n{usageNote}";

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Recorded usage of {amountUsed} {supply.Unit} for {supply.Name}");

            // Check if status changed to low stock and send notification
            if (supply.EnableLowStockAlert && 
                previousStatus != StockStatus.LowStock && 
                supply.Status == StockStatus.LowStock)
            {
                await SendLowStockNotificationAsync(supply);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error recording supply usage for supply ID: {supplyId}");
            return false;
        }
    }

    /// <summary>
    /// Get supplies that can be used for a specific purpose (e.g., dosing additives).
    /// Filters by category and active status.
    /// </summary>
    public async Task<List<SupplyItem>> GetSuppliesForDosingAsync(string userId)
    {
        return await _context.SupplyItems
            .Include(s => s.Tank)
            .Where(s => s.UserId == userId && 
                       s.IsActive &&
                       (s.Category == SupplyCategory.Supplements ||
                        s.Category == SupplyCategory.WaterTreatment ||
                        s.Category == SupplyCategory.Chemicals ||
                        s.Category == SupplyCategory.Medications))
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    #endregion
}
