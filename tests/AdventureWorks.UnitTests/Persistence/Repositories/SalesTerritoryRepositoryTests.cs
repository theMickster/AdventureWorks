using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.Repositories;

namespace AdventureWorks.UnitTests.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class SalesTerritoryRepositoryTests : PersistenceUnitTestBase
{
    private readonly SalesTerritoryRepository _sut;

    public SalesTerritoryRepositoryTests()
    {
        _sut = new SalesTerritoryRepository(DbContext);

        DbContext.CountryRegions.AddRange(new List<CountryRegionEntity>
        {
            new(){CountryRegionCode = "US", Name = "United States"}
            ,new(){CountryRegionCode = "DE", Name = "Germany"}
            ,new(){CountryRegionCode = "MX", Name = "Mexico"}
        });

        DbContext.SalesTerritories.AddRange(new List<SalesTerritoryEntity>
        {
            new(){ TerritoryId = 1, Name = "Northwest", CountryRegionCode = "US" }
            ,new(){ TerritoryId = 2, Name = "Northeast", CountryRegionCode = "US" }
            ,new(){ TerritoryId = 3, Name = "Mexico", CountryRegionCode = "MX" }
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(SalesTerritoryRepository)
                .Should().Implement<ISalesTerritoryRepository>();

            typeof(SalesTerritoryRepository)
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
            result.Count.Should().Be(3);
            result.Count(x => x.TerritoryId == 3 && x.Name == "Mexico").Should().Be(1);

            result[0].CountryRegion.Should().NotBeNull();
            result[1].CountryRegion.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetByIdAsync_is_correctAsync(int id)
    {
        var result = await _sut.GetByIdAsync(id);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.TerritoryId.Should().Be(id);
            result!.CountryRegion.Should().NotBeNull();
        }
    }
}
