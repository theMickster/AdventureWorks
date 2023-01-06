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

builder.Configuration
    .LoadApplicationConfiguration();

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

builder.RegisterAspDotNetServices();

builder.RegisterAdventureWorksDbContexts();

builder.RegisterAdventureWorksServices();

builder.RegisterAdventureWorksRepositories();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.RegisterConfigurations();

var app = builder.Build();

app.SetupMiddleware()
    .Run();