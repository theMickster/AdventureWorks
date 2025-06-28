using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.Repositories.HumanResources;

namespace AdventureWorks.UnitTests.Persistence.Repositories.HumanResources;

[ExcludeFromCodeCoverage]
public sealed class DepartmentRepositoryTests : PersistenceUnitTestBase
{
    private readonly DepartmentRepository _sut;

    public DepartmentRepositoryTests()
    {
        _sut = new DepartmentRepository(DbContext);

        DbContext.Departments.AddRange(new List<DepartmentEntity>
        {
            new() { DepartmentId = 1, Name = "Engineering", GroupName = "Research and Development", ModifiedDate = DateTime.UtcNow }
            ,new() { DepartmentId = 2, Name = "Tool Design", GroupName = "Research and Development", ModifiedDate = DateTime.UtcNow }
            ,new() { DepartmentId = 3, Name = "Sales", GroupName = "Sales and Marketing", ModifiedDate = DateTime.UtcNow }
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(DepartmentRepository)
                .Should().Implement<IDepartmentRepository>();

            typeof(DepartmentRepository)
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
            result.Count(x => x.DepartmentId == 3 && x.Name == "Sales").Should().Be(1);
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
            result!.DepartmentId.Should().Be((short)id);
        }
    }
}
