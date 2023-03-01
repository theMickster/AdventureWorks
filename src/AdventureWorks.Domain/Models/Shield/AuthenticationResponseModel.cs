namespace AdventureWorks.Domain.Models.Shield;

public sealed class AuthenticationResponseModel
{
    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string EmailAddress { get; set; } = string.Empty;

    public UserAccountTokenModel Token { get; set; }

    public IReadOnlyList<SecurityFunctionSlimModel> SecurityFunctions { get; set; }

    public IReadOnlyList<SecurityGroupSlimModel> SecurityGroups { get; set; }

    public IReadOnlyList<SecurityRoleSlimModel> SecurityRoles { get; set; }
}
