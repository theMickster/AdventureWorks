using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UnitTests.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class CurrencyRepositoryTests : PersistenceUnitTestBase
{
    private readonly CurrencyRepository _sut;

    public CurrencyRepositoryTests()
    {
        _sut = new CurrencyRepository(DbContext);

        DbContext.Set<Currency>().AddRange(new List<Currency>
        {
            new() { CurrencyCode = "USD", Name = "US Dollar", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
            new() { CurrencyCode = "EUR", Name = "Euro", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
            new() { CurrencyCode = "GBP", Name = "Pound Sterling", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) }
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(CurrencyRepository)
                .Should().Implement<ICurrencyRepository>();

            typeof(CurrencyRepository)
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
            result.Count(x => x.CurrencyCode == "USD" && x.Name == "US Dollar").Should().Be(1);
            result.Count(x => x.CurrencyCode == "EUR" && x.Name == "Euro").Should().Be(1);
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
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    public async Task GetByIdAsync_is_correctAsync(string code)
    {
        var result = await _sut.GetByIdAsync(code);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.CurrencyCode.Should().Be(code);
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_for_nonexistent_code()
    {
        var result = await _sut.GetByIdAsync("ZZZ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_uses_no_tracking()
    {
        var result = await _sut.GetByIdAsync("USD");

        var entry = DbContext.Entry(result!);
        entry.State.Should().Be(EntityState.Detached);
    }
}
