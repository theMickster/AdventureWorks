using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Domain.Entities.HumanResources;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadDepartmentHeadcountSummaryQueryHandlerTests : UnitTestBase
{
    private readonly Mock<IDepartmentRepository> _mockRepository = new();
    private ReadDepartmentHeadcountSummaryQueryHandler _sut;

    public ReadDepartmentHeadcountSummaryQueryHandlerTests()
    {
        _sut = new ReadDepartmentHeadcountSummaryQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_when_repository_is_null()
    {
        ((Action)(() => _sut = new ReadDepartmentHeadcountSummaryQueryHandler(null!)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("departmentRepository");
    }

    [Fact]
    public async Task Null_request_throws_ArgumentNullExceptionAsync()
    {
        await ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Returns_all_departments_including_zero_count_departmentsAsync()
    {
        var repoResult = new List<(DepartmentEntity Dept, int Count)>
        {
            (new DepartmentEntity { DepartmentId = 1, Name = "Engineering", GroupName = "R&D", ModifiedDate = DefaultAuditDate }, 10),
            (new DepartmentEntity { DepartmentId = 2, Name = "Marketing", GroupName = "Sales and Marketing", ModifiedDate = DefaultAuditDate }, 0),
            (new DepartmentEntity { DepartmentId = 3, Name = "Sales", GroupName = "Sales and Marketing", ModifiedDate = DefaultAuditDate }, 5),
        }.AsReadOnly();

        _mockRepository.Setup(x => x.GetDepartmentHeadcountSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoResult);

        var result = await _sut.Handle(new ReadDepartmentHeadcountSummaryQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().HaveCount(3);
            result.Should().Contain(x => x.DepartmentName == "Marketing" && x.ActiveEmployeeCount == 0);
        }
    }

    [Fact]
    public async Task Handler_preserves_repository_ordering_descending_by_countAsync()
    {
        // Ordering is the repository's responsibility; the handler maps in whatever order is returned.
        var repoResult = new List<(DepartmentEntity Dept, int Count)>
        {
            (new DepartmentEntity { DepartmentId = 3, Name = "Sales", GroupName = "Sales and Marketing", ModifiedDate = DefaultAuditDate }, 25),
            (new DepartmentEntity { DepartmentId = 1, Name = "Engineering", GroupName = "R&D", ModifiedDate = DefaultAuditDate }, 10),
            (new DepartmentEntity { DepartmentId = 2, Name = "Marketing", GroupName = "Sales and Marketing", ModifiedDate = DefaultAuditDate }, 0),
        }.AsReadOnly();

        _mockRepository.Setup(x => x.GetDepartmentHeadcountSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoResult);

        var result = await _sut.Handle(new ReadDepartmentHeadcountSummaryQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result[0].ActiveEmployeeCount.Should().Be(25);
            result[1].ActiveEmployeeCount.Should().Be(10);
            result[2].ActiveEmployeeCount.Should().Be(0);
        }
    }
}
