using App.Application.Abstractions;
using App.Infrastructure.Identity;
using App.Infrastructure.Persistence;
using App.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Veri erişimi (SQLite + EF Core), Identity ve altyapı servislerini kaydeder.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? "Data Source=app.db";

        // DbContextFactory: GraphQL resolver'ları gibi paralel çalışan tüketiciler
        // kendi context örneğini üretebilsin diye. Scoped AppDbContext kaydı da
        // factory üzerinden yapılır (MVC + Identity bu kaydı kullanır).
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddScoped(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEmailService, LogEmailService>();

        services
            .AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    /// <summary>
    /// Veritabanı şemasını oluşturur (yoksa). Modül entity'leri dahil.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
