using App.SharedKernel.Modules;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;

namespace App.Web.Infrastructure.GraphQl;

public static class GraphQlSetup
{
    /// <summary>
    /// GraphQL sunucusunu kurar. Modüllerdeki tüm aggregate root entity'ler için
    /// otomatik sorgu alanları üretir; ayrıca modül assembly'lerindeki
    /// <see cref="ObjectTypeExtension"/> sınıflarını (ör. modüle özel mutation'lar)
    /// otomatik keşfedip şemaya ekler.
    /// </summary>
    public static IRequestExecutorBuilder AddModularGraphQl(this IServiceCollection services)
    {
        var builder = services
            .AddGraphQLServer()
            .AddQueryType(d => d
                .Name(OperationTypeNames.Query)
                .Field("health")
                .Resolve("OK"))
            .AddFiltering()
            .AddSorting();

        // Her modül entity'si için otomatik query alanları (liste + id ile getir).
        foreach (var entityType in ModuleRegistry.GetAggregateRootTypes())
        {
            builder.AddTypeExtension(
                typeof(EntityQueryExtension<>).MakeGenericType(entityType));
        }

        // Modüllerin kendi tanımladığı type extension'lar (mutation vb.).
        foreach (var assembly in ModuleRegistry.Assemblies)
        {
            var extensionTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
                            && typeof(ObjectTypeExtension).IsAssignableFrom(t));

            foreach (var extensionType in extensionTypes)
                builder.AddTypeExtension(extensionType);
        }

        return builder;
    }
}
