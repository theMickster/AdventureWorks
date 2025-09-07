using AdventureWorks.Application.PersistenceContracts.DbContext;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AdventureWorks.IntegrationTests.Setup;

/// <summary>
/// In-process test host for the AdventureWorks API.
/// Replaces the SQL Server database with an isolated EF Core InMemory store and substitutes
/// <see cref="TestAuthHandler"/> for the production JWT Bearer scheme so tests run without
/// network dependencies or real credentials.
/// </summary>
/// <remarks>
/// Registered as an xUnit <see cref="ICollectionFixture{T}"/> via
/// <see cref="IntegrationTestCollection"/>, so one instance is shared across all test classes
/// in the <c>"Integration Tests"</c> collection. The InMemory database name is unique per
/// factory instance to prevent seed collisions when xUnit creates multiple factory instances
/// within the same process.
/// </remarks>
[ExcludeFromCodeCoverage]
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"AdventureWorks_{Guid.NewGuid():N}";

    /// <summary>
    /// Configures the test host to use InMemory persistence and a no-op authentication scheme.
    /// </summary>
    /// <remarks>
    /// EF Core 8+ registers <c>IDbContextOptionsConfiguration&lt;T&gt;</c> accumulatively via
    /// <c>AddSingleton</c>. Removing only <c>DbContextOptions&lt;T&gt;</c> leaves the SQL Server
    /// lambda active; both providers would then be applied simultaneously, causing a dual-provider
    /// error. All four related service registrations are removed before the InMemory provider is
    /// added.
    /// </remarks>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AdventureWorksDbContext>) ||
                    d.ServiceType == typeof(IDbContextOptionsConfiguration<AdventureWorksDbContext>) ||
                    d.ServiceType == typeof(AdventureWorksDbContext) ||
                    d.ServiceType == typeof(IAdventureWorksDbContext))
                .ToList();
            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            services.AddDbContext<AdventureWorksDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

            services.AddScoped<IAdventureWorksDbContext>(
                provider => provider.GetRequiredService<AdventureWorksDbContext>());

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName, _ => { });

            services.AddControllers();
        });
    }

    /// <summary>
    /// Builds the host and seeds the InMemory database with baseline test data before any test
    /// in the collection executes.
    /// </summary>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        SeedDatabaseAsync(host).GetAwaiter().GetResult();
        return host;
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> that presents valid test credentials on every request.
    /// The <see cref="TestAuthHandler"/> will authenticate the request and controllers protected
    /// by <c>[Authorize]</c> will proceed normally.
    /// </summary>
    public HttpClient CreateAuthenticatedClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    /// <summary>
    /// Creates an <see cref="HttpClient"/> that sends the <see cref="TestAuthHandler.AnonymousHeader"/>
    /// on every request, causing <see cref="TestAuthHandler"/> to return
    /// <see cref="AuthenticateResult.NoResult"/> and controllers protected by <c>[Authorize]</c>
    /// to respond with HTTP 401.
    /// </summary>
    public HttpClient CreateAnonymousClient()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add(TestAuthHandler.AnonymousHeader, "true");
        return client;
    }

    private static async Task SeedDatabaseAsync(IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AdventureWorksDbContext>();
        await context.Database.EnsureCreatedAsync();
        await TestDataSeeder.SeedAsync(context);
    }
}
