using AdventureWorks.Application.Exceptions;
using AdventureWorks.Application.Http;
using AdventureWorks.Application.Interfaces.DbContext;
using AdventureWorks.Application.Interfaces.Http;
using AdventureWorks.Application.Validators.Address;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Settings;
using AdventureWorks.Domain.Profiles;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net.Http.Headers;
using System.Reflection;

[assembly: InternalsVisibleTo("AdventureWorks.UnitTests")]
namespace AdventureWorks.API.libs;

[ExcludeFromCodeCoverage]
internal static class RegisterServices
{
    [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Because we said so.")]
    internal static WebApplicationBuilder RegisterAspDotNetServices(this WebApplicationBuilder builder)
    {

        builder.Services.AddControllers(options =>
            {
                options.ReturnHttpNotAcceptable = true;
            })
            .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true)
            .AddXmlSerializerFormatters()
            .AddXmlDataContractSerializerFormatters();

        builder.Services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("x-api-version"),
                new MediaTypeApiVersionReader("x-api-version"));
        });
        
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                MakeOpenApiInfo("Adventure Works API",
                                "v1",
                                "API",
                                new Uri("http://hello-world.info")));
            
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            options.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                {
                    return new[] { api.GroupName };
                }

                if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    return new[] { controllerActionDescriptor.ControllerName };
                }

                throw new InvalidOperationException("Unable to determine tag for endpoint.");
            });

            options.DocInclusionPredicate((name, api) => true);
        });

        builder.Services.AddAutoMapper(typeof(AddressEntityToAddressModelProfile).GetTypeInfo().Assembly);
        builder.Services.AddValidatorsFromAssemblyContaining<CreateAddressValidator>();
        return builder;
    }

    internal static WebApplicationBuilder RegisterAdventureWorksDbContexts(this WebApplicationBuilder builder)
    {
        var connectionStrings = GetDatabaseConnectionStrings(builder.Configuration);

        builder.Services.AddOptions<EntityFrameworkCoreSettings>()
            .Bind(builder.Configuration.GetSection(EntityFrameworkCoreSettings.SettingsRootName));

        builder.Services.PostConfigure<EntityFrameworkCoreSettings>(o =>
        {
            o.DatabaseConnectionStrings = connectionStrings;
        });

        var currentConnectionString = GetSqlConnectionString(builder.Configuration, connectionStrings );

        builder.Services.AddDbContext<AdventureWorksDbContext>(options =>
            {
                options.UseSqlServer(currentConnectionString);
            }
        ); 

        builder.Services.AddScoped<IAdventureWorksDbContext>(
            provider => provider.GetService<AdventureWorksDbContext>() ?? 
                        throw new ConfigurationException("The AdventureWorksDbContext is not properly registered in the correct order."));

        return builder;
    }

    internal static WebApplicationBuilder RegisterServicesViaReflection(this WebApplicationBuilder builder)
    {
        var scoped = typeof(ServiceLifetimeScopedAttribute);
        var transient = typeof(ServiceLifetimeTransientAttribute);
        var singleton = typeof(ServiceLifetimeSingletonAttribute);

        var appServices = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.ManifestModule.Name.StartsWith("AdventureWorks."))

            .SelectMany(t => t.GetTypes())
            .Where(x => (x.IsDefined(scoped, false) ||
                         x.IsDefined(transient, false) ||
                         x.IsDefined(singleton, false)) && !x.IsInterface)
            .Select(y => new { InterfaceName = y.GetInterface($"I{y.Name}"), Service = y })
            .Where(z => z.InterfaceName != null)
            .ToList();

        appServices.ForEach(t =>
        {
            if (t.Service.IsDefined(scoped, false))
            {
                builder.Services.AddScoped(t.InterfaceName!, t.Service);
            }

            if (t.Service.IsDefined(transient, false))
            {
                builder.Services.AddTransient(t.InterfaceName!, t.Service);
            }

            if (t.Service.IsDefined(singleton, false))
            {
                builder.Services.AddSingleton(t.InterfaceName!, t.Service);
            }
        });
        return builder;
    }

    internal static WebApplicationBuilder AddHttpRequestSender(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddHttpClient<IHttpRequestSender, HttpRequestSender>("Authenticator",
                (serviceProvider, httpClient) =>
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(15);

                    httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Authenticator Agent");

                    httpClient.DefaultRequestHeaders
                        .Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                })
            .SetHandlerLifetime(TimeSpan.FromMinutes(3));

        return builder;
    }

    #region Private Methods

    private static OpenApiInfo MakeOpenApiInfo(string title, string version, string description, Uri releaseNotes)
    {
        var oai = new OpenApiInfo
        {
            Title = title,
            Version = version,
            Contact = new OpenApiContact
                { Email = "bug.bashing.anonymous@outlook.com", Name = "Bug Bashing Anonymous" },
            Description = description
        };

        if (releaseNotes != null)
        {
            oai.Contact.Url = releaseNotes;
        }

        return oai;
    }
    
    private static List<DatabaseConnectionString> GetDatabaseConnectionStrings(IConfiguration configuration)
    {
        var defaultConnectionString = 
            configuration.GetConnectionString(ConfigurationConstants.SqlConnectionDefaultConnectionName);

        var sqlAzureConnectionString = 
            configuration.GetConnectionString(ConfigurationConstants.SqlConnectionSqlAzureConnectionName);

        if (string.IsNullOrWhiteSpace(defaultConnectionString))
        {
            throw new ConfigurationException(
                $"The required Configuration value for {ConfigurationConstants.SqlConnectionDefaultConnectionName} is missing." +
                "Please verify database configuration.");
        }
        
        var connectionStrings = new List<DatabaseConnectionString>
        {
            new()
            {
                ConnectionStringName = ConfigurationConstants.SqlConnectionDefaultConnectionName,
                ConnectionString = defaultConnectionString
            }
        };

        if (!string.IsNullOrWhiteSpace(sqlAzureConnectionString))
        {
            connectionStrings.Add(new DatabaseConnectionString
            {
                ConnectionStringName = ConfigurationConstants.SqlConnectionSqlAzureConnectionName,
                ConnectionString = sqlAzureConnectionString
            });
        }

        return connectionStrings;
    }

    private static string GetSqlConnectionString(IConfiguration configuration, IEnumerable<DatabaseConnectionString> connectionStrings)
    {
        var settings = configuration.GetSection(EntityFrameworkCoreSettings.SettingsRootName);

        if (settings == null)
        {
            throw new ConfigurationException(
                $"The required Configuration settings keys for the Entity Framework Core Settings are missing." +
                "Please verify configuration.");
        }

        var connectionStringName = settings[ConfigurationConstants.CurrentConnectionStringNameKey] ?? 
                                   ConfigurationConstants.SqlConnectionDefaultConnectionName;

        var currentConnectionString = connectionStrings.FirstOrDefault(x =>
            x.ConnectionStringName == connectionStringName);

        if (currentConnectionString == null)
        {
            throw new ConfigurationException(
                $"The required Configuration settings keys for the Entity Framework Core Settings are missing." +
                "Please verify configuration.");
        }

        return currentConnectionString.ConnectionString;
    }

    #endregion Private Methods
}