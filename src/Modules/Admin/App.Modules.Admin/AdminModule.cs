using App.Modules.Admin.Services;
using App.SharedKernel.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Modules.Admin;

/// <summary>
/// Yönetim modülü: kullanıcı listeleme, rol atama ve hesap kilitleme.
/// Başlangıçta "Admin" rolünü ve varsayılan yönetici hesabını seed eder.
/// </summary>
public class AdminModule : IModule
{
    public string Name => "Yönetim";
    public string Version => "1.0.0";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<IdentitySeeder>();
    }

    public IEnumerable<ModuleMenuItem> MenuItems =>
    [
        new("Yönetim", "/Admin", Order: 90)
    ];
}
