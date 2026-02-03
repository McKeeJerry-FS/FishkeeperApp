using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class ExpenseController : Controller
{
    private readonly IExpenseService _expenseService;
    private readonly ITankService _tankService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ExpenseController> _logger;

    public ExpenseController(
        IExpenseService expenseService,
        ITankService tankService,
        UserManager<AppUser> userManager,
        ILogger<ExpenseController> logger)
    {
        _expenseService = expenseService;
        _tankService = tankService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Expense
    public async Task<IActionResult> Index(int? tankId, ExpenseCategory? category)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            List<Expense> expenses;

            if (tankId.HasValue)
            {
                expenses = await _expenseService.GetExpensesByTankAsync(tankId.Value, userId);
                var tank = await _tankService.GetTankByIdAsync(tankId.Value, userId);
                ViewBag.TankName = tank?.Name;
                ViewBag.TankId = tankId.Value;
            }
            else if (category.HasValue)
            {
                expenses = await _expenseService.GetExpensesByCategoryAsync(userId, category.Value);
                ViewBag.CategoryFilter = category.Value;
            }
            else
            {
                expenses = await _expenseService.GetAllExpensesAsync(userId);
            }

            return View(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expenses");
            TempData["Error"] = "An error occurred while retrieving expenses.";
            return View(new List<Expense>());
        }
    }

    // GET: Expense/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var expense = await _expenseService.GetExpenseByIdAsync(id, userId);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense details for ID: {ExpenseId}", id);
            TempData["Error"] = "An error occurred while retrieving expense details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Expense/Summary
    public async Task<IActionResult> Summary(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var summary = await _expenseService.GetExpenseSummaryAsync(userId, startDate, endDate);
            var categoryTotals = await _expenseService.GetExpensesByCategoryTotalsAsync(userId, startDate, endDate);
            var monthlySpending = await _expenseService.GetMonthlySpendingAsync(userId);

            ViewBag.CategoryTotals = categoryTotals;
            ViewBag.MonthlySpending = monthlySpending;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense summary");
            TempData["Error"] = "An error occurred while retrieving expense summary.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Expense/Create
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

            // Pre-populate with current date
            var expense = new Expense
            {
                Date = DateTime.Now
            };

            return View(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create expense view");
            TempData["Error"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Expense/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Date,Amount,Category,Description,Vendor,PaymentMethod,TankId")] Expense expense)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                var createdExpense = await _expenseService.AddExpenseAsync(expense, userId);
                TempData["Success"] = "Expense recorded successfully!";
                return RedirectToAction(nameof(Details), new { id = createdExpense.Id });
            }

            await PopulateTanksDropdown(userId);
            return View(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            TempData["Error"] = "An error occurred while recording the expense.";
            await PopulateTanksDropdown(_userManager.GetUserId(User)!);
            return View(expense);
        }
    }

    // GET: Expense/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var expense = await _expenseService.GetExpenseByIdAsync(id, userId);
            if (expense == null)
            {
                return NotFound();
            }

            await PopulateTanksDropdown(userId);
            return View(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense for edit, ID: {ExpenseId}", id);
            TempData["Error"] = "An error occurred while retrieving the expense.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Expense/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Date,Amount,Category,Description,Vendor,PaymentMethod,TankId")] Expense expense)
    {
        if (id != expense.Id)
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
                await _expenseService.UpdateExpenseAsync(expense, userId);
                TempData["Success"] = "Expense updated successfully!";
                return RedirectToAction(nameof(Details), new { id = expense.Id });
            }

            await PopulateTanksDropdown(userId);
            return View(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense ID: {ExpenseId}", id);
            TempData["Error"] = "An error occurred while updating the expense.";
            await PopulateTanksDropdown(_userManager.GetUserId(User)!);
            return View(expense);
        }
    }

    // GET: Expense/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var expense = await _expenseService.GetExpenseByIdAsync(id, userId);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense for deletion, ID: {ExpenseId}", id);
            TempData["Error"] = "An error occurred while retrieving the expense.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Expense/Delete/5
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

            var result = await _expenseService.DeleteExpenseAsync(id, userId);
            if (result)
            {
                TempData["Success"] = "Expense deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete expense.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense ID: {ExpenseId}", id);
            TempData["Error"] = "An error occurred while deleting the expense.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task PopulateTanksDropdown(string userId)
    {
        var tanks = await _tankService.GetAllTanksAsync(userId);
        ViewBag.Tanks = new SelectList(tanks, "Id", "Name");
    }
}
