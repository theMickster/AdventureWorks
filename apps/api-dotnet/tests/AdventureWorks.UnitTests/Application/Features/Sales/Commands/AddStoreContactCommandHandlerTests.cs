using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

[ExcludeFromCodeCoverage]
public sealed class AddStoreContactCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IBusinessEntityContactEntityRepository> _mockBeceRepository = new();
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private readonly Mock<IValidator<StoreContactCreateModel>> _mockValidator = new();
    private AddStoreContactCommandHandler _sut;

    public AddStoreContactCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(StoreContactCreateModelToBusinessEntityContactEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new AddStoreContactCommandHandler(
            _mapper,
            _mockBeceRepository.Object,
            _mockStoreRepository.Object,
            _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new AddStoreContactCommandHandler(
                    null!,
                    _mockBeceRepository.Object,
                    _mockStoreRepository.Object,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new AddStoreContactCommandHandler(
                    _mapper,
                    null!,
                    _mockStoreRepository.Object,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("businessEntityContactRepository");

            _ = ((Action)(() => _sut = new AddStoreContactCommandHandler(
                    _mapper,
                    _mockBeceRepository.Object,
                    null!,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("storeRepository");

            _ = ((Action)(() => _sut = new AddStoreContactCommandHandler(
                    _mapper,
                    _mockBeceRepository.Object,
                    _mockStoreRepository.Object,
                    null!)))
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
    public async Task Handle_throws_KeyNotFoundException_when_store_does_not_existAsync()
    {
        var command = new AddStoreContactCommand
        {
            StoreId = 9999,
            Model = new StoreContactCreateModel { PersonId = 1, ContactTypeId = 11 },
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockStoreRepository.Setup(x => x.ExistsAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_throws_ValidationException_when_validator_failsAsync()
    {
        var command = new AddStoreContactCommand
        {
            StoreId = 2534,
            Model = new StoreContactCreateModel { PersonId = 0, ContactTypeId = 0 },
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockStoreRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var failingValidator = new Setup.Fakes.FakeFailureValidator<StoreContactCreateModel>("PersonId", "Bad person");
        _sut = new AddStoreContactCommandHandler(
            _mapper,
            _mockBeceRepository.Object,
            _mockStoreRepository.Object,
            failingValidator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Count(e => e.ErrorMessage == "Bad person").Should().Be(1);
    }

    [Fact]
    public async Task Handle_throws_ValidationException_with_Rule03_when_contact_already_existsAsync()
    {
        var command = new AddStoreContactCommand
        {
            StoreId = 2534,
            Model = new StoreContactCreateModel { PersonId = 100, ContactTypeId = 11 },
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockStoreRepository.Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreContactCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeceRepository.Setup(x => x.ExistsAsync(2534, 100, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Should().Contain(e => e.ErrorCode == "Rule-03");
    }

    [Fact]
    public async Task Handle_persists_entity_with_audit_fieldsAsync()
    {
        var command = new AddStoreContactCommand
        {
            StoreId = 2534,
            Model = new StoreContactCreateModel { PersonId = 100, ContactTypeId = 11 },
            ModifiedDate = DefaultAuditDate,
            RowGuid = new Guid("5ec92f1e-232b-430e-a729-ea59c943e3fc")
        };

        _mockStoreRepository.Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreContactCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeceRepository.Setup(x => x.ExistsAsync(2534, 100, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockBeceRepository.Setup(x => x.AddAsync(It.IsAny<BusinessEntityContactEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessEntityContactEntity e, CancellationToken _) => e);

        await _sut.Handle(command, CancellationToken.None);

        _mockBeceRepository.Verify(x => x.AddAsync(
            It.Is<BusinessEntityContactEntity>(e =>
                e.BusinessEntityId == 2534 &&
                e.PersonId == 100 &&
                e.ContactTypeId == 11 &&
                e.ModifiedDate == DefaultAuditDate &&
                e.Rowguid == command.RowGuid),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_returns_person_idAsync()
    {
        var command = new AddStoreContactCommand
        {
            StoreId = 2534,
            Model = new StoreContactCreateModel { PersonId = 100, ContactTypeId = 11 },
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockStoreRepository.Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreContactCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeceRepository.Setup(x => x.ExistsAsync(2534, 100, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockBeceRepository.Setup(x => x.AddAsync(It.IsAny<BusinessEntityContactEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessEntityContactEntity e, CancellationToken _) => e);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(100);
    }
}
