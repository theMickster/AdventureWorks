using AdventureWorks.Application.Exceptions;
using AdventureWorks.Application.Infrastructure.AutoMapper;
using AdventureWorks.Application.Interfaces.DbContext;
using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.Address;
using AdventureWorks.Application.Services.Address;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Settings;
using AdventureWorks.Domain.Profiles;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text;
using AdventureWorks.Application.Validators;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using AdventureWorks.Infrastructure.Persistence.Repositories;
using FluentValidation;

[assembly: InternalsVisibleTo("AdventureWorks.Test.UnitTests")]
namespace AdventureWorks.API.libs;

internal static class RegisterServices
{
    [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Because we said so.")]
    internal static WebApplicationBuilder RegisterAspDotNetServices(this WebApplicationBuilder builder)
    {
        // ******* Access the configuration manager *******
        var config = builder.Configuration;

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
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        builder.Services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });


        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(
                "v1",
                MakeOpenApiInfo("Adventure Works API",
                                "v1",
                                "API",
                                new Uri("http://hello-world.info")));

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            c.TagActionsBy(api =>
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

            c.DocInclusionPredicate((name, api) => true);
        });

        builder.Services.AddAutoMapper(
            new[]
            {
                typeof(AutoMapperProfile).GetTypeInfo().Assembly,
                typeof(AddressEntityToAddressModelProfile).GetTypeInfo().Assembly
            });

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
            options.UseSqlServer(currentConnectionString));

        builder.Services.AddScoped<IAdventureWorksDbContext>(
            provider => provider.GetService<AdventureWorksDbContext>() ?? 
                        throw new ConfigurationException("The AdventureWorksDbContext is not properly registered in the correct order."));

        return builder;
    }

    internal static WebApplicationBuilder RegisterAdventureWorksServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IReadAddressService, ReadAddressService>();
        builder.Services.AddScoped<ICreateAddressService, CreateAddressService>();

        return builder;
    }

    internal static WebApplicationBuilder RegisterAdventureWorksRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepository<>));


        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IAddressRepository, AddressRepository>();
        builder.Services.AddScoped<IStateProvinceRepository, StateProvinceRepository>();

        return builder;
    }

    #region Private Methods 

    private static OpenApiInfo MakeOpenApiInfo(string title, string version, string description, Uri releaseNotes)
    {
        var oai = new OpenApiInfo
        {
            Title = title,
            Version = version,
            Contact = new OpenApiContact { Email = "bug.bashing.anonymous@outlook.com", Name = "Bug Bashing Anonymous" },
            Description = description
        };

        if (releaseNotes != null)
        {
            oai.Contact.Url = releaseNotes;
        }

        return oai;
    }

    /// <summary>
    /// Re-write the swagger index page adding a nonce
    /// </summary>
    /// <param name="options"></param>
    /// <param name="nonceString"></param>
    private static void RewriteSwaggerIndexHtml(SwaggerUIOptions options, string nonceString)
    {
        var originalIndexStreamFactory = options.IndexStream;

        options.IndexStream = () =>
        {
            using var originalStream = originalIndexStreamFactory();
            using var originalStreamReader = new StreamReader(originalStream);
            var originalIndexHtmlContents = originalStreamReader.ReadToEnd();

            var nonceEnabledIndexHtmlContents = originalIndexHtmlContents
                .Replace("<script>", $"<script nonce=\"{nonceString}\">", StringComparison.OrdinalIgnoreCase)
                .Replace("<style>", $"<style nonce=\"{nonceString}\">", StringComparison.OrdinalIgnoreCase);

            return new MemoryStream(Encoding.UTF8.GetBytes(nonceEnabledIndexHtmlContents));
        };
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