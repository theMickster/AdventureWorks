using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeeDepartmentHistoryQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private ReadEmployeeDepartmentHistoryQueryHandler _sut;

    public ReadEmployeeDepartmentHistoryQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(EmployeeDepartmentHistoryEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadEmployeeDepartmentHistoryQueryHandler(_mapper, _mockEmployeeRepository.Object);
    }

    [Fact]
    public void Constructor_throws_when_mapper_is_null()
    {
        ((Action)(() => _sut = new ReadEmployeeDepartmentHistoryQueryHandler(null!, _mockEmployeeRepository.Object)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("mapper");
    }

    [Fact]
    public void Constructor_throws_when_employeeRepository_is_null()
    {
        ((Action)(() => _sut = new ReadEmployeeDepartmentHistoryQueryHandler(_mapper, null!)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("employeeRepository");
    }

    [Fact]
    public async Task Handle_throws_when_request_is_nullAsync()
    {
        await ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_key_not_found_when_employee_not_foundAsync()
    {
        const int businessEntityId = 999;

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeEntity?)null);

        Func<Task> act = async () => await _sut.Handle(
            new ReadEmployeeDepartmentHistoryQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Employee with ID {businessEntityId} not found.");
    }

    [Fact]
    public async Task Handle_returns_mapped_history_when_employee_existsAsync()
    {
        const int businessEntityId = 1;

        var employee = HumanResourcesDomainFixtures.GetValidEmployeeEntity(businessEntityId);

        var history = new List<EmployeeDepartmentHistory>
        {
            new()
            {
                BusinessEntityId = businessEntityId,
                DepartmentId = 2,
                ShiftId = 1,
                StartDate = new DateTime(2023, 6, 1),
                EndDate = null,
                ModifiedDate = DefaultAuditDate,
                Department = new DepartmentEntity { DepartmentId = 2, Name = "Tool Design", GroupName = "Research and Development" },
                Shift = new ShiftEntity { ShiftId = 1, Name = "Day" }
            },
            new()
            {
                BusinessEntityId = businessEntityId,
                DepartmentId = 1,
                ShiftId = 1,
                StartDate = new DateTime(2020, 1, 10),
                EndDate = new DateTime(2023, 5, 31),
                ModifiedDate = DefaultAuditDate,
                Department = new DepartmentEntity { DepartmentId = 1, Name = "Engineering", GroupName = "Research and Development" },
                Shift = new ShiftEntity { ShiftId = 1, Name = "Day" }
            }
        }.AsReadOnly();

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeDepartmentHistoryAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        var result = await _sut.Handle(
            new ReadEmployeeDepartmentHistoryQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(history.Count);

            result[0].DepartmentId.Should().Be(2);
            result[0].DepartmentName.Should().Be("Tool Design");
            result[0].ShiftName.Should().Be("Day");
            result[0].EndDate.Should().BeNull();

            result[1].DepartmentId.Should().Be(1);
            result[1].DepartmentName.Should().Be("Engineering");
            result[1].EndDate.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Handle_returns_empty_list_when_employee_has_no_historyAsync()
    {
        const int businessEntityId = 1;

        var employee = HumanResourcesDomainFixtures.GetValidEmployeeEntity(businessEntityId);

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeDepartmentHistoryAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmployeeDepartmentHistory>().AsReadOnly());

        var result = await _sut.Handle(
            new ReadEmployeeDepartmentHistoryQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
