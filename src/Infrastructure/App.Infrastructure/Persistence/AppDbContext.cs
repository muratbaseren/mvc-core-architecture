using App.Infrastructure.Identity;
using App.SharedKernel.Domain;
using App.SharedKernel.Modules;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence;

/// <summary>
/// Uygulamanın tek DbContext'i. Identity tablolarını içerir ve modül
/// assembly'lerindeki entity konfigürasyonlarını otomatik keşfeder.
/// Yeni bir modül eklendiğinde buraya dokunmak gerekmez.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Modüllerdeki IEntityTypeConfiguration<T> sınıflarını uygula.
        foreach (var assembly in ModuleRegistry.Assemblies)
            builder.ApplyConfigurationsFromAssembly(assembly);

        // Konfigürasyonu olmayan aggregate root'ları da modele dahil et.
        foreach (var entityType in ModuleRegistry.GetAggregateRootTypes())
        {
            if (builder.Model.FindEntityType(entityType) is null)
                builder.Entity(entityType);
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Audit alanlarını otomatik güncelle.
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = entry.Entity.CreatedAt == default
                        ? DateTime.UtcNow
                        : entry.Entity.CreatedAt;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
