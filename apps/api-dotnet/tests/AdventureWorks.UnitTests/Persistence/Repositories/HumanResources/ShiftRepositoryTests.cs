using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Infrastructure.Persistence.Repositories.HumanResources;

namespace AdventureWorks.UnitTests.Persistence.Repositories.HumanResources;

[ExcludeFromCodeCoverage]
public sealed class ShiftRepositoryTests : PersistenceUnitTestBase
{
    private readonly ShiftRepository _sut;

    public ShiftRepositoryTests()
    {
        _sut = new ShiftRepository(DbContext);

        DbContext.Shifts.AddRange(new List<ShiftEntity>
        {
            new() { ShiftId = 1, Name = "Day", StartTime = new TimeSpan(7, 0, 0), EndTime = new TimeSpan(15, 0, 0), ModifiedDate = DateTime.UtcNow }
            ,new() { ShiftId = 2, Name = "Evening", StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(23, 0, 0), ModifiedDate = DateTime.UtcNow }
            ,new() { ShiftId = 3, Name = "Night", StartTime = new TimeSpan(23, 0, 0), EndTime = new TimeSpan(7, 0, 0), ModifiedDate = DateTime.UtcNow }
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ShiftRepository)
                .Should().Implement<IShiftRepository>();

            typeof(ShiftRepository)
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
            result.Count(x => x.ShiftId == 3 && x.Name == "Night").Should().Be(1);
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
            result!.ShiftId.Should().Be((byte)id);
        }
    }
}
