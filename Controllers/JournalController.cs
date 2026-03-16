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
    private readonly IWebHostEnvironment _environment;

    public JournalController(
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        ILogger<JournalController> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _environment = environment;
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
    public async Task<IActionResult> Create(
        [Bind("TankId,Title,Content,Timestamp")] JournalEntry journalEntry,
        IFormFile? ImageFile,
        IFormFile? CameraImageFile)
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
                var selectedImage = ImageFile ?? CameraImageFile;
                if (selectedImage != null)
                {
                    journalEntry.ImagePath = await SaveJournalImageAsync(selectedImage);

                    if (string.IsNullOrWhiteSpace(journalEntry.ImagePath))
                    {
                        ModelState.AddModelError(string.Empty, "The image could not be uploaded.");
                        throw new InvalidOperationException("Image upload failed for journal entry creation.");
                    }
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
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,TankId,Title,Content,Timestamp")] JournalEntry journalEntry,
        IFormFile? ImageFile,
        IFormFile? CameraImageFile)
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

        var existingJournalEntry = await _context.JournalEntries
            .Include(j => j.Tank)
            .FirstOrDefaultAsync(j => j.Id == id && j.Tank!.UserId == userId);

        if (existingJournalEntry == null)
        {
            return NotFound();
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
                existingJournalEntry.TankId = journalEntry.TankId;
                existingJournalEntry.Title = journalEntry.Title;
                existingJournalEntry.Content = journalEntry.Content;
                existingJournalEntry.Timestamp = journalEntry.Timestamp;

                var selectedImage = ImageFile ?? CameraImageFile;
                if (selectedImage != null)
                {
                    var previousImagePath = existingJournalEntry.ImagePath;
                    existingJournalEntry.ImagePath = await SaveJournalImageAsync(selectedImage);

                    if (string.IsNullOrWhiteSpace(existingJournalEntry.ImagePath))
                    {
                        ModelState.AddModelError(string.Empty, "The image could not be uploaded.");
                        throw new InvalidOperationException("Image upload failed for journal entry update.");
                    }

                    if (!string.Equals(previousImagePath, existingJournalEntry.ImagePath, StringComparison.OrdinalIgnoreCase))
                    {
                        DeleteImageIfExists(previousImagePath);
                    }
                }

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

        journalEntry.ImagePath = existingJournalEntry.ImagePath;

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

    private async Task<string?> SaveJournalImageAsync(IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return null;
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "journal");
        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(imageFile.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".jpg";
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await imageFile.CopyToAsync(stream);

        return $"/images/journal/{uniqueFileName}";
    }

    private void DeleteImageIfExists(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return;
        }

        var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
