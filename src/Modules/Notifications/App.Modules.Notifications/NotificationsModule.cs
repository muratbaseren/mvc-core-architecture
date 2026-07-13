using App.Application.Abstractions;
using App.Modules.Notifications.Services;
using App.SharedKernel.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace App.Modules.Notifications;

/// <summary>
/// Bildirim modülü: EntityChangedEvent'leri uygulama içi bildirime çevirir,
/// SMTP yapılandırması varsa IEmailService'i gerçek e-posta gönderimiyle değiştirir.
/// </summary>
public class NotificationsModule : IModule
{
    public string Name => "Bildirimler";
    public string Version => "1.0.0";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // SMTP yapılandırılmışsa dev amaçlı LogEmailService'in yerine geç.
        if (!string.IsNullOrWhiteSpace(configuration["Email:Smtp:Host"]))
            services.Replace(ServiceDescriptor.Scoped<IEmailService, SmtpEmailService>());
    }

    public IEnumerable<ModuleMenuItem> MenuItems =>
    [
        new("Bildirimler", "/Notifications", Order: 70)
    ];
}
