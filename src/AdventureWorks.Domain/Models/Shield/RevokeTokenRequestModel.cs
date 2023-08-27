namespace AdventureWorks.Domain.Models.Shield;

public sealed class RevokeTokenRequestModel
{
    public string RefreshToken { get; set; } = null!;
}
