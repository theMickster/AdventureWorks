using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UnitTests.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class SpecialOfferRepositoryTests : PersistenceUnitTestBase
{
    private readonly SpecialOfferRepository _sut;

    public SpecialOfferRepositoryTests()
    {
        _sut = new SpecialOfferRepository(DbContext);

        var today = DateTime.UtcNow.Date;

        DbContext.Set<SpecialOffer>().AddRange(new List<SpecialOffer>
        {
            new()
            {
                SpecialOfferId = 1,
                Description = "Holiday Promotion",
                DiscountPct = 0.10m,
                Type = "Discount",
                Category = "Customer",
                StartDate = today.AddDays(-7),
                EndDate = today.AddDays(7),
                MinQty = 1,
                MaxQty = 10,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc),
                SpecialOfferProducts = []
            },
            new()
            {
                SpecialOfferId = 2,
                Description = "Clearance Promotion",
                DiscountPct = 0.25m,
                Type = "Discount",
                Category = "Reseller",
                StartDate = today.AddDays(-30),
                EndDate = today.AddDays(-7),
                MinQty = 5,
                MaxQty = null,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc),
                SpecialOfferProducts = []
            },
            new()
            {
                SpecialOfferId = 3,
                Description = "Volume Promotion",
                DiscountPct = 0.15m,
                Type = "Discount",
                Category = "Customer",
                StartDate = today.AddDays(-3),
                EndDate = today.AddDays(30),
                MinQty = 10,
                MaxQty = 50,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc),
                SpecialOfferProducts = []
            }
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(SpecialOfferRepository)
                .Should().Implement<ISpecialOfferRepository>();

            typeof(SpecialOfferRepository)
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
            result.Count(x => x.SpecialOfferId == 1 && x.Description == "Holiday Promotion").Should().Be(1);
            result.Count(x => x.SpecialOfferId == 2 && x.Description == "Clearance Promotion").Should().Be(1);
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
            result!.SpecialOfferId.Should().Be(id);
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
