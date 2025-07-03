using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.Features.HumanResources.Profiles;
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
using Microsoft.AspNetCore.JsonPatch;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Commands;

[ExcludeFromCodeCoverage]
public sealed class PatchEmployeeAddressCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly Mock<IAddressRepository> _mockAddressRepository = new();
    private readonly Mock<IValidator<EmployeeAddressUpdateModel>> _mockValidator = new();
    private PatchEmployeeAddressCommandHandler _sut;

    public PatchEmployeeAddressCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(EmployeeAddressProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();

        _sut = new PatchEmployeeAddressCommandHandler(
            _mapper,
            _mockEmployeeRepository.Object,
            _mockAddressRepository.Object,
            _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new PatchEmployeeAddressCommandHandler(
                    null!,
                    _mockEmployeeRepository.Object,
                    _mockAddressRepository.Object,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new PatchEmployeeAddressCommandHandler(
                    _mapper,
                    null!,
                    _mockAddressRepository.Object,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("employeeRepository");

            _ = ((Action)(() => _sut = new PatchEmployeeAddressCommandHandler(
                    _mapper,
                    _mockEmployeeRepository.Object,
                    null!,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("addressRepository");

            _ = ((Action)(() => _sut = new PatchEmployeeAddressCommandHandler(
                    _mapper,
                    _mockEmployeeRepository.Object,
                    _mockAddressRepository.Object,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("validator");
        }
    }

    [Fact]
    public void Handle_throws_exception_when_request_is_null()
    {
        ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_exception_when_patch_document_is_null()
    {
        var command = new PatchEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            AddressId = 1,
            PatchDocument = null!,
            ModifiedDate = DefaultAuditDate
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_employee_not_found()
    {
        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.City, "New City");

        var command = new PatchEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            AddressId = 1,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

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
        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.City, "New City");

        var command = new PatchEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            AddressId = 1,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidEmployeeEntity());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(100, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessEntityAddressEntity?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Address with ID 1 not found for employee 100.");
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_address_entity_not_found()
    {
        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.City, "New City");

        var command = new PatchEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            AddressId = 1,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidEmployeeEntity());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(100, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidBusinessEntityAddress());

        _mockAddressRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((AddressEntity?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Address with ID 1 not found.");
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_patched_model_is_invalid()
    {
        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.City, ""); // Empty city should fail validation

        var command = new PatchEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            AddressId = 1,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<EmployeeAddressUpdateModel>(
            "City",
            "City is required");

        _sut = new PatchEmployeeAddressCommandHandler(
            _mapper,
            _mockEmployeeRepository.Object,
            _mockAddressRepository.Object,
            validator);

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidEmployeeEntity());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(100, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidBusinessEntityAddress());

        _mockAddressRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidAddressEntity());

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors
            .Count(x => x.ErrorMessage == "City is required")
            .Should().Be(1);
    }

    [Fact]
    public async Task Handle_successfully_applies_patch_and_updates_address()
    {
        var addressEntity = HumanResourcesDomainFixtures.GetValidAddressEntity(1);
        addressEntity.City = "Seattle";
        addressEntity.PostalCode = "98101";

        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.City, "Portland");
        patchDocument.Replace(x => x.PostalCode, "97201");

        var command = new PatchEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            AddressId = 1,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeAddressUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidEmployeeEntity());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(100, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidBusinessEntityAddress());

        _mockAddressRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(addressEntity);

        _mockAddressRepository
            .Setup(x => x.UpdateAsync(It.IsAny<AddressEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            addressEntity.City.Should().Be("Portland", "because the patch document replaced the city");
            addressEntity.PostalCode.Should().Be("97201", "because the patch document replaced the postal code");
            addressEntity.ModifiedDate.Should().Be(DefaultAuditDate);
        }

        _mockAddressRepository.Verify(
            x => x.UpdateAsync(addressEntity),
            Times.Once);
    }

    [Fact]
    public async Task Handle_applies_multiple_patch_operations_correctly()
    {
        var addressEntity = HumanResourcesDomainFixtures.GetValidAddressEntity(1);
        addressEntity.AddressLine1 = "123 Main Street";
        addressEntity.AddressLine2 = "Apt 4B";
        addressEntity.City = "Seattle";
        addressEntity.StateProvinceId = 79;
        addressEntity.PostalCode = "98101";

        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.AddressLine1, "456 Oak Avenue");
        patchDocument.Replace(x => x.AddressLine2, null);
        patchDocument.Replace(x => x.City, "Portland");
        patchDocument.Replace(x => x.PostalCode, "97201");

        var command = new PatchEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            AddressId = 1,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeAddressUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidEmployeeEntity());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(100, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidBusinessEntityAddress());

        _mockAddressRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(addressEntity);

        _mockAddressRepository
            .Setup(x => x.UpdateAsync(It.IsAny<AddressEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            addressEntity.AddressLine1.Should().Be("456 Oak Avenue");
            addressEntity.AddressLine2.Should().BeNull();
            addressEntity.City.Should().Be("Portland");
            addressEntity.PostalCode.Should().Be("97201");
            addressEntity.StateProvinceId.Should().Be(79, "because StateProvinceId was not patched");
            addressEntity.ModifiedDate.Should().Be(DefaultAuditDate);
        }
    }

    [Fact]
    public async Task Handle_returns_Unit_value_when_successful()
    {
        var addressEntity = HumanResourcesDomainFixtures.GetValidAddressEntity(1);

        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.City, "New City");

        var command = new PatchEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            AddressId = 1,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeAddressUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidEmployeeEntity());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(100, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidBusinessEntityAddress());

        _mockAddressRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(addressEntity);

        _mockAddressRepository
            .Setup(x => x.UpdateAsync(It.IsAny<AddressEntity>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_validates_patched_model_before_updating()
    {
        var addressEntity = HumanResourcesDomainFixtures.GetValidAddressEntity(1);

        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.City, "Updated City");
        patchDocument.Replace(x => x.PostalCode, "12345");

        var command = new PatchEmployeeAddressCommand
        {
            BusinessEntityId = 100,
            AddressId = 1,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<EmployeeAddressUpdateModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidEmployeeEntity());

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(100, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidBusinessEntityAddress());

        _mockAddressRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(addressEntity);

        _mockAddressRepository
            .Setup(x => x.UpdateAsync(It.IsAny<AddressEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        _mockValidator.Verify(
            x => x.ValidateAsync(It.IsAny<ValidationContext<EmployeeAddressUpdateModel>>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "because validation should occur after applying patch and before updating entity");
    }
}
