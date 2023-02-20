namespace AdventureWorks.Domain.Entities.Shield;

public sealed class UserAuthorizationEntity
{
    public UserAuthorizationEntity()
    {
        
    }

    public UserAuthorizationEntity(
        IReadOnlyList<SecurityGroupEntity> securityGroups,
        IReadOnlyList<SecurityRoleEntity> securityRoles,
        IReadOnlyList<SecurityFunctionEntity> securityFunctions,
        int userId)
    {
        SecurityGroups = securityGroups ?? throw new ArgumentNullException(nameof(securityGroups));
        SecurityRoles = securityRoles ?? throw new ArgumentNullException(nameof(securityRoles));
        SecurityFunctions = securityFunctions ?? throw new ArgumentNullException(nameof(securityFunctions));
        BusinessEntityId = userId;
    }

    public int BusinessEntityId { get; init; }

    public IReadOnlyList<SecurityRoleEntity> SecurityRoles { get; init; }

    public IReadOnlyList<SecurityGroupEntity> SecurityGroups { get; init; }

    public IReadOnlyList<SecurityFunctionEntity> SecurityFunctions { get; init; }

}
