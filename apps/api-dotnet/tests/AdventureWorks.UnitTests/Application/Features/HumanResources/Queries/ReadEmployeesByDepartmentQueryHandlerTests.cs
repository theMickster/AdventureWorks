using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeesByDepartmentQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IDepartmentRepository> _mockRepository = new();
    private ReadEmployeesByDepartmentQueryHandler _sut;

    public ReadEmployeesByDepartmentQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(EmployeeEntityToModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadEmployeesByDepartmentQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_when_mapper_is_null()
    {
        ((Action)(() => _sut = new ReadEmployeesByDepartmentQueryHandler(null!, _mockRepository.Object)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("mapper");
    }

    [Fact]
    public void Constructor_throws_when_repository_is_null()
    {
        ((Action)(() => _sut = new ReadEmployeesByDepartmentQueryHandler(_mapper, null!)))
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
            new ReadEmployeesByDepartmentQuery { DepartmentId = departmentId },
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Department with ID {departmentId} not found.");
    }

    [Fact]
    public async Task Returns_mapped_employees_and_total_countAsync()
    {
        const int departmentId = 1;
        var dept = new DepartmentEntity { DepartmentId = 1, Name = "Engineering", GroupName = "R&D", ModifiedDate = DefaultAuditDate };
        var employees = new List<EmployeeEntity>
        {
            HumanResourcesDomainFixtures.GetCompleteEmployeeEntity(1, "Alice", "Anderson"),
            HumanResourcesDomainFixtures.GetCompleteEmployeeEntity(2, "Bob", "Brown")
        }.AsReadOnly();

        _mockRepository.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dept);

        _mockRepository.Setup(x => x.GetEmployeesByDepartmentAsync((short)departmentId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((employees, 2));

        var (result, totalCount) = await _sut.Handle(
            new ReadEmployeesByDepartmentQuery { DepartmentId = departmentId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().HaveCount(2);
            totalCount.Should().Be(2);
            result[0].FirstName.Should().Be("Alice");
        }
    }

    [Fact]
    public async Task Returns_empty_list_and_zero_count_when_no_employees_existAsync()
    {
        const int departmentId = 1;
        var dept = new DepartmentEntity { DepartmentId = 1, Name = "Engineering", GroupName = "R&D", ModifiedDate = DefaultAuditDate };

        _mockRepository.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dept);

        _mockRepository.Setup(x => x.GetEmployeesByDepartmentAsync((short)departmentId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<EmployeeEntity>().AsReadOnly(), 0));

        var (result, totalCount) = await _sut.Handle(
            new ReadEmployeesByDepartmentQuery { DepartmentId = departmentId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().BeEmpty();
            totalCount.Should().Be(0);
        }
    }
}
