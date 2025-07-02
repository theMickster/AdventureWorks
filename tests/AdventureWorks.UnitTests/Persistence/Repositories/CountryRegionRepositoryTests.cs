using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.Repositories;

namespace AdventureWorks.UnitTests.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class CountryRegionRepositoryTests : PersistenceUnitTestBase
{
    private readonly CountryRegionRepository _sut;

    public CountryRegionRepositoryTests()
    {
        _sut = new CountryRegionRepository(DbContext);

        DbContext.CountryRegions.AddRange(new List<CountryRegionEntity>
        {
           new(){CountryRegionCode = "USA", Name = "United States", ModifiedDate = DateTime.UtcNow}
           ,new(){CountryRegionCode = "CA", Name = "CA", ModifiedDate = DateTime.UtcNow}
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(CountryRegionRepository)
                .Should().Implement<ICountryRegionRepository>();

            typeof(CountryRegionRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task ListAllAsync_is_correctAsync()
    {
        var result = await _sut.ListAllAsync();

        using (new AssertionScope())
        {
            result.Count.Should().Be(2);
            result.Count(x => x.CountryRegionCode == "USA").Should().Be(1);
            result.Count(x => x.CountryRegionCode == "CA").Should().Be(1);
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_nullAsync()
    {
        var result = await _sut.GetByIdAsync("UK");

        result.Should().BeNull();
    }


    [Fact]
    public async Task GetByIdAsync_returns_correctlyAsync()
    {
        var result = await _sut.GetByIdAsync("USA");

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Name.Should().Be("United States");
        }
    }
}