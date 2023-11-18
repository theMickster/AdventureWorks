using AdventureWorks.Application.Interfaces.Repositories.AccountInfo;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Infrastructure.Persistence.Repositories.AccountInfo;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.Persistence.Repositories.AccountInfo;

[ExcludeFromCodeCoverage]
public sealed class ReadUserAuthorizationRepositoryTests : PersistenceUnitTestBase
{
    private readonly Mock<ILogger<ReadUserAuthorizationRepository>> _mockLogger = new();
    private ReadUserAuthorizationRepository _sut;

    public ReadUserAuthorizationRepositoryTests()
    {
        _sut = new ReadUserAuthorizationRepository(DbContext, _mockLogger.Object);
        LoadMockUserSecurityData();
        LoadMockPeople();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ReadUserAuthorizationRepository)
                .Should().Implement<IReadUserAuthorizationRepository>();

            typeof(ReadUserAuthorizationRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_returns_null_when_user_is_not_foundAsync()
    {
        const int userId = -10;

         var result = await _sut.GetByUserIdAsync(userId);
         result?.Should().BeNull();
    }

    [Theory]
    [InlineData(1, 1, 1, 3)]
    [InlineData(2, 1, 1, 2)]
    [InlineData(3, 1, 1, 2)]
    public async Task GetByUserIdAsync_succeedsAsync(int userId, int functionsCount, int groupsCount, int rolesCount)
    {
        var result = await _sut.GetByUserIdAsync(userId);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.BusinessEntityId.Should().Be(userId);

            result.SecurityFunctions.Count.Should().Be(functionsCount);
            result.SecurityGroups.Count.Should().Be(groupsCount);
            result.SecurityRoles.Count.Should().Be(rolesCount);
        }
    }
}
