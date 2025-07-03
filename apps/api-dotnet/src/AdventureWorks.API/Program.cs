using AdventureWorks.API.libs;
using AdventureWorks.API.libs.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

#pragma warning disable S1118

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", false, true)
    .AddEnvironmentVariables()
    .AddInMemoryCollection();

builder.Services.AddOptions();

builder.Services.AddHttpContextAccessor();

builder.AddHttpRequestSender();

builder.Services.AddMemoryCache();

builder.AddAdventureWorksResponseCompression();

builder.Services.AddCorrelationIdServices();

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

builder.Services.AddAdventureWorksLogging(builder.Configuration);

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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration);

builder.RegisterAspDotNetServices();

builder.RegisterApiVersioning();

builder.RegisterAdventureWorksDbContexts();

builder.RegisterServicesViaReflection();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();

await app.SetupMiddleware().RunAsync();

/// <summary>
/// The entry point for the API.
/// </summary>
/// <remarks>
/// Declared this way to bypass the unit test code coverage analysis
/// </remarks>
[ExcludeFromCodeCoverage]
public partial class Program { }