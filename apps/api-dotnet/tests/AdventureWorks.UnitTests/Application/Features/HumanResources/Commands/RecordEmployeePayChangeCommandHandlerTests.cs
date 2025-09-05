using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fakes;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Commands;

[ExcludeFromCodeCoverage]
public sealed class RecordEmployeePayChangeCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly Mock<IValidator<EmployeePayChangeCreateModel>> _mockValidator = new();
    private readonly Mock<ILogger<RecordEmployeePayChangeCommandHandler>> _mockLogger = new();
    private RecordEmployeePayChangeCommandHandler _sut;

    public RecordEmployeePayChangeCommandHandlerTests()
    {
        _sut = new RecordEmployeePayChangeCommandHandler(
            _mockEmployeeRepository.Object,
            _mockValidator.Object,
            _mockLogger.Object);
    }

    private static RecordEmployeePayChangeCommand BuildCommand(
        int employeeId = 1,
        decimal rate = 50.00m,
        byte payFrequency = 2) => new()
    {
        EmployeeId = employeeId,
        Model = new EmployeePayChangeCreateModel { Rate = rate, PayFrequency = payFrequency },
        ModifiedDate = new DateTime(2026, 1, 15),
        RateChangeDate = new DateTime(2026, 1, 15)
    };

    [Fact]
    public void Constructor_throws_when_employeeRepository_is_null()
    {
        ((Action)(() => _ = new RecordEmployeePayChangeCommandHandler(
            null!,
            _mockValidator.Object,
            _mockLogger.Object)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("employeeRepository");
    }

    [Fact]
    public void Constructor_throws_when_validator_is_null()
    {
        ((Action)(() => _ = new RecordEmployeePayChangeCommandHandler(
            _mockEmployeeRepository.Object,
            null!,
            _mockLogger.Object)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("validator");
    }

    [Fact]
    public void Constructor_throws_when_logger_is_null()
    {
        ((Action)(() => _ = new RecordEmployeePayChangeCommandHandler(
            _mockEmployeeRepository.Object,
            _mockValidator.Object,
            null!)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    [Fact]
    public async Task Null_request_throws_ArgumentNullExceptionAsync()
    {
        Func<Task> act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Null_model_throws_ArgumentNullExceptionAsync()
    {
        var command = new RecordEmployeePayChangeCommand
        {
            EmployeeId = 1,
            Model = null!,
            ModifiedDate = DefaultAuditDate,
            RateChangeDate = DefaultAuditDate
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request.Model");
    }

    [Fact]
    public async Task ValidateAndThrow_throws_ValidationException_propagatesAsync()
    {
        var command = BuildCommand();

        var fakeValidator = new FakeFailureValidator<EmployeePayChangeCreateModel>(
            "Rate",
            "Rate must be greater than zero.");

        _sut = new RecordEmployeePayChangeCommandHandler(
            _mockEmployeeRepository.Object,
            fakeValidator,
            _mockLogger.Object);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Employee_not_found_throws_KeyNotFoundExceptionAsync()
    {
        var command = BuildCommand(employeeId: 999);

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeEntity?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Employee with ID 999 not found.");
    }

    [Fact]
    public async Task Inactive_employee_throws_ValidationException_with_Rule04Async()
    {
        var command = BuildCommand(employeeId: 1);
        var employee = HumanResourcesDomainFixtures.GetInactiveEmployeeEntity(businessEntityId: 1);

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Single().ErrorCode.Should().Be("Rule-04");
    }

    [Fact]
    public async Task Happy_path_calls_RecordPayChangeAsync_with_correct_recordAsync()
    {
        var command = BuildCommand(employeeId: 1, rate: 50.00m, payFrequency: 2);
        var employee = HumanResourcesDomainFixtures.GetValidEmployeeEntity(businessEntityId: 1);

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        EmployeePayHistory? capturedRecord = null;
        _mockEmployeeRepository
            .Setup(x => x.RecordPayChangeAsync(It.IsAny<EmployeePayHistory>(), It.IsAny<CancellationToken>()))
            .Callback<EmployeePayHistory, CancellationToken>((r, _) => capturedRecord = r)
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(Unit.Value);
            capturedRecord.Should().NotBeNull();
            capturedRecord!.BusinessEntityId.Should().Be(1);
            capturedRecord.Rate.Should().Be(50.00m);
            capturedRecord.PayFrequency.Should().Be(2);
            capturedRecord.RateChangeDate.Should().Be(new DateTime(2026, 1, 15));
            capturedRecord.ModifiedDate.Should().Be(new DateTime(2026, 1, 15));
        }
    }
}
