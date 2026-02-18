using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AquaHub.MVC.Models;
using AquaHub.MVC.Data;

namespace AquaHub.MVC.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                var latestEntry = await _context.JournalEntries
                    .Include(j => j.Tank)
                    .Where(j => j.Tank!.UserId == userId)
                    .OrderByDescending(j => j.Timestamp)
                    .FirstOrDefaultAsync();

                ViewBag.LatestJournalEntry = latestEntry;
            }
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
