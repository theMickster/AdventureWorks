using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Shield;
using AdventureWorks.Infrastructure.Persistence.Repositories.AccountInfo;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.Persistence.Repositories.AccountInfo;

[ExcludeFromCodeCoverage]   
public sealed class UserAccountRepositoryTests : PersistenceUnitTestBase
{
    private readonly Mock<ILogger<UserAccountRepository>> _mockLogger = new();
    private readonly UserAccountRepository _sut;

    public UserAccountRepositoryTests()
    {
        _sut = new UserAccountRepository(DbContext, _mockLogger.Object);

        DbContext.Persons.AddRange(new List<Person>
        {
            new()
            {
                BusinessEntityId = 1, FirstName = "John", LastName = "Elway",
                Rowguid = new Guid("7a80b0a7-1122-49e3-875b-95cc9fcae017")
            },
            new()
            {
                BusinessEntityId = 2, FirstName = "Terrell", LastName = "Davis",
                Rowguid = new Guid("87c347f0-e6d1-46bc-b510-ff2f9be50d82")
            },
            new()
            {
                BusinessEntityId = 3, FirstName = "Shannon", LastName = "Sharpe",
                Rowguid = new Guid("94159810-21c3-4666-ba28-04911f05215e")
            },
            new()
            {
                BusinessEntityId = 4, FirstName = "Emmitt", LastName = "Smith",
                Rowguid = new Guid("1bfe2f92-cf14-4258-a634-14ed56dbad69")
            },
            new()
            {
                BusinessEntityId = 5, FirstName = "Duplicate", LastName = "User",
                Rowguid = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c")
            }
        });

        DbContext.UserAccounts.AddRange(new List<UserAccountEntity>
        {
            new()
            {
                BusinessEntityId = 1, 
                UserName = "john.elway",
                RecordId = new Guid("7a80b0a7-1122-49e3-875b-95cc9fcae017"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK"
            },
            new()
            {
                BusinessEntityId = 2, 
                UserName = "terrell.davis",
                RecordId = new Guid("87c347f0-e6d1-46bc-b510-ff2f9be50d82"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK"
            },
            new()
            {
                BusinessEntityId = 3, 
                UserName = "shannon.sharpe",
                RecordId = new Guid("94159810-21c3-4666-ba28-04911f05215e"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK"
            },
            new()
            {
                BusinessEntityId = 5,
                UserName = "Duplicate.User",
                RecordId = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK"
            },
            new()
            {
                BusinessEntityId = 4,
                UserName = "emmitt.smith",
                RecordId = new Guid("1bfe2f92-cf14-4258-a634-14ed56dbad69"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK"
            },
            new()
            {
            BusinessEntityId = 6,
            UserName = "Duplicate.User",
            RecordId = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"),
            PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK"
        },
        });

        DbContext.BusinessEntities.AddRange(new List<BusinessEntity>
        {
            new() { BusinessEntityId = 1, Rowguid = new Guid("7a80b0a7-1122-49e3-875b-95cc9fcae017") },
            new() { BusinessEntityId = 2, Rowguid = new Guid("87c347f0-e6d1-46bc-b510-ff2f9be50d82") },
            new() { BusinessEntityId = 3, Rowguid = new Guid("94159810-21c3-4666-ba28-04911f05215e") },
            new() { BusinessEntityId = 4, Rowguid = new Guid("1bfe2f92-cf14-4258-a634-14ed56dbad69") },
            new() { BusinessEntityId = 5, Rowguid = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c") },
            new() { BusinessEntityId = 6, Rowguid = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c") }
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(UserAccountRepository)
                .Should().Implement<IUserAccountRepository>();

            typeof(UserAccountRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task ListAllAsync_returns_complete_entitiesAsync()
    {

        var orderedAccounts = (await _sut.ListAllAsync().ConfigureAwait(false)).OrderBy(x => x.BusinessEntityId).ToList();

        using (new AssertionScope())
        {
            orderedAccounts!.Should().NotBeNullOrEmpty();
            orderedAccounts!.Count.Should().Be(6);

            orderedAccounts[0].Person!.FirstName.Should().Be("John");
            orderedAccounts[1].Person!.LastName.Should().Be("Davis");
            orderedAccounts[2].Person!.FirstName.Should().Be("Shannon");
        }
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    [InlineData(5, true)]
    [InlineData(25, false)]
    public async Task GetByIdAsync_returns_correctlyAsync(int userId, bool shouldExist)
    {
        var entity = await _sut.GetByIdAsync(userId).ConfigureAwait(false);

        using (new AssertionScope())
        {
            if (shouldExist)
            {
                entity.Should().NotBeNull();
                entity.BusinessEntityId.Should().Be(userId);
            }
            else
            {
                entity?.Should().BeNull();
            }
        }
    }

    [Theory]
    [InlineData(-1, "tickle-me-elmo", false)]
    [InlineData(2, "terrell.davis", true)]
    [InlineData(4, "emmitt.smith", true)]
    [InlineData(25, "cookie-monster", false)]
    public async Task GetByUserNameAsync_correctlyAsync(int userId, string username, bool shouldExist)
    {
        var entity = await _sut.GetByUserNameAsync(username).ConfigureAwait(false);

        using (new AssertionScope())
        {
            if (shouldExist)
            {
                entity.Should().NotBeNull();
                entity.BusinessEntityId.Should().Be(userId);
                entity.UserName.Should().Be(username);
            }
            else
            {
                entity?.Should().BeNull();
            }
        }
    }
}
