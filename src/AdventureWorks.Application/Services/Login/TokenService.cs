using AdventureWorks.Application.Interfaces.Repositories.AccountInfo;
using AdventureWorks.Application.Interfaces.Services.Login;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Settings;
using AdventureWorks.Domain.Entities.Shield;
using AdventureWorks.Domain.Models.Shield;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AdventureWorks.Application.Services.Login;

[ServiceLifetimeScoped]
public sealed class TokenService : ITokenService
{
    private readonly IOptionsSnapshot<TokenSettings> _tokenSettings;
    private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;

    public TokenService( IOptionsSnapshot<TokenSettings> tokenSettings, IUserRefreshTokenRepository userRefreshTokenRepository)
    {
        _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
        _userRefreshTokenRepository = userRefreshTokenRepository ?? throw new ArgumentNullException(nameof(userRefreshTokenRepository));
    }

    /// <summary>
    /// Creates a JWT security token for the given <see cref="UserAccountModel"/>
    /// </summary>
    /// <param name="userAccount">the AdventureWorks user requesting the token</param>
    /// <param name="ipAddress">the IP of the remote host making the token request</param>
    /// <returns>a valid AdventureWorks API token</returns>
    public async Task<UserAccountTokenModel> GenerateUserTokenAsync(UserAccountModel userAccount, string ipAddress)
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

        var refreshToken = await CreateUserRefreshTokenAsync(userAccount.Id, ipAddress);

        var model = new UserAccountTokenModel
        {
            Id = Guid.NewGuid(),
            Token = tokenHandler.WriteToken(token),
            TokenExpiration = tokenExpiration,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = DateTime.MinValue
        };

        return model;
    }

    private async Task<string> CreateUserRefreshTokenAsync(int userId, string ipAddress)
    {
        var token = Guid.NewGuid().ToString("N");
        await Task.Delay(50);
        token += Guid.NewGuid().ToString("N");
        token += ipAddress.Replace(".", string.Empty);
        token = token.ToUpper();

        var userRefreshToken = new UserRefreshTokenEntity
        {
            BusinessEntityId = userId,
            RecordId = Guid.NewGuid(),
            RefreshToken = token,
            IpAddress = ipAddress,
            ExpiresOn = DateTime.UtcNow.AddDays(_tokenSettings.Value.RefreshTokenExpirationInDays),
            CreatedBy = userId,
            CreatedOn = DateTime.UtcNow,
            ModifiedBy = userId,
            ModifiedOn = DateTime.UtcNow
        };

        var entity = await _userRefreshTokenRepository.AddAsync(userRefreshToken);

        return entity == null ? throw new InvalidOperationException("Unable to create user refresh token") : token;
    }
}
