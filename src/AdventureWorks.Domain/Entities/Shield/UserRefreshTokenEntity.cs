using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Domain.Entities.Shield;

public sealed class UserRefreshTokenEntity : BaseAuditableEntity
{
    public int UserRefreshTokenId { get; set; }

    public Guid RecordId { get; set; }

    public int BusinessEntityId { get; set; }

    public string IpAddress { get; set; }

    public string RefreshToken { get; set; }

    public DateTime ExpiresOn { get; set; }

    public bool IsExpired { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? RevokedOn { get; set; }

    public BusinessEntity BusinessEntity { get; set; }

    public UserAccountEntity UserAccountEntity { get; set; }
}
