using AdventureWorks.Application.Exceptions;
using AdventureWorks.Common.Helpers;
using AdventureWorks.Common.Settings;

[assembly: InternalsVisibleTo("AdventureWorks.UnitTests")]
namespace AdventureWorks.API.libs;

[ExcludeFromCodeCoverage]
internal static class RegisterSettings
{
    internal static WebApplicationBuilder RegisterCommonSettings(this WebApplicationBuilder builder)
    {
        // ******* Access the configuration manager *******
        var configuration = builder.Configuration;

        builder.Services.Configure<AkvExampleSettings>(
            config => config.MyFavoriteComedicMovie =
                SecretHelper.GetSecret("mick-favorite-comedy-movie")
        );

        var tokenKey = SecretHelper.GetSecret("adventure-works-jwt-signing-key");
        var tokenSettings = configuration.GetSection(TokenSettings.SettingsRootName);

        if (tokenKey == null)
        {
            throw new ConfigurationException(
                $"The required Configuration secret key for the Token Signing Key is missing." +
                "Please verify configuration.");
        }

        if (tokenSettings == null)
        {
            throw new ConfigurationException(
                $"The required Configuration settings keys for the Token Settings are missing." +
                "Please verify configuration.");
        }

        builder.Services.AddOptions<TokenSettings>().Bind(tokenSettings);

        builder.Services.PostConfigure<TokenSettings>(o =>
        {
            o.Key = tokenKey;
        });

        return builder;
    }
}