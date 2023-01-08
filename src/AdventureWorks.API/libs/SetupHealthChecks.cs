using AdventureWorks.Application.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("AdventureWorks.Test.UnitTests")]
namespace AdventureWorks.API.libs;

[ExcludeFromCodeCoverage]
internal static class SetupHealthChecks
{
    /// <summary>
    /// Add the default health check.
    /// </summary>
    /// <param name="services"><see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /></param>
    /// <returns><see cref="T:Microsoft.Extensions.DependencyInjection.IHealthChecksBuilder" /></returns>
    public static
#nullable disable
    IHealthChecksBuilder AddDefaultHealthCheck(
      this IServiceCollection services)
    {
        return services.AddHealthChecks().AddCheck<DefaultHealthCheck>("default");
    }

    /// <summary>
    /// Add the default health check with an optional func to perform some check for health.
    /// </summary>
    /// <param name="services"><see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /></param>
    /// <param name="statusFunc">A delegate function that will be evaluated as part of the health check.</param>
    /// <returns><see cref="T:Microsoft.Extensions.DependencyInjection.IHealthChecksBuilder" /></returns>
    public static IHealthChecksBuilder AddDefaultHealthCheck(
      this IServiceCollection services,
      Func<(HealthStatus status, Dictionary<string, object> deps)> statusFunc)
    {
        return services.AddHealthChecks().AddTypeActivatedCheck<DefaultHealthCheck>("default", (object)statusFunc);
    }

    /// <summary>
    /// Add the default health check endpoint and version endpoint
    /// </summary>
    /// <param name="application"><see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" /></param>
    /// <returns><see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" /></returns>
    public static IApplicationBuilder UseApplicationHealthChecks(
      this IApplicationBuilder application)
    {
        application.UseEndpoints(ep =>
        {
            ep.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, health) =>
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(health)).ConfigureAwait(false);
                }
            });

            ep.Map("/", async (ctx) =>
            {
                ctx.Response.ContentType = "text/plain";
                await ctx.Response.WriteAsync(MetadataAccessor.ProgramMetadata.SemanticVersion).ConfigureAwait(false);
            });

            ep.Map("/version", async (ctx) =>
            {
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync(JsonConvert.SerializeObject(MetadataAccessor.ProgramMetadata)).ConfigureAwait(false);
            });
        });
        return application;
    }
}