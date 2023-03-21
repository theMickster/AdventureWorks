using AdventureWorks.Application.Exceptions;
using AdventureWorks.Common.Helpers;
using AdventureWorks.Common.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

[assembly: InternalsVisibleTo("AdventureWorks.UnitTests")]
namespace AdventureWorks.API.libs;

/// <summary>
/// Extension methods for setting up authentication services in an <see cref="IServiceCollection" />.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class RegisterAuthentication
{
    /// <summary>
    /// Registers services required by authentication services.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/>.</param>
    /// <returns>A <see cref="WebApplicationBuilder"/> that can be used to further configure the application.</returns>
    [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Because we said so.")]
    internal static WebApplicationBuilder RegisterApiAuthentication(this WebApplicationBuilder builder)
    {
        var tokenKey = SecretHelper.GetSecret("adventure-works-jwt-signing-key");
        var tokenSettings = builder.Configuration.GetSection(TokenSettings.SettingsRootName)
            .Get<TokenSettings>();

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

        var secretKey = Encoding.UTF8.GetBytes(tokenKey);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.RequireHttpsMetadata = false;
            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = tokenSettings.Issuer,
                ValidAudience = tokenSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true
            };
        });

        return builder;
    }
}
