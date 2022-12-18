using AdventureWorks.API.Controllers;
using AdventureWorks.Application.Infrastructure.AutoMapper;
using AdventureWorks.Application.Interfaces;
using AdventureWorks.Infrastructure.DbContexts;
using AdventureWorks.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AdventureWorks.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add AutoMapper
            services.AddAutoMapper(new Assembly[] { typeof(AutoMapperProfile).GetTypeInfo().Assembly });

            //services.AddCustomDbContext(Configuration);
            
            services.AddDbContext<AdventureWorksDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("AdventureWorksDatabase")));

            services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepository<>));

            services.AddScoped<IAdventureWorksDbContext>(provider => provider.GetService<AdventureWorksDbContext>());

            services.AddScoped<IProductRepository, ProductRepository>();

            services.AddControllers(options =>
            {
                //options.Filters.Add(typeof(ValidatorActionFilter));
                options.ReturnHttpNotAcceptable = true;
            })
                .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true)
                .AddXmlSerializerFormatters()
                .AddXmlDataContractSerializerFormatters()
                //.AddFluentValidation(s =>
                //{
                //    s.RegisterValidatorsFromAssemblyContaining<FluentValidator<AuthorCreateDto>>();
                //    s.DisableDataAnnotationsValidation = true;
                //})
                ;

            services.AddCors(options =>
            {
                options.AddPolicy("PubsCorsPolicy",
                    builder => builder
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddScoped<ILogger, Logger<ProductsController>>();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            //services.AddSwaggerGen(options =>
            //{
            //    options.SwaggerDoc("v1", new Info { Title = "AdventureWorks API", Version = "v1" });

            //    // Get xml comments path
            //    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            //    // Set xml path
            //    options.IncludeXmlComments(xmlPath);
            //});

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "AdventureWorks API V1");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
