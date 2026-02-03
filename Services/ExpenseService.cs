using System;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class ExpenseService : IExpenseService
{
    private readonly ApplicationDbContext _context;

    public ExpenseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Expense>> GetAllExpensesAsync(string userId)
    {
        return await _context.Expenses
            .Include(e => e.Tank)
            .Include(e => e.Equipment)
            .Include(e => e.Livestock)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<List<Expense>> GetExpensesByTankAsync(int tankId, string userId)
    {
        return await _context.Expenses
            .Include(e => e.Tank)
            .Include(e => e.Equipment)
            .Include(e => e.Livestock)
            .Where(e => e.TankId == tankId && e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<Expense?> GetExpenseByIdAsync(int expenseId, string userId)
    {
        return await _context.Expenses
            .Include(e => e.Tank)
            .Include(e => e.Equipment)
            .Include(e => e.Livestock)
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId);
    }

    public async Task<Expense> AddExpenseAsync(Expense expense, string userId)
    {
        expense.UserId = userId;
        expense.CreatedAt = DateTime.UtcNow;

        // Calculate unit price if not provided
        if (!expense.UnitPrice.HasValue && expense.Quantity > 0)
        {
            expense.UnitPrice = expense.Amount / expense.Quantity;
        }

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        return expense;
    }

    public async Task<Expense?> UpdateExpenseAsync(Expense expense, string userId)
    {
        var existingExpense = await GetExpenseByIdAsync(expense.Id, userId);
        if (existingExpense == null)
            return null;

        existingExpense.Date = expense.Date;
        existingExpense.Category = expense.Category;
        existingExpense.Amount = expense.Amount;
        existingExpense.Currency = expense.Currency;
        existingExpense.ItemName = expense.ItemName;
        existingExpense.Brand = expense.Brand;
        existingExpense.Quantity = expense.Quantity;
        existingExpense.UnitPrice = expense.UnitPrice;
        existingExpense.Vendor = expense.Vendor;
        existingExpense.PaymentMethod = expense.PaymentMethod;
        existingExpense.ReceiptNumber = expense.ReceiptNumber;
        existingExpense.Notes = expense.Notes;
        existingExpense.IsRecurring = expense.IsRecurring;
        existingExpense.RecurrenceFrequency = expense.RecurrenceFrequency;
        existingExpense.TankId = expense.TankId;
        existingExpense.EquipmentId = expense.EquipmentId;
        existingExpense.LivestockId = expense.LivestockId;
        existingExpense.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingExpense;
    }

    public async Task<bool> DeleteExpenseAsync(int expenseId, string userId)
    {
        var expense = await GetExpenseByIdAsync(expenseId, userId);
        if (expense == null)
            return false;

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Expense>> GetExpensesByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await _context.Expenses
            .Include(e => e.Tank)
            .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<List<Expense>> GetExpensesByCategoryAsync(string userId, ExpenseCategory category)
    {
        return await _context.Expenses
            .Include(e => e.Tank)
            .Where(e => e.UserId == userId && e.Category == category)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<List<Expense>> GetRecentExpensesAsync(string userId, int count = 10)
    {
        return await _context.Expenses
            .Include(e => e.Tank)
            .Include(e => e.Equipment)
            .Include(e => e.Livestock)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .Take(count)
            .ToListAsync();
    }

    public async Task<ExpenseSummary> GetExpenseSummaryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddYears(-1);
        var end = endDate ?? DateTime.UtcNow;

        var expenses = await GetExpensesByDateRangeAsync(userId, start, end);

        var summary = new ExpenseSummary
        {
            StartDate = start,
            EndDate = end,
            TotalAmount = expenses.Sum(e => e.Amount),
            TotalExpenses = expenses.Count,
            AverageExpense = expenses.Any() ? expenses.Average(e => e.Amount) : 0
        };

        // Category breakdown
        var categoryGroups = expenses.GroupBy(e => e.Category).ToList();
        foreach (var group in categoryGroups)
        {
            summary.CategoryTotals[group.Key] = group.Sum(e => e.Amount);
            summary.CategoryCounts[group.Key] = group.Count();
        }

        // Top category
        if (categoryGroups.Any())
        {
            var topCat = categoryGroups.OrderByDescending(g => g.Sum(e => e.Amount)).First();
            summary.TopCategory = topCat.Key;
            summary.TopCategoryAmount = topCat.Sum(e => e.Amount);
        }

        // Top vendor
        var vendorGroups = expenses.Where(e => !string.IsNullOrEmpty(e.Vendor))
            .GroupBy(e => e.Vendor)
            .OrderByDescending(g => g.Sum(e => e.Amount))
            .FirstOrDefault();

        if (vendorGroups != null)
        {
            summary.TopVendor = vendorGroups.Key;
            summary.TopVendorAmount = vendorGroups.Sum(e => e.Amount);
        }

        // Monthly breakdown
        var monthlyGroups = expenses.GroupBy(e => e.Date.ToString("yyyy-MM")).ToList();
        foreach (var group in monthlyGroups)
        {
            summary.MonthlyTotals[group.Key] = group.Sum(e => e.Amount);
        }

        // Previous period comparison
        var periodLength = (end - start).Days;
        var previousStart = start.AddDays(-periodLength);
        var previousEnd = start.AddDays(-1);
        var previousExpenses = await GetExpensesByDateRangeAsync(userId, previousStart, previousEnd);

        if (previousExpenses.Any())
        {
            summary.PreviousPeriodTotal = previousExpenses.Sum(e => e.Amount);
            if (summary.PreviousPeriodTotal > 0)
            {
                summary.PercentageChange = ((summary.TotalAmount - summary.PreviousPeriodTotal.Value) / summary.PreviousPeriodTotal.Value) * 100;
            }
        }

        return summary;
    }

    public async Task<ExpenseSummary> GetTankExpenseSummaryAsync(int tankId, string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddYears(-1);
        var end = endDate ?? DateTime.UtcNow;

        var expenses = await _context.Expenses
            .Where(e => e.TankId == tankId && e.UserId == userId && e.Date >= start && e.Date <= end)
            .OrderByDescending(e => e.Date)
            .ToListAsync();

        var summary = new ExpenseSummary
        {
            StartDate = start,
            EndDate = end,
            TotalAmount = expenses.Sum(e => e.Amount),
            TotalExpenses = expenses.Count,
            AverageExpense = expenses.Any() ? expenses.Average(e => e.Amount) : 0
        };

        // Category breakdown
        var categoryGroups = expenses.GroupBy(e => e.Category).ToList();
        foreach (var group in categoryGroups)
        {
            summary.CategoryTotals[group.Key] = group.Sum(e => e.Amount);
            summary.CategoryCounts[group.Key] = group.Count();
        }

        // Top category
        if (categoryGroups.Any())
        {
            var topCat = categoryGroups.OrderByDescending(g => g.Sum(e => e.Amount)).First();
            summary.TopCategory = topCat.Key;
            summary.TopCategoryAmount = topCat.Sum(e => e.Amount);
        }

        // Monthly breakdown
        var monthlyGroups = expenses.GroupBy(e => e.Date.ToString("yyyy-MM")).ToList();
        foreach (var group in monthlyGroups)
        {
            summary.MonthlyTotals[group.Key] = group.Sum(e => e.Amount);
        }

        return summary;
    }

    public async Task<Dictionary<ExpenseCategory, decimal>> GetExpensesByCategoryTotalsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddYears(-1);
        var end = endDate ?? DateTime.UtcNow;

        var expenses = await GetExpensesByDateRangeAsync(userId, start, end);

        return expenses
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
    }

    public async Task<Dictionary<string, decimal>> GetMonthlySpendingAsync(string userId, int months = 12)
    {
        var startDate = DateTime.UtcNow.AddMonths(-months);
        var expenses = await GetExpensesByDateRangeAsync(userId, startDate, DateTime.UtcNow);

        return expenses
            .GroupBy(e => e.Date.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
    }
}
