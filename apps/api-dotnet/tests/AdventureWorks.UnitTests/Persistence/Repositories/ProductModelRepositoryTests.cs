using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UnitTests.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class ProductModelRepositoryTests : PersistenceUnitTestBase
{
    private readonly ProductModelRepository _sut;

    public ProductModelRepositoryTests()
    {
        _sut = new ProductModelRepository(DbContext);

        DbContext.Set<ProductModel>().AddRange(new List<ProductModel>
        {
            new() { ProductModelId = 1, Name = "Classic Vest", CatalogDescription = "<catalog>vest</catalog>", Rowguid = Guid.NewGuid(), ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
            new() { ProductModelId = 2, Name = "Long-Sleeve Logo Jersey", CatalogDescription = null!, Rowguid = Guid.NewGuid(), ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
            new() { ProductModelId = 3, Name = "Mountain Bike Socks", CatalogDescription = null!, Rowguid = Guid.NewGuid(), ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) }
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ProductModelRepository)
                .Should().Implement<IProductModelRepository>();

            typeof(ProductModelRepository)
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
            result.Count(x => x.ProductModelId == 1 && x.Name == "Classic Vest").Should().Be(1);
            result.Count(x => x.ProductModelId == 2 && x.Name == "Long-Sleeve Logo Jersey").Should().Be(1);
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
            result!.ProductModelId.Should().Be(id);
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
