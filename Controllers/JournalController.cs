using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;

namespace AquaHub.MVC.Controllers;

[Authorize]
public class JournalController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<JournalController> _logger;

    public JournalController(
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        ILogger<JournalController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Journal
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var entries = await _context.JournalEntries
            .Include(j => j.Tank)
            .Where(j => j.Tank!.UserId == userId)
            .OrderByDescending(j => j.Timestamp)
            .ToListAsync();

        return View(entries);
    }

    // GET: Journal/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var journalEntry = await _context.JournalEntries
            .Include(j => j.Tank)
            .Include(j => j.MaintenanceLinks)
                .ThenInclude(m => m.MaintenanceLog)
            .Include(j => j.WaterTestLinks)
                .ThenInclude(w => w.WaterTest)
            .FirstOrDefaultAsync(j => j.Id == id && j.Tank!.UserId == userId);

        if (journalEntry == null)
        {
            return NotFound();
        }

        return View(journalEntry);
    }

    // GET: Journal/Create
    public async Task<IActionResult> Create()
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

        if (!tanks.Any())
        {
            TempData["ErrorMessage"] = "You must create a tank before adding journal entries.";
            return RedirectToAction("Create", "Tank");
        }

        ViewBag.TankId = new SelectList(tanks, "Id", "Name");

        var model = new JournalEntry
        {
            Timestamp = DateTime.UtcNow
        };

        return View(model);
    }

    // POST: Journal/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TankId,Title,Content,Timestamp,ImagePath")] JournalEntry journalEntry, IFormFile? ImageFile)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Verify the tank belongs to the user
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == journalEntry.TankId && t.UserId == userId);

        if (tank == null)
        {
            ModelState.AddModelError("TankId", "Invalid tank selected.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/journal");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"journal_{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    // Save relative path for web access
                    journalEntry.ImagePath = $"/images/journal/{uniqueFileName}";
                }

                _context.Add(journalEntry);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Journal entry created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating journal entry");
                ModelState.AddModelError("", "An error occurred while saving the journal entry.");
            }
        }

        var tanks = await _context.Tanks
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync();

        ViewBag.TankId = new SelectList(tanks, "Id", "Name", journalEntry.TankId);
        return View(journalEntry);
    }

    // GET: Journal/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var journalEntry = await _context.JournalEntries
            .Include(j => j.Tank)
            .FirstOrDefaultAsync(j => j.Id == id && j.Tank!.UserId == userId);

        if (journalEntry == null)
        {
            return NotFound();
        }

        var tanks = await _context.Tanks
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync();

        ViewBag.TankId = new SelectList(tanks, "Id", "Name", journalEntry.TankId);
        return View(journalEntry);
    }

    // POST: Journal/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,TankId,Title,Content,Timestamp,ImagePath")] JournalEntry journalEntry, IFormFile? ImageFile)
    {
        if (id != journalEntry.Id)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Verify the tank belongs to the user
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == journalEntry.TankId && t.UserId == userId);

        if (tank == null)
        {
            ModelState.AddModelError("TankId", "Invalid tank selected.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/journal");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"journal_{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    // Save relative path for web access
                    journalEntry.ImagePath = $"/images/journal/{uniqueFileName}";
                }

                _context.Update(journalEntry);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Journal entry updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JournalEntryExists(journalEntry.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating journal entry");
                ModelState.AddModelError("", "An error occurred while updating the journal entry.");
            }
        }

        var tanks = await _context.Tanks
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync();

        ViewBag.TankId = new SelectList(tanks, "Id", "Name", journalEntry.TankId);
        return View(journalEntry);
    }

    // GET: Journal/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var journalEntry = await _context.JournalEntries
            .Include(j => j.Tank)
            .FirstOrDefaultAsync(j => j.Id == id && j.Tank!.UserId == userId);

        if (journalEntry == null)
        {
            return NotFound();
        }

        return View(journalEntry);
    }

    // POST: Journal/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var journalEntry = await _context.JournalEntries
            .Include(j => j.Tank)
            .FirstOrDefaultAsync(j => j.Id == id && j.Tank!.UserId == userId);

        if (journalEntry != null)
        {
            _context.JournalEntries.Remove(journalEntry);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Journal entry deleted successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool JournalEntryExists(int id)
    {
        return _context.JournalEntries.Any(e => e.Id == id);
    }
}
