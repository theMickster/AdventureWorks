using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

[ExcludeFromCodeCoverage]
public sealed class UpdateStoreAddressTypeCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IBusinessEntityAddressRepository> _mockBeaRepository = new();
    private readonly Mock<IValidator<StoreAddressUpdateModel>> _mockValidator = new();
    private UpdateStoreAddressTypeCommandHandler _sut;

    public UpdateStoreAddressTypeCommandHandlerTests()
    {
        _sut = new UpdateStoreAddressTypeCommandHandler(_mockBeaRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new UpdateStoreAddressTypeCommandHandler(
                    null!, _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("businessEntityAddressRepository");

            _ = ((Action)(() => _sut = new UpdateStoreAddressTypeCommandHandler(
                    _mockBeaRepository.Object, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("validator");
        }
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_request_is_nullAsync()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_throws_ValidationException_when_validator_failsAsync()
    {
        var command = new UpdateStoreAddressTypeCommand
        {
            StoreId = 2534,
            AddressId = 100,
            CurrentAddressTypeId = 2,
            Model = new StoreAddressUpdateModel { AddressTypeId = 0 },
            ModifiedDate = DefaultAuditDate
        };

        var failingValidator = new Setup.Fakes.FakeFailureValidator<StoreAddressUpdateModel>("AddressTypeId", "Bad type");
        _sut = new UpdateStoreAddressTypeCommandHandler(_mockBeaRepository.Object, failingValidator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_existing_address_not_foundAsync()
    {
        var command = new UpdateStoreAddressTypeCommand
        {
            StoreId = 2534,
            AddressId = 100,
            CurrentAddressTypeId = 2,
            Model = new StoreAddressUpdateModel { AddressTypeId = 3 },
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreAddressUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeaRepository.Setup(x => x.GetByCompositeKeyAsync(2534, 100, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessEntityAddressEntity?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_does_not_replace_when_target_equals_currentAsync()
    {
        var command = new UpdateStoreAddressTypeCommand
        {
            StoreId = 2534,
            AddressId = 100,
            CurrentAddressTypeId = 2,
            Model = new StoreAddressUpdateModel { AddressTypeId = 2 },
            ModifiedDate = DefaultAuditDate
        };

        var existing = new BusinessEntityAddressEntity
        {
            BusinessEntityId = 2534,
            AddressId = 100,
            AddressTypeId = 2,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreAddressUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeaRepository.Setup(x => x.GetByCompositeKeyAsync(2534, 100, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(100);
            _mockBeaRepository.Verify(x => x.ExistsAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockBeaRepository.Verify(x => x.ReplaceAddressTypeAsync(
                It.IsAny<BusinessEntityAddressEntity>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Fact]
    public async Task Handle_throws_ValidationException_with_Rule02_when_target_already_existsAsync()
    {
        var command = new UpdateStoreAddressTypeCommand
        {
            StoreId = 2534,
            AddressId = 100,
            CurrentAddressTypeId = 2,
            Model = new StoreAddressUpdateModel { AddressTypeId = 3 },
            ModifiedDate = DefaultAuditDate
        };

        var existing = new BusinessEntityAddressEntity
        {
            BusinessEntityId = 2534,
            AddressId = 100,
            AddressTypeId = 2,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreAddressUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeaRepository.Setup(x => x.GetByCompositeKeyAsync(2534, 100, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _mockBeaRepository.Setup(x => x.ExistsAsync(2534, 100, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Should().Contain(e => e.ErrorCode == "Rule-02");
    }

    [Fact]
    public async Task Handle_replaces_address_type_and_returns_address_idAsync()
    {
        var command = new UpdateStoreAddressTypeCommand
        {
            StoreId = 2534,
            AddressId = 100,
            CurrentAddressTypeId = 2,
            Model = new StoreAddressUpdateModel { AddressTypeId = 3 },
            ModifiedDate = DefaultAuditDate
        };

        var existing = new BusinessEntityAddressEntity
        {
            BusinessEntityId = 2534,
            AddressId = 100,
            AddressTypeId = 2,
            ModifiedDate = DefaultAuditDate
        };

        var replacement = new BusinessEntityAddressEntity
        {
            BusinessEntityId = 2534,
            AddressId = 100,
            AddressTypeId = 3,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreAddressUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeaRepository.Setup(x => x.GetByCompositeKeyAsync(2534, 100, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _mockBeaRepository.Setup(x => x.ExistsAsync(2534, 100, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockBeaRepository.Setup(x => x.ReplaceAddressTypeAsync(existing, 3, DefaultAuditDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(replacement);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(100);
            _mockBeaRepository.Verify(x => x.ReplaceAddressTypeAsync(existing, 3, DefaultAuditDate, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
