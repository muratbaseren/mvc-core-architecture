using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.SharedKernel.Modules;

/// <summary>
/// Her modülün uygulaması gereken sözleşme. Modül assembly'si yüklendiğinde
/// bu arayüzü uygulayan sınıf bulunur ve servisleri otomatik kaydedilir.
/// </summary>
public interface IModule
{
    /// <summary>Modülün görünen adı.</summary>
    string Name { get; }

    /// <summary>Modülün versiyonu.</summary>
    string Version { get; }

    /// <summary>Modüle özel servis kayıtları burada yapılır.</summary>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>Modülün ana menüye ekleyeceği bağlantılar (opsiyonel).</summary>
    IEnumerable<ModuleMenuItem> MenuItems => [];
}
