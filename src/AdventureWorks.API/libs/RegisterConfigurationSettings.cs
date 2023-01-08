using AdventureWorks.Common.Helpers;
using AdventureWorks.Common.Settings;

[assembly: InternalsVisibleTo("AdventureWorks.Test.UnitTests")]
namespace AdventureWorks.API.libs;

internal static class RegisterConfigurationSettings
{
    internal static WebApplicationBuilder RegisterConfigurations(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AkvExampleSettings>(
            config => config.MyFavoriteComedicMovie =
                SecretHelper.GetSecret("mick-favorite-comedy-movie")
        );

        return builder;
    }
}