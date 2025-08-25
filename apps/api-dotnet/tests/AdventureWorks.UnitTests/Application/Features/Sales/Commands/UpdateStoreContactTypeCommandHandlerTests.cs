using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

[ExcludeFromCodeCoverage]
public sealed class UpdateStoreContactTypeCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IBusinessEntityContactEntityRepository> _mockBeceRepository = new();
    private readonly Mock<IValidator<StoreContactUpdateModel>> _mockValidator = new();
    private UpdateStoreContactTypeCommandHandler _sut;

    public UpdateStoreContactTypeCommandHandlerTests()
    {
        _sut = new UpdateStoreContactTypeCommandHandler(_mockBeceRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new UpdateStoreContactTypeCommandHandler(
                    null!, _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("businessEntityContactRepository");

            _ = ((Action)(() => _sut = new UpdateStoreContactTypeCommandHandler(
                    _mockBeceRepository.Object, null!)))
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
        var command = new UpdateStoreContactTypeCommand
        {
            StoreId = 2534,
            PersonId = 100,
            CurrentContactTypeId = 11,
            Model = new StoreContactUpdateModel { ContactTypeId = 0 },
            ModifiedDate = DefaultAuditDate
        };

        var failingValidator = new Setup.Fakes.FakeFailureValidator<StoreContactUpdateModel>("ContactTypeId", "Bad type");
        _sut = new UpdateStoreContactTypeCommandHandler(_mockBeceRepository.Object, failingValidator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_existing_contact_not_foundAsync()
    {
        var command = new UpdateStoreContactTypeCommand
        {
            StoreId = 2534,
            PersonId = 100,
            CurrentContactTypeId = 11,
            Model = new StoreContactUpdateModel { ContactTypeId = 12 },
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreContactUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeceRepository.Setup(x => x.GetByCompositeKeyAsync(2534, 100, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessEntityContactEntity?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_does_not_replace_when_target_equals_currentAsync()
    {
        var command = new UpdateStoreContactTypeCommand
        {
            StoreId = 2534,
            PersonId = 100,
            CurrentContactTypeId = 11,
            Model = new StoreContactUpdateModel { ContactTypeId = 11 },
            ModifiedDate = DefaultAuditDate
        };

        var existing = new BusinessEntityContactEntity
        {
            BusinessEntityId = 2534,
            PersonId = 100,
            ContactTypeId = 11,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreContactUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeceRepository.Setup(x => x.GetByCompositeKeyAsync(2534, 100, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(100);
            _mockBeceRepository.Verify(x => x.ReplaceContactTypeAsync(
                It.IsAny<BusinessEntityContactEntity>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Fact]
    public async Task Handle_throws_ValidationException_with_Rule02_when_target_already_existsAsync()
    {
        var command = new UpdateStoreContactTypeCommand
        {
            StoreId = 2534,
            PersonId = 100,
            CurrentContactTypeId = 11,
            Model = new StoreContactUpdateModel { ContactTypeId = 12 },
            ModifiedDate = DefaultAuditDate
        };

        var existing = new BusinessEntityContactEntity
        {
            BusinessEntityId = 2534,
            PersonId = 100,
            ContactTypeId = 11,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreContactUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeceRepository.Setup(x => x.GetByCompositeKeyAsync(2534, 100, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _mockBeceRepository.Setup(x => x.ExistsAsync(2534, 100, 12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Should().Contain(e => e.ErrorCode == "Rule-02");
    }

    [Fact]
    public async Task Handle_replaces_contact_type_and_returns_person_idAsync()
    {
        var command = new UpdateStoreContactTypeCommand
        {
            StoreId = 2534,
            PersonId = 100,
            CurrentContactTypeId = 11,
            Model = new StoreContactUpdateModel { ContactTypeId = 12 },
            ModifiedDate = DefaultAuditDate
        };

        var existing = new BusinessEntityContactEntity
        {
            BusinessEntityId = 2534,
            PersonId = 100,
            ContactTypeId = 11,
            ModifiedDate = DefaultAuditDate
        };

        var replacement = new BusinessEntityContactEntity
        {
            BusinessEntityId = 2534,
            PersonId = 100,
            ContactTypeId = 12,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreContactUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeceRepository.Setup(x => x.GetByCompositeKeyAsync(2534, 100, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _mockBeceRepository.Setup(x => x.ExistsAsync(2534, 100, 12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockBeceRepository.Setup(x => x.ReplaceContactTypeAsync(existing, 12, DefaultAuditDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(replacement);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(100);
            _mockBeceRepository.Verify(x => x.ReplaceContactTypeAsync(existing, 12, DefaultAuditDate, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
