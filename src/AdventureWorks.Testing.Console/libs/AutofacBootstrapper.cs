using AdventureWorks.Application.Interfaces.DbContext;
using AdventureWorks.Common.Extensions;
using AdventureWorks.Common.Logging;
using AdventureWorks.Common.Settings;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using AdventureWorks.Testing.Console.Settings;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace AdventureWorks.Testing.Console.libs;

internal static class AutofacBootstrapper
{
    public static IServiceCollection BuildServiceCollection(IConfiguration configuration)
    {
        return new ServiceCollection()
            .AddLogging(cfg =>
            {
                cfg.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
                cfg.AddNLog(configuration);
            })

            .Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = Microsoft.Extensions.Logging.LogLevel.Information);
    }


    public static ContainerBuilder BuildContainer(
        this ContainerBuilder builder
        , IConfiguration appConfiguration)
    {
        RegisterConfigOptions(builder, appConfiguration);

        RegisterLoggers(builder, appConfiguration);

        RegisterCustomModules(builder);

        RegisterDbContexts(builder, appConfiguration);

        return builder;
    }


    #region Private Methods

    private static void RegisterConfigOptions(ContainerBuilder builder, IConfiguration configuration)
    {
        var entityFrameworkCoreSettings = new EntityFrameworkCoreSettings();
        configuration.GetSection(EntityFrameworkCoreSettings.SettingsRootName).Bind(entityFrameworkCoreSettings);

        builder.RegisterInstance(entityFrameworkCoreSettings).SingleInstance();
    }

    private static void RegisterCustomModules(ContainerBuilder builder)
    {
        builder.RegisterModule<AdventureWorksModule>();


    }

    private static void RegisterLoggers(ContainerBuilder builder, IConfiguration configuration)
    {
        LogManager.LoadConfiguration("nlog.config");

        var logger = LogManager.Setup()
            .LoadConfigurationFromSection(configuration)
            .GetCurrentClassLogger();

        logger.WithProperty("EventId_Id", Convert.ToInt32(LoggingEventId.ApplicationStartup))
            .WithProperty("EventId_Name", LoggingEventId.ApplicationStartup.GetDisplayName())
            .Info("Program startup (Environment: Integration Tests)");

        var loggerFactoryForDebugging = LoggerFactory.Create(factory =>
        {
            factory.ClearProviders();
            factory.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
            factory.AddConsole();
            factory.AddDebug();
            factory.AddNLog();
        });

        // Create Logger<T> when ILogger<T> is required.
        builder.RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>));

        // Use NLogLoggerFactory as a factory required by Logger<T>.
        builder.RegisterType<NLogLoggerFactory>()
            .AsImplementedInterfaces().InstancePerLifetimeScope();

    }

    private static void RegisterDbContexts(ContainerBuilder builder, IConfiguration configuration)
    {
        var defaultLevel = configuration.GetValue<string>("Logging:LogLevel:Default");

        var entityFrameworkLevel = configuration.GetValue<string>("Logging:LogLevel.Microsoft.EntityFrameworkCore.Database.Command");

        Microsoft.Extensions.Logging.LogLevel defaultLogLevel = Microsoft.Extensions.Logging.LogLevel.Error;
        Microsoft.Extensions.Logging.LogLevel entityFrameworkLogLevel = Microsoft.Extensions.Logging.LogLevel.Error;

        entityFrameworkLogLevel = entityFrameworkLevel switch
        {
            "Warning" => Microsoft.Extensions.Logging.LogLevel.Warning,
            "Information" => Microsoft.Extensions.Logging.LogLevel.Information,
            "Trace" => Microsoft.Extensions.Logging.LogLevel.Trace,
            "Debug" => Microsoft.Extensions.Logging.LogLevel.Debug,
            "Critical" => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => Microsoft.Extensions.Logging.LogLevel.Error,
        };

        defaultLogLevel = defaultLevel switch
        {
            "Warning" => Microsoft.Extensions.Logging.LogLevel.Warning,
            "Information" => Microsoft.Extensions.Logging.LogLevel.Information,
            "Trace" => Microsoft.Extensions.Logging.LogLevel.Trace,
            "Debug" => Microsoft.Extensions.Logging.LogLevel.Debug,
            "Critical" => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => Microsoft.Extensions.Logging.LogLevel.Error,
        };
        LogManager.LoadConfiguration("nlog.config");

        var loggerFactoryForDebugging = LoggerFactory.Create(factory =>
        {
            factory.ClearProviders();
            factory.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
            factory.AddConsole();
            factory.AddDebug();
            factory.AddNLog();
        });

        var loggerFactory = LoggerFactory.Create(factory =>
        {
            factory.ClearProviders();
            factory.SetMinimumLevel(defaultLogLevel);
            factory.AddFilter((category, level) =>
                category == DbLoggerCategory.Database.Command.Name
                && level == entityFrameworkLogLevel);
            factory.AddConsole();
            factory.AddDebug();
            factory.AddNLog();
        });

        var optionsBuilder = new DbContextOptionsBuilder<AdventureWorksDbContext>()
            .UseLoggerFactory(loggerFactory)
            .UseSqlServer(DbConnectionString.ConnectionString)
            .EnableDetailedErrors(true)
            .EnableSensitiveDataLogging(true)
            .Options;

        if (entityFrameworkLogLevel == Microsoft.Extensions.Logging.LogLevel.Debug ||
            entityFrameworkLogLevel == Microsoft.Extensions.Logging.LogLevel.Trace)
        {
            builder.RegisterType<AdventureWorksDbContext>()
                .WithParameter(new NamedParameter("options", optionsBuilder))
                .WithParameter(new NamedParameter("factory", loggerFactoryForDebugging))
                .As<IAdventureWorksDbContext>();

            builder.RegisterType<AdventureWorksDbContext>()
                .WithParameter(new NamedParameter("options", optionsBuilder))
                .WithParameter(new NamedParameter("factory", loggerFactoryForDebugging));
        }
        else
        {
            builder.RegisterType<AdventureWorksDbContext>()
                .WithParameter(new NamedParameter("options", optionsBuilder))
                .WithParameter(new NamedParameter("factory", loggerFactory))
                .As<IAdventureWorksDbContext>();

            builder.RegisterType<AdventureWorksDbContext>()
                .WithParameter(new NamedParameter("options", optionsBuilder))
                .WithParameter(new NamedParameter("factory", loggerFactory));
        }
    }

    #endregion Private Methods 


    #region Private Autoface Module Classes

    private class AdventureWorksModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(c => c.ManifestModule.Name.StartsWith("AdventureWorks"));

            foreach (var assembly in assemblies)
            {
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Services"))
                    .AsImplementedInterfaces();

                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Repository"))
                    .AsImplementedInterfaces();

                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Validator"))
                    .AsImplementedInterfaces();
            }
        }
    }

    #endregion Private Autoface Module Classes
}
