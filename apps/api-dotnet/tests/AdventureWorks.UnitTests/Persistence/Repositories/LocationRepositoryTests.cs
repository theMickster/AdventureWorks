using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UnitTests.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class LocationRepositoryTests : PersistenceUnitTestBase
{
    private readonly LocationRepository _sut;

    public LocationRepositoryTests()
    {
        _sut = new LocationRepository(DbContext);

        DbContext.Set<Location>().AddRange(new List<Location>
        {
            new() { LocationId = (short)1, Name = "Tool Crib", CostRate = 0.00m, Availability = 0.00m, ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
            new() { LocationId = (short)2, Name = "Sheet Metal Racks", CostRate = 0.00m, Availability = 0.00m, ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
            new() { LocationId = (short)3, Name = "Paint Shop", CostRate = 0.00m, Availability = 0.00m, ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) }
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(LocationRepository)
                .Should().Implement<ILocationRepository>();

            typeof(LocationRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task ListAllAsync_returns_correct_count()
    {
        var result = await _sut.ListAllAsync();

        using (new AssertionScope())
        {
            result.Count.Should().Be(3);
            result.Count(x => x.LocationId == 1 && x.Name == "Tool Crib").Should().Be(1);
            result.Count(x => x.LocationId == 2 && x.Name == "Sheet Metal Racks").Should().Be(1);
        }
    }

    [Fact]
    public async Task ListAllAsync_uses_no_tracking()
    {
        var result = await _sut.ListAllAsync();

        var entry = DbContext.Entry(result[0]);
        entry.State.Should().Be(EntityState.Detached);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetByIdAsync_returns_correct_entity(int id)
    {
        var result = await _sut.GetByIdAsync(id);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.LocationId.Should().Be((short)id);
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_for_nonexistent_id()
    {
        var result = await _sut.GetByIdAsync(9999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_uses_no_tracking()
    {
        var result = await _sut.GetByIdAsync(1);

        var entry = DbContext.Entry(result!);
        entry.State.Should().Be(EntityState.Detached);
    }
}
