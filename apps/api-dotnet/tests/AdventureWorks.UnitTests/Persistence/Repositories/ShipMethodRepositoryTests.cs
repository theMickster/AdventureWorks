using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UnitTests.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class ShipMethodRepositoryTests : PersistenceUnitTestBase
{
    private readonly ShipMethodRepository _sut;

    public ShipMethodRepositoryTests()
    {
        _sut = new ShipMethodRepository(DbContext);

        DbContext.Set<ShipMethod>().AddRange(new List<ShipMethod>
        {
            new() { ShipMethodId = 1, Name = "CARGO TRANSPORT 5", ShipBase = 3.95m, ShipRate = 1.25m, Rowguid = Guid.NewGuid(), ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
            new() { ShipMethodId = 2, Name = "ZY - EXPRESS", ShipBase = 2.95m, ShipRate = 1.50m, Rowguid = Guid.NewGuid(), ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
            new() { ShipMethodId = 3, Name = "OVERNIGHT J-FAST", ShipBase = 4.95m, ShipRate = 1.75m, Rowguid = Guid.NewGuid(), ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) }
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ShipMethodRepository)
                .Should().Implement<IShipMethodRepository>();

            typeof(ShipMethodRepository)
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
            result.Count(x => x.ShipMethodId == 1 && x.Name == "CARGO TRANSPORT 5").Should().Be(1);
            result.Count(x => x.ShipMethodId == 2 && x.Name == "ZY - EXPRESS").Should().Be(1);
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
    [InlineData(3)]
    public async Task GetByIdAsync_is_correctAsync(int id)
    {
        var result = await _sut.GetByIdAsync(id);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ShipMethodId.Should().Be(id);
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_for_nonexistent_id()
    {
        var result = await _sut.GetByIdAsync(999);

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
