using AdventureWorks.API.libs;
using AdventureWorks.API.libs.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

#pragma warning disable S1118

var builder = WebApplication.CreateBuilder(args);

builder.AddAspireTelemetry();

var environment = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", false, true)
    .AddEnvironmentVariables()
    .AddInMemoryCollection();

builder.Services.AddOptions();

builder.Services.AddHttpContextAccessor();

builder.Services.AddUserContextServices();

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

var skipExternalConfig = !environment.IsEnvironment("Docker") && !environment.IsEnvironment("Testing");

if (skipExternalConfig)
{
    builder.Configuration.RegisterApplicationConfiguration();
}

builder.Services.AddAdventureWorksLogging(builder.Configuration);

if (skipExternalConfig)
{
    builder.RegisterCommonSettings();
}

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DashboardAccess", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events ??= new JwtBearerEvents();
    // JWT is extracted from the query string for all hub transports (WebSocket, SSE, Long Polling).
    // Browsers cannot set Authorization headers on WebSocket or SSE connections, so the SignalR
    // client appends the token as ?access_token=. Restricting to IsWebSocketRequest breaks SSE.
    // Accepted risk: token appears in server access logs; mitigated by short token lifetimes.
    options.Events.OnMessageReceived = context =>
    {
        var token = context.Request.Query["access_token"];
        if (!string.IsNullOrEmpty(token) &&
            context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
        {
            context.Token = token;
        }
        return Task.CompletedTask;
    };
});

builder.RegisterAdventureWorksDbContexts();

builder.RegisterAspDotNetServices();

builder.RegisterApiVersioning();

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