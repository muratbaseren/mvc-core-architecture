using App.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.Modules.Admin.Services;

/// <summary>
/// Uygulama açılışında "Admin" rolünü ve varsayılan yönetici hesabını oluşturur.
/// Varsayılanlar appsettings üzerinden değiştirilebilir:
///   "Admin": { "Email": "...", "Password": "..." }
/// </summary>
public class IdentitySeeder(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<IdentitySeeder> logger) : IHostedService
{
    public const string AdminRole = "Admin";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
            logger.LogInformation("'{Role}' rolü oluşturuldu.", AdminRole);
        }

        var email = configuration["Admin:Email"] ?? "admin@local.dev";
        var password = configuration["Admin:Password"] ?? "Admin123!";

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = "Sistem Yöneticisi",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                logger.LogError("Varsayılan yönetici oluşturulamadı: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }
            logger.LogInformation("Varsayılan yönetici oluşturuldu: {Email}", email);
        }

        if (!await userManager.IsInRoleAsync(admin, AdminRole))
            await userManager.AddToRoleAsync(admin, AdminRole);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
