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
        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, _tokenSettings.Value.Subject),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Exp, tokenExpiration.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.GivenName, userAccount.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, userAccount.LastName),
            new("UserId", userAccount.Id.ToString()),
            new("UserName", userAccount.UserName)
        };

        if (userAccount.SecurityRoles != null && userAccount.SecurityRoles.Any())
        {
            claims.AddRange(userAccount.SecurityRoles.Select(x => new Claim(ClaimTypes.Role, x.Name)));
        }

        if (userAccount.SecurityFunctions != null && userAccount.SecurityFunctions.Any())
        {
            claims.AddRange(userAccount.SecurityFunctions.Select(x => new Claim("SecurityFunction", x.Name)));
        }

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
