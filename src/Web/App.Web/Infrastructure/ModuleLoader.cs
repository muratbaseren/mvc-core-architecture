using System.Reflection;
using App.SharedKernel.Modules;

namespace App.Web.Infrastructure;

/// <summary>
/// Çalışma dizinindeki <c>App.Modules.*.dll</c> assembly'lerini tarar,
/// içlerindeki <see cref="IModule"/> implementasyonlarını bulur ve
/// <see cref="ModuleRegistry"/>'ye kaydeder.
///
/// Modül keşfi dosya sistemine dayalıdır: bir modül klasörü silindiğinde
/// dll üretilmez ve modül uygulamadan tamamen kalkar.
/// </summary>
public static class ModuleLoader
{
    public static IReadOnlyList<IModule> LoadModules(Serilog.ILogger logger)
    {
        var moduleFiles = Directory.GetFiles(AppContext.BaseDirectory, "App.Modules.*.dll");

        foreach (var file in moduleFiles)
        {
            var assemblyName = System.IO.Path.GetFileNameWithoutExtension(file);
            var assembly = Assembly.Load(new AssemblyName(assemblyName));

            var moduleTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false }
                            && typeof(IModule).IsAssignableFrom(t));

            foreach (var moduleType in moduleTypes)
            {
                var module = (IModule)Activator.CreateInstance(moduleType)!;
                ModuleRegistry.Register(module, assembly);
                logger.Information(
                    "Modül yüklendi: {ModuleName} v{ModuleVersion} ({Assembly})",
                    module.Name, module.Version, assemblyName);
            }
        }

        if (ModuleRegistry.Modules.Count == 0)
            logger.Information("Hiç modül bulunamadı (src/Modules boş olabilir).");

        return ModuleRegistry.Modules;
    }
}
