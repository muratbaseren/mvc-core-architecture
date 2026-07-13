using App.Infrastructure.Identity;
using App.Modules.Admin.Models;
using App.Modules.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Admin.Controllers;

/// <summary>
/// Kullanıcı yönetimi ekranları. Yalnızca Admin rolü erişebilir.
/// </summary>
[Authorize(Roles = IdentitySeeder.AdminRole)]
public class AdminController(UserManager<AppUser> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        var model = new List<UserListItem>();
        foreach (var user in users)
        {
            model.Add(new UserListItem
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                IsAdmin = await userManager.IsInRoleAsync(user, IdentitySeeder.AdminRole),
                IsLockedOut = await userManager.IsLockedOutAsync(user)
            });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdmin(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        if (user.Id == userManager.GetUserId(User))
        {
            TempData["Error"] = "Kendi yetkinizi değiştiremezsiniz.";
            return RedirectToAction(nameof(Index));
        }

        if (await userManager.IsInRoleAsync(user, IdentitySeeder.AdminRole))
        {
            await userManager.RemoveFromRoleAsync(user, IdentitySeeder.AdminRole);
            TempData["Success"] = $"{user.Email} kullanıcısından Admin rolü alındı.";
        }
        else
        {
            await userManager.AddToRoleAsync(user, IdentitySeeder.AdminRole);
            TempData["Success"] = $"{user.Email} kullanıcısına Admin rolü verildi.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        if (user.Id == userManager.GetUserId(User))
        {
            TempData["Error"] = "Kendi hesabınızı kilitleyemezsiniz.";
            return RedirectToAction(nameof(Index));
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            await userManager.SetLockoutEndDateAsync(user, null);
            TempData["Success"] = $"{user.Email} hesabının kilidi açıldı.";
        }
        else
        {
            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            TempData["Success"] = $"{user.Email} hesabı kilitlendi.";
        }

        return RedirectToAction(nameof(Index));
    }
}
