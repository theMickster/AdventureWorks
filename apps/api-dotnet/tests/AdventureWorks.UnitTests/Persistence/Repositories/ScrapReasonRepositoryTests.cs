using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UnitTests.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class ScrapReasonRepositoryTests : PersistenceUnitTestBase
{
    private readonly ScrapReasonRepository _sut;

    public ScrapReasonRepositoryTests()
    {
        _sut = new ScrapReasonRepository(DbContext);

        DbContext.Set<ScrapReason>().AddRange(new List<ScrapReason>
        {
            new() { ScrapReasonId = (short)1, Name = "Brake assembly not as ordered", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
            new() { ScrapReasonId = (short)2, Name = "Color incorrect", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
            new() { ScrapReasonId = (short)3, Name = "Drill size too large", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) }
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ScrapReasonRepository)
                .Should().Implement<IScrapReasonRepository>();

            typeof(ScrapReasonRepository)
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
            result.Count(x => x.ScrapReasonId == 1 && x.Name == "Brake assembly not as ordered").Should().Be(1);
            result.Count(x => x.ScrapReasonId == 2 && x.Name == "Color incorrect").Should().Be(1);
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
            result!.ScrapReasonId.Should().Be((short)id);
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
