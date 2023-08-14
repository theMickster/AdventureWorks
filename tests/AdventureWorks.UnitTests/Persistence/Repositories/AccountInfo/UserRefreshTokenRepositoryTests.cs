using AdventureWorks.Application.Interfaces.Repositories.AccountInfo;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Shield;
using AdventureWorks.Infrastructure.Persistence.Repositories.AccountInfo;

namespace AdventureWorks.UnitTests.Persistence.Repositories.AccountInfo;

[ExcludeFromCodeCoverage]
public sealed class UserRefreshTokenRepositoryTests : PersistenceUnitTestBase
{
    private readonly UserRefreshTokenRepository _sut;

    public UserRefreshTokenRepositoryTests()
    {
        _sut = new UserRefreshTokenRepository(DbContext);

        DbContext.UserRefreshTokens.AddRange(new List<UserRefreshTokenEntity>
        {
            new()
            {
                UserRefreshTokenId = 1, BusinessEntityId = 1, IpAddress = "192.168.100.169", 
                RefreshToken = "7883FE9B029641C28BC119672547F8371F235969226C4F4EA128C6FE8F073D46",
                CreatedBy = 1, CreatedOn = StandardCreatedDate, ModifiedBy = 1, ModifiedOn = StandardModifiedDate,
                RecordId = new Guid("14fff880-0315-4af6-9cc4-a0530298c4cf"),
                ExpiresOn = DateTime.UtcNow.AddDays(-5), IsExpired = true
            },
            new()
            {
                UserRefreshTokenId = 2, BusinessEntityId = 1, IpAddress = "192.168.100.185",
                RefreshToken = "F63F83B76CD1430DB1C3DFBF19A66275454D8356210D4B2793526DE911710A7E",
                CreatedBy = 1, CreatedOn = StandardCreatedDate, ModifiedBy = 1, ModifiedOn = StandardModifiedDate,
                RecordId = new Guid("ebd0e1c5-5027-4c85-9719-c5c70366eac0"), 
                ExpiresOn = DateTime.UtcNow.AddDays(5), IsExpired = false
            },
            new()
            {
                UserRefreshTokenId = 3, BusinessEntityId = 2, IpAddress = "192.168.100.185",
                RefreshToken = "D4EFB99864D65B7CB19F43D9C7662DCC658412C0744B389412182C73CF488",
                CreatedBy = 1, CreatedOn = StandardCreatedDate, ModifiedBy = 1, ModifiedOn = StandardModifiedDate,
                RecordId = new Guid("57a468b2-c0ed-471a-af72-dd21ae8b1d9e"),
                ExpiresOn = DateTime.UtcNow.AddDays(2), IsExpired = false
            },
        });
        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(UserRefreshTokenRepository)
                .Should().Implement<IUserRefreshTokenRepository>();

            typeof(UserRefreshTokenRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Theory]
    [InlineData(1, "7883FE9B029641C28BC119672547F8371F235969226C4F4EA128C6FE8F073D46", 1)]
    [InlineData(1, "7883FE9B029641C28BCAYGADLFGKJASGHDFAHBS749528435A128C6FE8F073D46", 0)]
    [InlineData(2, "D4EFB99864D65B7CB19F43D9C7662DCC658412C0744B389412182C73CF488", 1)]
    public async Task GetRefreshTokenListByUserIdAsync_returns_correct_listAsync(int userId, string userToken,
        int recordCount)
    {
        var results = await _sut.GetRefreshTokenListByUserIdAsync(userId, userToken).ConfigureAwait(false);
        results.Should().HaveCount(recordCount);
    }

    [Theory]
    [InlineData(1, "F63F83B76CD1430DB1C3DFBF19A66275454D8356210D4B2793526DE911710A7E", true)]
    [InlineData(2, "D4EFB99864D65B7CB19F43D9C7662DCC658412C0744B389412182C73CF488", true)]
    [InlineData(3, "", false)]
    public async Task GetMostRecentRefreshTokenByUserIdAsync_returns_correct_tokenAsync(int userId, string userToken, bool shouldExist)
    {
        var result = await _sut.GetMostRecentRefreshTokenByUserIdAsync(userId).ConfigureAwait(false);

        using (new AssertionScope())
        {
            if (shouldExist)
            {
                result?.Should().NotBeNull();
                result!.RefreshToken.Should().Be(userToken);
            }
            else
            {
                result?.Should().BeNull();
            }
        }
    }
}
