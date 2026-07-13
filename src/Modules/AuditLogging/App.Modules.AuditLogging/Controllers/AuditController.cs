using App.Application.Abstractions;
using App.Modules.AuditLogging.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Modules.AuditLogging.Controllers;

[Authorize(Roles = "Admin")]
public class AuditController(IRepository<AuditLog> repository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? entity, CancellationToken cancellationToken)
    {
        var query = repository.Query()
            .OrderByDescending(l => l.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(entity))
            query = query.Where(l => l.EntityName == entity);

        var logs = query.Take(200).ToList();

        ViewBag.EntityNames = repository.Query()
            .Select(l => l.EntityName)
            .Distinct()
            .OrderBy(n => n)
            .ToList();
        ViewBag.SelectedEntity = entity;

        return View(logs);
    }
}
