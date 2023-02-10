﻿using AdventureWorks.Domain.Models;

namespace AdventureWorks.Application.Interfaces.Services.Login;

public interface ITokenService
{
    /// <summary>
    /// Creates a JWT security token for the given <see cref="UserAccountModel"/>
    /// </summary>
    /// <param name="userAccount">the AdventureWorks user requesting the token</param>
    /// <returns>a valid AdventureWorks API token</returns>
    string GenerateUserToken(UserAccountModel userAccount);
}