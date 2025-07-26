using AdventureWorks.Application.Behaviors;
using AdventureWorks.Application.Exceptions;
using AdventureWorks.Common.Constants;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorks.Application.Helpers;

[ExcludeFromCodeCoverage]
public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var autoMapperLicenseKey = configuration[ConfigurationConstants.AutoMapperLicenseKey];

        if (string.IsNullOrWhiteSpace(autoMapperLicenseKey))
        {
            throw new ConfigurationException($"The '{ConfigurationConstants.AutoMapperLicenseKey}' configuration value is required.");
        }

        services.AddAutoMapper(cfg => cfg.LicenseKey = autoMapperLicenseKey, AppDomain.CurrentDomain.GetAssemblies());
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            cfg.AddOpenBehavior(typeof(CorrelationIdLoggingBehavior<,>));
        });

        return services;
    }
}