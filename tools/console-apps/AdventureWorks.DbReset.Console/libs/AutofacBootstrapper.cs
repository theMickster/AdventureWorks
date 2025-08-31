using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Migration;
using AdventureWorks.DbReset.Console.Resolution;
using AdventureWorks.DbReset.Console.Safety;
using AdventureWorks.DbReset.Console.Verbs.Handlers;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace AdventureWorks.DbReset.Console.Libs;

internal static class AutofacBootstrapper
{
    public static IContainer Build(IConfiguration configuration, DbResetOptions options, string repoRoot)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(repoRoot);

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
        builder.RegisterType<Snapshot.Internal.SqlScriptExecutor>()
            .As<Snapshot.Internal.ISqlScriptExecutor>()
            .SingleInstance();
        builder.RegisterType<SqlSourceMarkerProbe>().As<ISourceMarkerProbe>().SingleInstance();
        builder.RegisterType<Snapshot.LocalSqlServerSnapshotProvider>()
            .As<Snapshot.IDatabaseSnapshotProvider>()
            .SingleInstance();
        builder.RegisterType<RepoRootResolver>().AsSelf().SingleInstance();
        builder.RegisterType<TargetResolver>().AsSelf().SingleInstance();

        // Verb handlers (Story #926). Stateless — singletons are fine.
        builder.RegisterType<VerifyBaselineHandler>()
            .As<IVerifyBaselineHandler>()
            .SingleInstance();
        builder.RegisterType<SnapshotHandler>()
            .As<ISnapshotHandler>()
            .SingleInstance();
        builder.RegisterType<RestoreHandler>()
            .As<IRestoreHandler>()
            .SingleInstance();

        builder.RegisterInstance(repoRoot).Named<string>("repoRoot").SingleInstance();

        builder.RegisterType<DbUpProcessRunner>()
            .As<IDbUpProcessRunner>()
            .SingleInstance();

        builder.RegisterType<MigrateHandler>()
            .As<IMigrateHandler>()
            .SingleInstance()
            .WithParameter(new ResolvedParameter(
                (pi, _) => pi.ParameterType == typeof(string) && pi.Name == "repoRoot",
                (_, ctx) => ctx.ResolveNamed<string>("repoRoot")));

        builder.RegisterType<ResetHandler>()
            .As<IResetHandler>()
            .SingleInstance();

        return builder.Build();
    }
}
