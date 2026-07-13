using App.Application.Abstractions;
using App.Modules.Notifications.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Modules.Notifications.Controllers;

[Authorize]
public class NotificationsController(
    IRepository<Notification> repository,
    IUnitOfWork unitOfWork) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var notifications = repository.Query()
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToList();

        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var notification = await repository.GetByIdAsync(id, cancellationToken);
        if (notification is not null && !notification.IsRead)
        {
            notification.IsRead = true;
            repository.Update(notification);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var unread = await repository.ListAsync(n => !n.IsRead, cancellationToken);
        foreach (var notification in unread)
        {
            notification.IsRead = true;
            repository.Update(notification);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        TempData["Success"] = $"{unread.Count} bildirim okundu olarak işaretlendi.";
        return RedirectToAction(nameof(Index));
    }
}
