using AdventureWorks.AppHost;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.DependencyInjection;

const string SqlServerHealthCheckKey = "sql-server-tcp";
const int SqlServerPort = 1433;
const int AngularDevServerPort = 4200;

var builder = DistributedApplication.CreateBuilder(args);

RegisterServices(builder);
AddSqlServer(builder);
var defaultConnection = builder.AddConnectionString("DefaultConnection");
AddMigrations(builder);
AddApi(builder, defaultConnection);
AddAngularWeb(builder);

builder.Build().Run();

/// <summary>
/// Registers AppHost-level DI services before resources are constructed.
/// </summary>
static void RegisterServices(IDistributedApplicationBuilder builder)
{
    builder.Services.AddSingleton<IDistributedApplicationLifecycleHook, ExternalContainerLifecycleHook>();
    builder.Services.AddHealthChecks()
        .AddCheck(SqlServerHealthCheckKey, new TcpPortHealthCheck("localhost", SqlServerPort));
}

/// <summary>
/// Adds the external SQL Server container as a monitored dashboard tile.
/// </summary>
/// <remarks>
/// Aspire does not own this container — OrbStack does. Reading the container name from
/// <c>SqlServer:ContainerName</c> lets each developer match their local setup without
/// modifying code.
/// </remarks>
static void AddSqlServer(IDistributedApplicationBuilder builder)
{
    var containerName = builder.Configuration["SqlServer:ContainerName"] ?? "tosk-mssql";
    builder.AddResource(new ExternalContainerResource(containerName))
        .WithAnnotation(new HealthCheckAnnotation(SqlServerHealthCheckKey));
}

/// <summary>
/// Adds the DbUp migration runner.
/// </summary>
/// <remarks>
/// WithExplicitStart prevents migrations from firing on every AppHost launch — accidental
/// restarts should never silently re-execute scripts against a populated database.
/// DbUp reads its connection string from its own user secrets, keeping its credentials
/// independent of the API.
/// </remarks>
static void AddMigrations(IDistributedApplicationBuilder builder)
{
    builder.AddProject<Projects.AdventureWorks_DbUp>("dbup")
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
        .WithExplicitStart();
}

/// <summary>
/// Adds the .NET API and wires the shared database connection string.
/// </summary>
static void AddApi(IDistributedApplicationBuilder builder, IResourceBuilder<IResourceWithConnectionString> defaultConnection)
{
    builder.AddProject<Projects.AdventureWorks_API>("api")
        .WithReference(defaultConnection)
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");
}

/// <summary>
/// Adds the Angular dev server.
/// </summary>
/// <remarks>
/// isProxied: false is required because Aspire's reverse proxy intercepts WebSocket
/// upgrades, which breaks Angular's HMR connection.
/// </remarks>
static void AddAngularWeb(IDistributedApplicationBuilder builder)
{
    builder.AddNpmApp("angular-web", "../../../apps/angular-web", "start")
        .WithHttpEndpoint(port: AngularDevServerPort, name: "http", isProxied: false)
        .WithEnvironment("NODE_ENV", "development");
}
