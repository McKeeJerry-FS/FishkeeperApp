using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AquaHub.MVC.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace AquaHub.MVC.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        private readonly string _avatarPath = "wwwroot/images/avatars";

        public ProfileController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            if (!Directory.Exists(_avatarPath))
            {
                Directory.CreateDirectory(_avatarPath);
            }
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();
            return View(user);
        }

        // GET: /Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();
            return View(user);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUser model, IFormFile? avatarFile)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();


            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Bio = model.Bio;
            user.SocialLinks = model.SocialLinks;

            // Handle avatar upload
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var ext = Path.GetExtension(avatarFile.FileName);
                var fileName = $"{user.Id}{ext}";
                var filePath = Path.Combine(_avatarPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }
                user.AvatarUrl = $"/images/avatars/{fileName}";
            }
            else
            {
                user.AvatarUrl = model.AvatarUrl;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return RedirectToAction(nameof(Index));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }
    }
}
