using AdventureWorks.API.libs;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", false, true)
    .AddEnvironmentVariables()
    .AddInMemoryCollection();

builder.Services.AddOptions();

builder.Services.AddMemoryCache();

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromMilliseconds(31536000);
});

builder.Configuration
    .AddUserSecrets<Program>()
    .Build();

builder.Configuration.RegisterApplicationConfiguration();

builder.RegisterCommonSettings();

builder.Services.AddDefaultHealthCheck();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AdventureWorksCorsPolicy",
        builder => builder
            .SetIsOriginAllowed((host) => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.RegisterApiAuthentication();

builder.RegisterAspDotNetServices();

builder.RegisterAdventureWorksDbContexts();

builder.RegisterServicesViaReflection();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();

app.SetupMiddleware()
    .Run();


/// <summary>
/// The entry point for the API.
/// </summary>
/// <remarks>
/// Declared this way to bypass the unit test code coverage analysis
/// </remarks>
[ExcludeFromCodeCoverage]
public partial class Program { }