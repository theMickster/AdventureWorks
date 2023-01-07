using Microsoft.AspNetCore.Mvc.Versioning;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Reflection;
using AdventureWorks.Application.Exceptions;
using AdventureWorks.Application.Infrastructure.AutoMapper;
using AdventureWorks.Application.Interfaces;
using AdventureWorks.Infrastructure.DbContexts;
using AdventureWorks.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using AdventureWorks.Domain.Profiles;

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

        return builder;
    }

    internal static WebApplicationBuilder RegisterAdventureWorksDbContexts(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AdventureWorksDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("AdventureWorksDatabase")));

        builder.Services.AddScoped<IAdventureWorksDbContext>(
            provider => provider.GetService<AdventureWorksDbContext>() ?? 
                        throw new ConfigurationException("The AdventureWorksDbContext is not properly registered in the correct order."));

        return builder;
    }

    internal static WebApplicationBuilder RegisterAdventureWorksServices(this WebApplicationBuilder builder)
    {


        return builder;
    }
    internal static WebApplicationBuilder RegisterAdventureWorksRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepository<>));


        builder.Services.AddScoped<IProductRepository, ProductRepository>();


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

    #endregion Private Methods 
}