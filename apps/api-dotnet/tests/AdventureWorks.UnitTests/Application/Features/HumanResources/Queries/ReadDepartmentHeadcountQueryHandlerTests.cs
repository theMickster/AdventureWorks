using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Domain.Entities.HumanResources;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadDepartmentHeadcountQueryHandlerTests : UnitTestBase
{
    private readonly Mock<IDepartmentRepository> _mockRepository = new();
    private ReadDepartmentHeadcountQueryHandler _sut;

    public ReadDepartmentHeadcountQueryHandlerTests()
    {
        _sut = new ReadDepartmentHeadcountQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_when_repository_is_null()
    {
        ((Action)(() => _sut = new ReadDepartmentHeadcountQueryHandler(null!)))
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
    public async Task Department_not_found_throws_KeyNotFoundExceptionAsync()
    {
        const int departmentId = 999;

        _mockRepository.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DepartmentEntity?)null);

        Func<Task> act = async () => await _sut.Handle(
            new ReadDepartmentHeadcountQuery { DepartmentId = departmentId },
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Department with ID {departmentId} not found.");
    }

    [Fact]
    public async Task Returns_zero_count_when_no_active_employees_existAsync()
    {
        const int departmentId = 1;
        var dept = new DepartmentEntity { DepartmentId = 1, Name = "Engineering", GroupName = "R&D", ModifiedDate = DefaultAuditDate };

        _mockRepository.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dept);

        _mockRepository.Setup(x => x.GetDepartmentHeadcountAsync((short)departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _sut.Handle(
            new ReadDepartmentHeadcountQuery { DepartmentId = departmentId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.DepartmentId.Should().Be(1);
            result.DepartmentName.Should().Be("Engineering");
            result.ActiveEmployeeCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task Returns_correct_count_when_active_employees_existAsync()
    {
        const int departmentId = 1;
        var dept = new DepartmentEntity { DepartmentId = 1, Name = "Engineering", GroupName = "R&D", ModifiedDate = DefaultAuditDate };

        _mockRepository.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dept);

        _mockRepository.Setup(x => x.GetDepartmentHeadcountAsync((short)departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(15);

        var result = await _sut.Handle(
            new ReadDepartmentHeadcountQuery { DepartmentId = departmentId },
            CancellationToken.None);

        result.ActiveEmployeeCount.Should().Be(15);
    }
}
