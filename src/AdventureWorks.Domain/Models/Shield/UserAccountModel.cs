namespace AdventureWorks.Domain.Models.Shield;

public sealed class UserAccountModel
{
    public int Id { get; set; }

    public string UserName { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public string PasswordHash { get; set; }

    public string PrimaryEmailAddress { get; set; }

    public string FullName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(MiddleName)
                ? LastName + ", " + FirstName
                : LastName + ", " + FirstName + " " + MiddleName;
        }
    }

    public IReadOnlyList<SecurityRoleSlimModel> SecurityRoles { get; set; }

    public IReadOnlyList<SecurityFunctionSlimModel> SecurityFunctions { get; set; }

    public IReadOnlyList<SecurityGroupSlimModel> SecurityGroups { get; set; }
}
