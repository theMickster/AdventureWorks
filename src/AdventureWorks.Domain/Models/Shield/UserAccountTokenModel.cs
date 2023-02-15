namespace AdventureWorks.Domain.Models.Shield;

public sealed class UserAccountTokenModel
{
    public Guid Id { get; set; }

    public string Token { get; set; }

    public DateTime TokenExpiration { get; set; }

    public string RefreshToken { get; set; }

    public DateTime RefreshTokenExpiration { get; set; }

}
