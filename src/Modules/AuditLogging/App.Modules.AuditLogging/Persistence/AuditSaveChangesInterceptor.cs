using System.Text.Json;
using App.Modules.AuditLogging.Domain;
using App.SharedKernel.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace App.Modules.AuditLogging.Persistence;

/// <summary>
/// SaveChanges sırasında BaseEntity türevi tüm ekleme/güncelleme/silme
/// işlemlerini AuditLog tablosuna yazar. Infrastructure katmanı, DI'a
/// kayıtlı IInterceptor'ları DbContext'e otomatik ekler — modül silinirse
/// denetim kaydı da kendiliğinden devre dışı kalır.
/// </summary>
public class AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditLogs(DbContext? context)
    {
        if (context is null)
            return;

        var userName = httpContextAccessor.HttpContext?.User.Identity?.Name;

        var logs = context.ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity is not AuditLog &&
                        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => new AuditLog
            {
                EntityName = e.Entity.GetType().Name,
                EntityId = e.Entity.Id.ToString(),
                Action = e.State.ToString(),
                Changes = SerializeChanges(e),
                UserName = userName
            })
            .ToList();

        if (logs.Count > 0)
            context.Set<AuditLog>().AddRange(logs);
    }

    private static string? SerializeChanges(EntityEntry<BaseEntity> entry)
    {
        var changes = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    changes[property.Metadata.Name] = property.CurrentValue;
                    break;
                case EntityState.Deleted:
                    changes[property.Metadata.Name] = property.OriginalValue;
                    break;
                case EntityState.Modified when property.IsModified:
                    changes[property.Metadata.Name] = new
                    {
                        Eski = property.OriginalValue,
                        Yeni = property.CurrentValue
                    };
                    break;
            }
        }

        return changes.Count == 0 ? null : JsonSerializer.Serialize(changes);
    }
}
