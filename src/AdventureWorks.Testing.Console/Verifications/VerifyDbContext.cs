using AdventureWorks.Application.Interfaces.DbContext;
using AdventureWorks.Application.Interfaces.Repositories.AccountInfo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorks.Testing.Console.Verifications;

internal sealed class VerifyDbContext
{
    private readonly IAdventureWorksDbContext _dbContext;
    private readonly IReadUserAuthorizationRepository _readUserAuthorizationRepository;

    public VerifyDbContext(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        _dbContext = serviceProvider.GetRequiredService<IAdventureWorksDbContext>() ??
                     throw new InvalidOperationException(
                         "Unable to find a concrete implementation of IAdventureWorksDbContext ");

        _readUserAuthorizationRepository = serviceProvider.GetRequiredService<IReadUserAuthorizationRepository>() ??
                                           throw new InvalidOperationException(
                                               "Unable to find a concrete implementation of IReadUserAuthorizationRepository ");
    }

    public async Task<(bool status, List<string> errorsList)> VerifyAllTheThings()
    {
        var success = true;
        var errorList = new List<string>();

        var functions = _dbContext.SecurityFunctions.ToList();
        if (functions.Any())
        {
            success = false;

        }

        var roles = _dbContext.SecurityRoles.ToList();
        if (!roles.Any())
        {
            success = false;
        }

        var groups = _dbContext.SecurityGroups.ToList();
        if (!groups.Any())
        {
            success = false;
        }

        //var userName = "mick.letofsky";
        //var userName = "michael.scott";
        //var userName = "pam.beesly";
        //var userName = "jim.halpert";


        //var userGroupMapping = 
        //    _dbContext.SecurityGroupUserAccounts
        //        .Include(x => x.SecurityGroup)
        //        .Include(x => x.UserAccount)
        //        .Include(x => x.BusinessEntity)
        //        .Where(x => x.UserAccount.UserName == userName)
        //        .ToList();

        //var securityGroups = userGroupMapping.Select(x => x.SecurityGroup).ToList();

        //var securityGroupIds = userGroupMapping.Select(x => x.GroupId).ToList();

        //var securityRoles = 
        //    _dbContext.SecurityGroupSecurityRoles
        //        .Include(x => x.SecurityRole)
        //        .Where( x => securityGroupIds.Contains(x.GroupId))
        //        .Select(z => z.SecurityRole)
        //        .ToList();

        //var securityFunctions =
        //    _dbContext.SecurityGroupSecurityFunctions
        //        .Include(x => x.SecurityFunction)
        //        .Where(x => securityGroupIds.Contains(x.GroupId))
        //        .Select(z => z.SecurityFunction)
        //        .ToList();

        //if (!userGroupMapping.Any())
        //{
        //    success = false;
        //}

        //if (!securityGroups.Any())
        //{
        //    success = false;
        //}

        //if (!securityRoles.Any())
        //{
        //    success = false;
        //}
        
        //if (securityFunctions.Any())
        //{
        //    success = false;
        //}


        const int userId = 1000001;

        var result = await _readUserAuthorizationRepository.GetByUserIdAsync(userId).ConfigureAwait(false);

        if (result is null)
        {
            success = false;
        }

        return (success, errorList);
    }
}
