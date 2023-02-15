namespace AdventureWorks.Domain.Models.Shield;

public sealed class AuthenticationResponseModel
{
    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public UserAccountTokenModel Token { get; set; }

}
