using App.Modules.AuditLogging.Persistence;
using App.SharedKernel.Modules;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Modules.AuditLogging;

/// <summary>
/// Denetim kaydı modülü. EF Core SaveChanges interceptor'ı ile tüm entity
/// değişikliklerini kaydeder ve /Audit ekranında gösterir.
/// </summary>
public class AuditLoggingModule : IModule
{
    public string Name => "Denetim Kayıtları";
    public string Version => "1.0.0";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IInterceptor, AuditSaveChangesInterceptor>();
    }

    public IEnumerable<ModuleMenuItem> MenuItems =>
    [
        new("Denetim", "/Audit", Order: 80)
    ];
}
