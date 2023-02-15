using AdventureWorks.Application.Interfaces.Services.Login;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AdventureWorks.Domain.Models.Shield;

namespace AdventureWorks.Application.Services.Login;

[ServiceLifetimeScoped]
public sealed class TokenService : ITokenService
{
    private readonly IOptionsSnapshot<TokenSettings> _tokenSettings;

    public TokenService( IOptionsSnapshot<TokenSettings> tokenSettings)
    {
        _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
    }

    /// <summary>
    /// Creates a JWT security token for the given <see cref="UserAccountModel"/>
    /// </summary>
    /// <param name="userAccount">the AdventureWorks user requesting the token</param>
    /// <returns>a valid AdventureWorks API token</returns>
    public UserAccountTokenModel GenerateUserToken(UserAccountModel userAccount)
    {
        var tokenExpiration = DateTime.UtcNow.AddSeconds(_tokenSettings.Value.TokenExpirationInSeconds);
        var secretKey = Encoding.UTF8.GetBytes(_tokenSettings.Value.Key);
        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, _tokenSettings.Value.Subject),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new Claim(JwtRegisteredClaimNames.Exp, tokenExpiration.ToString(CultureInfo.InvariantCulture)),
            new Claim(JwtRegisteredClaimNames.GivenName, userAccount.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, userAccount.LastName),
            new Claim("UserId", userAccount.Id.ToString()),
            new Claim("UserName", userAccount.UserName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = tokenExpiration,
            Issuer = _tokenSettings.Value.Issuer,
            Audience = _tokenSettings.Value.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        var model = new UserAccountTokenModel
        {
            Id = Guid.NewGuid(),
            Token = tokenHandler.WriteToken(token),
            TokenExpiration = tokenExpiration,
            RefreshToken = string.Empty,
            RefreshTokenExpiration = DateTime.MinValue
        };

        return model;
    }
}
