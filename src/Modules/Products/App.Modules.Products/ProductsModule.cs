using App.SharedKernel.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Modules.Products;

/// <summary>
/// Ürün yönetimi modülü. Modül yükleyici bu sınıfı keşfeder;
/// entity'ler, MediatR handler'ları, controller ve view'lar
/// bu assembly'den otomatik toplanır.
/// </summary>
public class ProductsModule : IModule
{
    public string Name => "Ürünler";
    public string Version => "1.0.0";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Modüle özel servisler burada kaydedilir (şimdilik gerek yok).
    }

    public IEnumerable<ModuleMenuItem> MenuItems =>
    [
        new("Ürünler", "/Products", Order: 10)
    ];
}
