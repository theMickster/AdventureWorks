﻿using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.Login;
using AdventureWorks.Common.Attributes;
using AutoMapper;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using AdventureWorks.Application.Interfaces.Repositories.AccountInfo;
using AdventureWorks.Domain.Entities.Shield;
using AdventureWorks.Domain.Models.Shield;
namespace AdventureWorks.Application.Services.Login;

[ServiceLifetimeScoped]
public sealed class ReadUserLoginService : IReadUserLoginService
{
    private readonly ILogger<ReadUserLoginService> _logger;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IReadUserAuthorizationRepository _readUserAuthorizationRepository;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public ReadUserLoginService(
        ILogger<ReadUserLoginService> logger,
        IUserAccountRepository userAccountRepository,
        IReadUserAuthorizationRepository readUserAuthorizationRepository,
        ITokenService tokenService,
        IMapper mapper
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userAccountRepository = userAccountRepository ?? throw new ArgumentNullException(nameof(userAccountRepository));
        _readUserAuthorizationRepository = readUserAuthorizationRepository ?? throw new ArgumentNullException(nameof(readUserAuthorizationRepository));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Authenticate an AdventureWorks user and, if the request is valid, the generate a JWT security token
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns>a tuple that includes the user model, security token, and validation failure list </returns>
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
    public async Task<(UserAccountModel?, UserAccountTokenModel?, List<ValidationFailure>)> AuthenticateUserAsync(string username, string password)
    {
        var validationFailures = new List<ValidationFailure>();

        var userEntity = await _userAccountRepository.GetByUserNameAsync(username)
                .ConfigureAwait(false);

        if (userEntity == null)
        {
            var validationFailure = new ValidationFailure{ ErrorCode = "Auth-Error-001", ErrorMessage = "Username does not exist" };
            _logger.LogInformation( $"{validationFailure.ErrorCode} - {validationFailure.ErrorMessage}" );
            validationFailures.Add(validationFailure);

            return (null, null, validationFailures);
        }

        if (!BC.Verify(password, userEntity.PasswordHash))
        {
            var validationFailure = new ValidationFailure { ErrorCode = "Auth-Error-002", ErrorMessage = "Password does not match" };
            _logger.LogInformation($"{validationFailure.ErrorCode} - {validationFailure.ErrorMessage}");
            validationFailures.Add(validationFailure);

            return (null, null, validationFailures);
        }

        var userAuthEntity = await _readUserAuthorizationRepository.GetByUserIdAsync(userEntity.BusinessEntityId).ConfigureAwait(false);

        if (userAuthEntity == null)
        {
            var validationFailure = new ValidationFailure { ErrorCode = "Auth-Error-003", ErrorMessage = "Unable to construct authorization entity as user permission mappings do not exist" };
            _logger.LogInformation($"{validationFailure.ErrorCode} - {validationFailure.ErrorMessage}");
            validationFailures.Add(validationFailure);

            return (null, null, validationFailures);
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

        var token = _tokenService.GenerateUserToken(model);

        return (model, token, validationFailures);
    }

}
