namespace AdventureWorks.Domain.Models.AccountInfo;

public sealed class AuthenticationRequestModel
{
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;
}
