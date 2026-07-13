using System.Reflection;
using App.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application;

public static class DependencyInjection
{
    /// <summary>
    /// MediatR handler'larını ve FluentValidation validator'larını verilen
    /// assembly'lerden (çekirdek + tüm modüller) kaydeder.
    /// </summary>
    public static IServiceCollection AddApplicationCore(
        this IServiceCollection services,
        params IEnumerable<Assembly> assemblies)
    {
        var assemblyList = assemblies.ToArray();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            foreach (var assembly in assemblyList)
                cfg.RegisterServicesFromAssembly(assembly);
        });

        services.AddMemoryCache();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        foreach (var assembly in assemblyList)
            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
