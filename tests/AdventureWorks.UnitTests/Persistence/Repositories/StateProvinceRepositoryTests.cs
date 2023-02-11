using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.Repositories;

namespace AdventureWorks.UnitTests.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class StateProvinceRepositoryTests : PersistenceUnitTestBase
{
    private readonly StateProvinceRepository _sut;

    public StateProvinceRepositoryTests()
    {
        _sut = new StateProvinceRepository(DbContext);

        DbContext.StateProvinces.AddRange(new List<StateProvinceEntity>
        {
            new() { StateProvinceId = 10, StateProvinceCode = "CO", Name = "Colorado", CountryRegionCode = "US", TerritoryId = 1 }
            ,new() { StateProvinceId = 58, StateProvinceCode = "OR", Name = "Oregon", CountryRegionCode = "US", TerritoryId = 1 }
            ,new() { StateProvinceId = 79, StateProvinceCode = "WA", Name = "Washington", CountryRegionCode = "US", TerritoryId = 1 }
            ,new() { StateProvinceId = 85, StateProvinceCode = "BB", Name = "Brandenburg", CountryRegionCode = "DE", TerritoryId = 8 }
        });

        DbContext.CountryRegions.AddRange( new List<CountryRegionEntity>
        {
            new(){CountryRegionCode = "US", Name = "United States"}
            ,new(){CountryRegionCode = "DE", Name = "Germany"}
        });

        DbContext.SalesTerritories.AddRange(new List<SalesTerritoryEntity>
        {
            new(){TerritoryId = 1, Name = "Northwest"}
            ,new(){TerritoryId = 8, Name = "Germany"}
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(StateProvinceRepository)
                .Should().Implement<IStateProvinceRepository>();

            typeof(StateProvinceRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task ListAllAsync_is_correctAsync()
    {
        var result = await _sut.ListAllAsync().ConfigureAwait(false);

        using (new AssertionScope())
        {
            result.Count.Should().Be(4);
            result.Count(x => x.StateProvinceId == 10 && x.StateProvinceCode == "CO").Should().Be(1);
            result.Count(x => x.StateProvinceId == 58 && x.StateProvinceCode == "OR").Should().Be(1);
            result.Count(x => x.StateProvinceId == 79 && x.StateProvinceCode == "WA").Should().Be(1);

            result[0].SalesTerritory.Should().NotBeNull();
            result[0].CountryRegion.Should().NotBeNull();
            result[1].SalesTerritory.Should().NotBeNull();
            result[1].CountryRegion.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData(10)]
    [InlineData(58)]
    [InlineData(79)]
    public async Task GetByIdAsync_is_correctAsync(int id)
    {
        var result = await _sut.GetByIdAsync(id).ConfigureAwait(false);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.StateProvinceId.Should().Be(id);

            result!.SalesTerritory.Should().NotBeNull();
            result!.CountryRegion.Should().NotBeNull();
        }
    }
}