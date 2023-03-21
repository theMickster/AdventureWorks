using AdventureWorks.Application.Exceptions;
using AdventureWorks.Common.Constants;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using System.Diagnostics;

namespace AdventureWorks.API.libs;

/// <summary>
/// Intended for use during .NET Core API startup to add logging 
/// </summary>
[ExcludeFromCodeCoverage]
internal static class SetupLogging
{
    internal static IServiceCollection AddAdventureWorksLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (!ValidateRegisteredServices(services))
        {
            throw new InvalidOperationException(
                "IHttpContextAccessor was not added to the IServiceCollection in the correct order. " +
                "The concrete implementation of IHttpContextAccessor must be registered before logging.");
        }

        var appInsightsInstrumentationKey = configuration[ConfigurationConstants.AppInsightsInstrumentationKey] ?? string.Empty;
        var appInsightsConnectionString = configuration[ConfigurationConstants.AppInsightsConnectionString] ?? string.Empty;
        var aspNetEnvironment = configuration.RetrieveEnvironment();

        if (string.IsNullOrWhiteSpace(appInsightsInstrumentationKey))
        {
            throw new ConfigurationException(
                $"The required Configuration value for {ConfigurationConstants.AppInsightsInstrumentationKey} is missing." +
                "Please verify local or Azure resource configuration.");
        }

        if (string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            throw new ConfigurationException(
                $"The required Configuration value for {ConfigurationConstants.AppInsightsConnectionString} is missing." +
                "Please verify local or Azure resource configuration.");
        }

        if (string.IsNullOrWhiteSpace(aspNetEnvironment))
        {
            throw new ConfigurationException(
                $"The required Configuration value for {ConfigurationConstants.ApplicationEnvironment01} or " +
                $"{ConfigurationConstants.ApplicationEnvironment02} is missing." +
                "Please verify local or Azure resource configuration.");
        }

        if (!configuration.GetSection(LogSettingsConstants.BaseLogLevelDefault).Exists())
        {
            configuration[LogSettingsConstants.BaseLogLevelDefault] =
                LogLevelConstants.BaseLogLevelDefault.ToString();
        }

        if (!configuration.GetSection(LogSettingsConstants.BaseLogLevelMicrosoft).Exists())
        {
            configuration[LogSettingsConstants.BaseLogLevelMicrosoft] =
                LogLevelConstants.BaseLogLevelMicrosoft.ToString();
        }

        if (!configuration.GetSection(LogSettingsConstants.BaseLogLevelMicrosoftHostingLifetime).Exists())
        {
            configuration[LogSettingsConstants.BaseLogLevelMicrosoftHostingLifetime] =
                LogLevelConstants.BaseLogLevelMicrosoftHostingLifetime.ToString();
        }

        _ = services.AddLogging(logBuilder =>
        {
            logBuilder.AddConfiguration(configuration.GetSection(LogSettingsConstants.Logging));

            if (configuration.GetValue(LogSettingsConstants.UseDebugLog, false))
            {
                logBuilder.AddDebug();
                Trace.TraceInformation("Add Authenticator Logging:: Debug logging enabled");
            }

            if (configuration.GetValue(LogSettingsConstants.UseConsoleLog, false))
            {
                logBuilder.AddConsole();
                Trace.TraceInformation("Add Authenticator Logging:: Console logging enabled");
            }

            var options = new ApplicationInsightsServiceOptions
            {
                ConnectionString = appInsightsConnectionString
            };

            _ = services.AddApplicationInsightsTelemetry(options);

        });
        return services;
    }
    

    private static string RetrieveEnvironment(this IConfiguration configuration)
    {
        var environments = new[] {
            configuration[ConfigurationConstants.ApplicationEnvironment01] ?? string.Empty,
            configuration[ConfigurationConstants.ApplicationEnvironment02] ?? string.Empty
        };

        return environments.FirstOrDefault(environment => !string.IsNullOrWhiteSpace(environment)) ?? string.Empty;
    }

    private static bool ValidateRegisteredServices(IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (services.All(s => s.ServiceType != typeof(IHttpContextAccessor)))
        {
            Trace.TraceInformation(
                "IHttpContextAccessor was not added to the IServiceCollection in the correct order." +
                "The concrete implementation of IHttpContextAccessor must be registered before logging.");

            return false;
        }

        return true;
    }
}
