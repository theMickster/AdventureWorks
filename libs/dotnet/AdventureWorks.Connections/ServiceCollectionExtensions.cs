using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorks.Connections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConnectionCatalog(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ConnectionCatalogBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new ConnectionCatalogBuilder();
        configure(builder);
        var catalog = builder.Build(configuration);

        services.AddSingleton(catalog);
        return services;
    }
}
