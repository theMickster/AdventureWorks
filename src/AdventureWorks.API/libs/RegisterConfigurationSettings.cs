using AdventureWorks.Common.Helpers;
using AdventureWorks.Common.Settings;

namespace AdventureWorks.API.libs;

internal static class RegisterConfigurationSettings
{
    internal static WebApplicationBuilder RegisterConfigurations(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AkvExampleSettings>(
            config => config.MyFavoriteComedicMovie =
                SecretHelper.GetSecret("mick-favorite-comedy-movie")
        );

        builder.Services.Configure<EntityFrameworkCoreSettings>(
            o => builder.Configuration.GetSection(EntityFrameworkCoreSettings.SettingsRootName).Bind(o));

        builder.Services.PostConfigure<EntityFrameworkCoreSettings>(
            o =>
            {
                o.DatabaseConnectionStrings = GetDatabaseConnectionStrings(builder.Configuration);
            });

        return builder;
    }

    #region Private Methods

    private static List<DatabaseConnectionString> GetDatabaseConnectionStrings(IConfiguration configuration)
    {
        return new List<DatabaseConnectionString>
        {
            new()
            {
                ConnectionStringName = "DefaultConnection",
                ConnectionString = configuration.GetConnectionString("DefaultConnection")
            },
            new ()
            {
                ConnectionStringName = "SqlAzureConnection",
                ConnectionString = configuration.GetConnectionString("SqlAzureConnection")
            }
        };
    }

    #endregion Private Methods
}