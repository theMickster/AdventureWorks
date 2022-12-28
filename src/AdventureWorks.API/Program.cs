using AdventureWorks.API.libs;
using AdventureWorks.Application.Infrastructure.AutoMapper;
using System.Reflection;
using AdventureWorks.Application.Interfaces;
using AdventureWorks.Infrastructure.DbContexts;
using AdventureWorks.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
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


builder.Services.AddCors(options =>
{
    options.AddPolicy("PubsCorsPolicy",
        builder => builder
            .SetIsOriginAllowed((host) => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.RegisterServices();

builder.Services.AddAutoMapper(new Assembly[] { typeof(AutoMapperProfile).GetTypeInfo().Assembly });


builder.Services.AddDbContext<AdventureWorksDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AdventureWorksDatabase")));

builder.Services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepository<>));

builder.Services.AddScoped<IAdventureWorksDbContext>(provider => provider.GetService<AdventureWorksDbContext>());

builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});



var app = builder.Build();

app.SetupMiddleware()
    .Run();