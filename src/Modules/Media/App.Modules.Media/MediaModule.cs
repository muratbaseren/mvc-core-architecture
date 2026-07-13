using App.Application.Abstractions;
using App.Modules.Media.Services;
using App.SharedKernel.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Modules.Media;

/// <summary>
/// Medya modülü: dosya yükleme/listeleme/silme ve IFileStorage'ın
/// yerel disk implementasyonu.
/// </summary>
public class MediaModule : IModule
{
    public string Name => "Medya";
    public string Version => "1.0.0";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFileStorage, LocalFileStorage>();
    }

    public IEnumerable<ModuleMenuItem> MenuItems =>
    [
        new("Medya", "/Media", Order: 30)
    ];
}
