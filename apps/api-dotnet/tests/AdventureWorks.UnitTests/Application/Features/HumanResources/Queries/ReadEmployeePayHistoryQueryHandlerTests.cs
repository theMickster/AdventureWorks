using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeePayHistoryQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private ReadEmployeePayHistoryQueryHandler _sut;

    public ReadEmployeePayHistoryQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(EmployeePayHistoryEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadEmployeePayHistoryQueryHandler(_mapper, _mockEmployeeRepository.Object);
    }

    [Fact]
    public void Constructor_throws_when_mapper_is_null()
    {
        ((Action)(() => _sut = new ReadEmployeePayHistoryQueryHandler(null!, _mockEmployeeRepository.Object)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("mapper");
    }

    [Fact]
    public void Constructor_throws_when_employeeRepository_is_null()
    {
        ((Action)(() => _sut = new ReadEmployeePayHistoryQueryHandler(_mapper, null!)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("employeeRepository");
    }

    [Fact]
    public async Task Null_request_throws_ArgumentNullExceptionAsync()
    {
        await ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Employee_not_found_throws_KeyNotFoundExceptionAsync()
    {
        const int businessEntityId = 999;

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeEntity?)null);

        Func<Task> act = async () => await _sut.Handle(
            new ReadEmployeePayHistoryQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Employee with ID {businessEntityId} not found.");
    }

    [Fact]
    public async Task Returns_mapped_history_when_records_existAsync()
    {
        const int businessEntityId = 1;

        var employee = HumanResourcesDomainFixtures.GetValidEmployeeEntity(businessEntityId);

        var history = new List<EmployeePayHistory>
        {
            new()
            {
                BusinessEntityId = businessEntityId,
                RateChangeDate = new DateTime(2025, 1, 1),
                Rate = 75.00m,
                PayFrequency = 2,
                ModifiedDate = DefaultAuditDate
            },
            new()
            {
                BusinessEntityId = businessEntityId,
                RateChangeDate = new DateTime(2024, 1, 1),
                Rate = 50.00m,
                PayFrequency = 1,
                ModifiedDate = DefaultAuditDate
            }
        }.AsReadOnly();

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeePayHistoryAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        var result = await _sut.Handle(
            new ReadEmployeePayHistoryQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            result[0].Rate.Should().Be(75.00m);
            result[0].PayFrequency.Should().Be(2);
            result[0].PayFrequencyLabel.Should().Be("Bi-Weekly");

            result[1].Rate.Should().Be(50.00m);
            result[1].PayFrequency.Should().Be(1);
            result[1].PayFrequencyLabel.Should().Be("Monthly");
        }
    }

    [Fact]
    public async Task Empty_history_returns_empty_list_not_exceptionAsync()
    {
        const int businessEntityId = 1;

        var employee = HumanResourcesDomainFixtures.GetValidEmployeeEntity(businessEntityId);

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeePayHistoryAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmployeePayHistory>().AsReadOnly());

        var result = await _sut.Handle(
            new ReadEmployeePayHistoryQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
