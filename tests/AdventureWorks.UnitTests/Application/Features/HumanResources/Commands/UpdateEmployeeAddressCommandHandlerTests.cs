using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fakes;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Commands;

[ExcludeFromCodeCoverage]
public sealed class UpdateEmployeeAddressCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly Mock<IAddressRepository> _mockAddressRepository = new();
    private readonly Mock<IValidator<EmployeeAddressUpdateModel>> _mockValidator = new();
    private UpdateEmployeeAddressCommandHandler _sut;

    public UpdateEmployeeAddressCommandHandlerTests()
    {
        _sut = new UpdateEmployeeAddressCommandHandler(
            _mockEmployeeRepository.Object,
            _mockAddressRepository.Object,
            _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public void Handle_throws_exception_when_request_is_null()
    {
        ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_exception_when_request_model_is_null()
    {
        var command = new UpdateEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            Model = null!,
            ModifiedDate = DefaultAuditDate
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request.Model");
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_model_is_invalid()
    {
        var command = new UpdateEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            Model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel(),
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<EmployeeAddressUpdateModel>(
            "AddressLine1",
            "Address line 1 is required");

        _sut = new UpdateEmployeeAddressCommandHandler(
            _mockEmployeeRepository.Object,
            _mockAddressRepository.Object,
            validator);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors
            .Count(x => x.ErrorMessage == "Address line 1 is required")
            .Should().Be(1);
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_employee_not_found()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        var command = new UpdateEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeAddressUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeEntity?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Employee with ID 100 not found.");
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_business_entity_address_not_found()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        var command = new UpdateEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeAddressUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidEmployeeEntity());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(100, model.AddressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessEntityAddressEntity?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Address with ID {model.AddressId} not found for employee 100.");
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_address_entity_not_found()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        var command = new UpdateEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeAddressUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidEmployeeEntity());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(100, model.AddressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidBusinessEntityAddress());

        _mockAddressRepository
            .Setup(x => x.GetByIdAsync(model.AddressId))
            .ReturnsAsync((AddressEntity?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Address with ID {model.AddressId} not found.");
    }

    [Fact]
    public async Task Handle_updates_address_fields_correctly()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        var addressEntity = HumanResourcesDomainFixtures.GetValidAddressEntity();

        var command = new UpdateEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeAddressUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidEmployeeEntity());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(100, model.AddressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidBusinessEntityAddress());

        _mockAddressRepository
            .Setup(x => x.GetByIdAsync(model.AddressId))
            .ReturnsAsync(addressEntity);

        _mockAddressRepository
            .Setup(x => x.UpdateAsync(It.IsAny<AddressEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            addressEntity.AddressLine1.Should().Be(model.AddressLine1);
            addressEntity.AddressLine2.Should().Be(model.AddressLine2);
            addressEntity.City.Should().Be(model.City);
            addressEntity.StateProvinceId.Should().Be(model.StateProvinceId);
            addressEntity.PostalCode.Should().Be(model.PostalCode);
            addressEntity.ModifiedDate.Should().Be(DefaultAuditDate);
        }

        _mockAddressRepository.Verify(
            x => x.UpdateAsync(addressEntity),
            Times.Once);
    }

    [Fact]
    public async Task Handle_returns_Unit_value_when_successful()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        var addressEntity = HumanResourcesDomainFixtures.GetValidAddressEntity();

        var command = new UpdateEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeAddressUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidEmployeeEntity());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(100, model.AddressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidBusinessEntityAddress());

        _mockAddressRepository
            .Setup(x => x.GetByIdAsync(model.AddressId))
            .ReturnsAsync(addressEntity);

        _mockAddressRepository
            .Setup(x => x.UpdateAsync(It.IsAny<AddressEntity>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
    }
}
