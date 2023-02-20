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
}
