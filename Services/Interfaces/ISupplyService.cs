using AquaHub.MVC.Models;

namespace AquaHub.MVC.Services.Interfaces;

public interface ISupplyService
{
    // CRUD operations
    Task<List<SupplyItem>> GetSuppliesByUserAsync(string userId);
    Task<List<SupplyItem>> GetSuppliesByTankAsync(int tankId, string userId);
    Task<List<SupplyItem>> GetSuppliesByCategoryAsync(string userId, Models.Enums.SupplyCategory category);
    Task<SupplyItem?> GetSupplyByIdAsync(int id, string userId);
    Task<SupplyItem> CreateSupplyAsync(SupplyItem supply);
    Task<SupplyItem?> UpdateSupplyAsync(SupplyItem supply, string userId);
    Task<bool> DeleteSupplyAsync(int id, string userId);

    // Inventory management
    Task<bool> UpdateQuantityAsync(int id, string userId, double newQuantity, string? notes = null);
    Task<bool> AddQuantityAsync(int id, string userId, double amountToAdd, decimal? purchasePrice = null);
    Task<bool> RemoveQuantityAsync(int id, string userId, double amountToRemove);

    // Low stock alerts
    Task<List<SupplyItem>> GetLowStockItemsAsync(string userId);
    Task<List<SupplyItem>> GetOutOfStockItemsAsync(string userId);
    Task<List<SupplyItem>> GetExpiringSoonItemsAsync(string userId, int daysAhead = 30);

    // Statistics
    Task<int> GetTotalSupplyCountAsync(string userId);
    Task<int> GetLowStockCountAsync(string userId);
    Task<decimal> GetTotalInventoryValueAsync(string userId);

    // Search
    Task<List<SupplyItem>> SearchSuppliesAsync(string userId, string searchTerm);
}
