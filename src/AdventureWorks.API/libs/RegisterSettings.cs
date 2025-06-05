using AdventureWorks.Common.Helpers;
using AdventureWorks.Common.Settings;

[assembly: InternalsVisibleTo("AdventureWorks.UnitTests")]
namespace AdventureWorks.API.libs;

[ExcludeFromCodeCoverage]
internal static class RegisterSettings
{
    internal static WebApplicationBuilder RegisterCommonSettings(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AkvExampleSettings>(config => config.MyFavoriteComedicMovie = SecretHelper.GetSecret("mick-favorite-comedy-movie"));
        
        return builder;
    }
}