using System;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Services.Interfaces;

public interface IExpenseService
{
// CRUD operations
    Task<List<Expense>> GetAllExpensesAsync(string userId);
    Task<List<Expense>> GetExpensesByTankAsync(int tankId, string userId);
    Task<Expense?> GetExpenseByIdAsync(int expenseId, string userId);
    Task<Expense> AddExpenseAsync(Expense expense, string userId);
    Task<Expense?> UpdateExpenseAsync(Expense expense, string userId);
    Task<bool> DeleteExpenseAsync(int expenseId, string userId);

    // Filtering and querying
    Task<List<Expense>> GetExpensesByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
    Task<List<Expense>> GetExpensesByCategoryAsync(string userId, ExpenseCategory category);
    Task<List<Expense>> GetRecentExpensesAsync(string userId, int count = 10);

    // Analytics and summaries
    Task<ExpenseSummary> GetExpenseSummaryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ExpenseSummary> GetTankExpenseSummaryAsync(int tankId, string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<ExpenseCategory, decimal>> GetExpensesByCategoryTotalsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, decimal>> GetMonthlySpendingAsync(string userId, int months = 12);
}

public class ExpenseSummary
{
    public decimal TotalAmount { get; set; }
    public int TotalExpenses { get; set; }
    public decimal AverageExpense { get; set; }
    public ExpenseCategory? TopCategory { get; set; }
    public decimal TopCategoryAmount { get; set; }
    public string? TopVendor { get; set; }
    public decimal TopVendorAmount { get; set; }

    // Period comparison
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? PreviousPeriodTotal { get; set; }
    public decimal? PercentageChange { get; set; }

    // Category breakdown
    public Dictionary<ExpenseCategory, decimal> CategoryTotals { get; set; } = new();
    public Dictionary<ExpenseCategory, int> CategoryCounts { get; set; } = new();

    // Monthly breakdown
    public Dictionary<string, decimal> MonthlyTotals { get; set; } = new();
}

