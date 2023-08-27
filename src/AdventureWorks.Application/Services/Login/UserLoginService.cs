using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Repositories.AccountInfo;
using AdventureWorks.Application.Interfaces.Services.Login;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Shield;
using AdventureWorks.Domain.Models.Shield;
using AutoMapper;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Application.Services.Login;

[ServiceLifetimeScoped]
[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
public sealed class UserLoginService : IUserLoginService
{
    private readonly ILogger<UserLoginService> _logger;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IReadUserAuthorizationRepository _readUserAuthorizationRepository;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;

    public UserLoginService(
        ILogger<UserLoginService> logger,
        IUserAccountRepository userAccountRepository,
        IReadUserAuthorizationRepository readUserAuthorizationRepository,
        ITokenService tokenService,
        IMapper mapper,
        IUserRefreshTokenRepository userRefreshTokenRepository
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userAccountRepository = userAccountRepository ?? throw new ArgumentNullException(nameof(userAccountRepository));
        _readUserAuthorizationRepository = readUserAuthorizationRepository ?? throw new ArgumentNullException(nameof(readUserAuthorizationRepository));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _userRefreshTokenRepository = userRefreshTokenRepository ?? throw new ArgumentNullException(nameof(userRefreshTokenRepository));
    }

    /// <summary>
    /// Authenticate an AdventureWorks user and, if the request is valid, the generate a JWT security token
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="ipAddress"></param>
    /// <returns>a tuple that includes the user model, security token, and validation failure list </returns>
    public async Task<(UserAccountModel?, UserAccountTokenModel?, List<ValidationFailure>)> AuthenticateUserAsync(string username, string password, string ipAddress)
    {
        var validationFailures = new List<ValidationFailure>();

        var (userEntity, userEntityFailure) = await GetUserAccountEntityAsync(username);

        if (userEntity == null)
        {
            if (userEntityFailure != null)
            {
                validationFailures.Add(userEntityFailure);
            }

            return (null, null, validationFailures);
        }

        if (!BC.Verify(password, userEntity.PasswordHash))
        {
            var validationFailure = new ValidationFailure { ErrorCode = "Auth-Error-002", ErrorMessage = "Password does not match" };
            _logger.LogInformation($"{validationFailure.ErrorCode} - {validationFailure.ErrorMessage}");
            validationFailures.Add(validationFailure);

            return (null, null, validationFailures);
        }

        var (model, userModelFailure) = await GetUserAccountModelAsync(userEntity).ConfigureAwait(false);

        if (model == null)
        {
            if (userModelFailure != null)
            {
                validationFailures.Add(userModelFailure);
            }

            return (null, null, validationFailures);
        }

        var token = await _tokenService.GenerateUserTokenAsync(model, ipAddress).ConfigureAwait(false);

        return (model, token, validationFailures);
    }

    /// <summary>
    /// Generate a user's JWT security token from a refresh token
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <param name="username"></param>
    /// <param name="ipAddress"></param>
    /// <returns>a tuple that includes the user model, security token, and validation failure list </returns>
    public async Task<(UserAccountModel?, UserAccountTokenModel?, List<ValidationFailure>)> RefreshTokenAsync(string refreshToken, string username, string ipAddress)
    {
        var validationFailures = new List<ValidationFailure>();

        var (userEntity, userEntityFailure) = await GetUserAccountEntityAsync(username);

        if (userEntity == null)
        {
            if (userEntityFailure != null)
            {
                validationFailures.Add(userEntityFailure);
            }

            return (null, null, validationFailures);
        }

        var tokenList = await _userRefreshTokenRepository
            .GetRefreshTokenListByUserIdAsync(userEntity.BusinessEntityId, refreshToken).ConfigureAwait(false);

        if (tokenList == null || tokenList.Count == 0)
        {
            var validationFailure = new ValidationFailure
            {
                ErrorCode = "Auth-Error-004", 
                ErrorMessage = "Unable to create new auth token! The refresh token does not exist for the given user."
            };

            _logger.LogInformation($"{validationFailure.ErrorCode} - {validationFailure.ErrorMessage}");
            validationFailures.Add(validationFailure);

            return (null, null, validationFailures);
        }

        if (tokenList.Count > 1)
        {
            var validationFailure = new ValidationFailure
            {
                ErrorCode = "Auth-Error-005",
                ErrorMessage = "Unable to create new auth token! The system does not support duplicate refresh token entries for the same user."
            };

            _logger.LogInformation($"{validationFailure.ErrorCode} - {validationFailure.ErrorMessage}");
            validationFailures.Add(validationFailure);

            foreach (var refreshTokenEntity in tokenList)
            {
                await _userRefreshTokenRepository.RevokeRefreshTokenAsync(refreshTokenEntity).ConfigureAwait(false);
            }
            
            return (null, null, validationFailures);
        }

        var validRefreshToken = tokenList.Single();

        if (validRefreshToken.IsExpired)
        {
            var validationFailure = new ValidationFailure
            {
                ErrorCode = "Auth-Error-006",
                ErrorMessage = "Unable to create new auth token! The supplied refresh token is expired!"
            };

            _logger.LogInformation($"{validationFailure.ErrorCode} - {validationFailure.ErrorMessage}");
            validationFailures.Add(validationFailure);

            return (null, null, validationFailures);
        }

        if (validRefreshToken.IsRevoked)
        {
            var validationFailure = new ValidationFailure
            {
                ErrorCode = "Auth-Error-007",
                ErrorMessage = "Unable to create new auth token! The supplied refresh token is revoked!"
            };

            _logger.LogInformation($"{validationFailure.ErrorCode} - {validationFailure.ErrorMessage}");
            validationFailures.Add(validationFailure);

            return (null, null, validationFailures);
        }

        var (model, userModelFailure) = await GetUserAccountModelAsync(userEntity).ConfigureAwait(false);

        if (model == null)
        {
            if (userModelFailure != null)
            {
                validationFailures.Add(userModelFailure);
            }

            return (null, null, validationFailures);
        }

        var token = await _tokenService.GenerateUserTokenAsync(model, ipAddress).ConfigureAwait(false);

        return (model, token, validationFailures);
    }

    private async Task<(UserAccountEntity?, ValidationFailure?)> GetUserAccountEntityAsync(string username)
    {
        var userEntity = await _userAccountRepository.GetByUserNameAsync(username)
            .ConfigureAwait(false);

        if (userEntity != null)
        {
            return (userEntity, null);
        }

        var validationFailure = new ValidationFailure { ErrorCode = "Auth-Error-001", ErrorMessage = "Username does not exist" };
        _logger.LogInformation($"{validationFailure.ErrorCode} - {validationFailure.ErrorMessage}");
        return (null, validationFailure);
    }

    private async Task<(UserAccountModel?, ValidationFailure?)> GetUserAccountModelAsync(UserAccountEntity userEntity)
    {
        var userAuthEntity = await _readUserAuthorizationRepository.GetByUserIdAsync(userEntity.BusinessEntityId).ConfigureAwait(false);

        if (userAuthEntity == null)
        {
            var validationFailure = new ValidationFailure { ErrorCode = "Auth-Error-003", ErrorMessage = "Unable to construct authorization entity as user permission mappings do not exist" };
            _logger.LogInformation($"{validationFailure.ErrorCode} - {validationFailure.ErrorMessage}");

            return (null, validationFailure);
        }

        var model = _mapper.Map<UserAccountModel>(userEntity);

        model.SecurityRoles =
            _mapper.Map<IReadOnlyList<SecurityRoleEntity>, IReadOnlyList<SecurityRoleSlimModel>>(userAuthEntity.SecurityRoles);

        model.SecurityFunctions =
            _mapper.Map<IReadOnlyList<SecurityFunctionEntity>, IReadOnlyList<SecurityFunctionSlimModel>>
                (userAuthEntity.SecurityFunctions);

        model.SecurityGroups =
            _mapper.Map<IReadOnlyList<SecurityGroupEntity>, IReadOnlyList<SecurityGroupSlimModel>>
                (userAuthEntity.SecurityGroups);

        return (model, null);
    }

}
