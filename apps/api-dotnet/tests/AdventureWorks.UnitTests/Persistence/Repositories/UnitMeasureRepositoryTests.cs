using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UnitTests.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class UnitMeasureRepositoryTests : PersistenceUnitTestBase
{
    private readonly UnitMeasureRepository _sut;

    public UnitMeasureRepositoryTests()
    {
        _sut = new UnitMeasureRepository(DbContext);

        DbContext.Set<UnitMeasure>().AddRange(new List<UnitMeasure>
        {
            new() { UnitMeasureCode = "EA", Name = "Each", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
            new() { UnitMeasureCode = "LB", Name = "Pound", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
            new() { UnitMeasureCode = "OZ", Name = "Ounce", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) }
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(UnitMeasureRepository)
                .Should().Implement<IUnitMeasureRepository>();

            typeof(UnitMeasureRepository)
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
            result.Count(x => x.UnitMeasureCode == "EA" && x.Name == "Each").Should().Be(1);
            result.Count(x => x.UnitMeasureCode == "LB" && x.Name == "Pound").Should().Be(1);
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
    [InlineData("EA")]
    [InlineData("LB")]
    public async Task GetByCodeAsync_returns_correct_entity(string code)
    {
        var result = await _sut.GetByCodeAsync(code);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.UnitMeasureCode.Should().Be(code);
        }
    }

    [Fact]
    public async Task GetByCodeAsync_returns_null_for_nonexistent_code()
    {
        var result = await _sut.GetByCodeAsync("ZZZ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_uses_no_tracking()
    {
        var result = await _sut.GetByCodeAsync("EA");

        var entry = DbContext.Entry(result!);
        entry.State.Should().Be(EntityState.Detached);
    }
}
