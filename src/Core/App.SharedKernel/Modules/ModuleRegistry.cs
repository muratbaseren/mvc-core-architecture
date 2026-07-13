using System.Reflection;
using App.SharedKernel.Domain;

namespace App.SharedKernel.Modules;

/// <summary>
/// Keşfedilen modüllerin merkezi kayıt noktası. Web katmanındaki modül yükleyici
/// tarafından doldurulur; Infrastructure (EF Core) ve GraphQL katmanları buradan
/// modül assembly'lerine ve entity tiplerine ulaşır.
/// </summary>
public static class ModuleRegistry
{
    private static readonly List<IModule> _modules = [];
    private static readonly List<Assembly> _assemblies = [];

    public static IReadOnlyList<IModule> Modules => _modules;
    public static IReadOnlyList<Assembly> Assemblies => _assemblies;

    public static void Register(IModule module, Assembly assembly)
    {
        _modules.Add(module);
        if (!_assemblies.Contains(assembly))
            _assemblies.Add(assembly);
    }

    /// <summary>
    /// Tüm modül assembly'lerindeki aggregate root entity tiplerini döner.
    /// GraphQL şeması ve EF Core model keşfi için kullanılır.
    /// </summary>
    public static IEnumerable<Type> GetAggregateRootTypes() =>
        _assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && typeof(IAggregateRoot).IsAssignableFrom(t)
                        && typeof(BaseEntity).IsAssignableFrom(t));
}
