using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Resolution;
using AdventureWorks.DbReset.Console.Safety;
using Autofac;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace AdventureWorks.DbReset.Console.Libs;

internal static class AutofacBootstrapper
{
    public static IContainer Build(IConfiguration configuration, DbResetOptions options)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(options);

        LogManager.Setup().LoadConfigurationFromFile(
            Path.Combine(AppContext.BaseDirectory, "nlog.config"),
            optional: true);

        var builder = new ContainerBuilder();

        builder.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();
        builder.RegisterInstance(options).AsSelf().SingleInstance();

        // Logging: NLog factory + open-generic Logger<T> for ILogger<T>.
        builder.RegisterType<NLogLoggerFactory>()
            .As<ILoggerFactory>()
            .InstancePerLifetimeScope();

        builder.RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>));

        // Services.
        builder.RegisterType<ConfigurationValidator>().AsSelf().SingleInstance();
        builder.RegisterType<DualRoleSafetyValidator>().AsSelf().SingleInstance();
        builder.RegisterType<SqlSourceMarkerProbe>().As<ISourceMarkerProbe>().SingleInstance();
        builder.RegisterType<RepoRootResolver>().AsSelf().SingleInstance();
        builder.RegisterType<TargetResolver>().AsSelf().SingleInstance();

        return builder.Build();
    }
}
