using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

[ExcludeFromCodeCoverage]
public sealed class AddStoreAddressCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IBusinessEntityAddressRepository> _mockBeaRepository = new();
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private readonly Mock<IValidator<StoreAddressCreateModel>> _mockValidator = new();
    private AddStoreAddressCommandHandler _sut;

    public AddStoreAddressCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(StoreAddressCreateModelToBusinessEntityAddressEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new AddStoreAddressCommandHandler(
            _mapper,
            _mockBeaRepository.Object,
            _mockStoreRepository.Object,
            _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new AddStoreAddressCommandHandler(
                    null!,
                    _mockBeaRepository.Object,
                    _mockStoreRepository.Object,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new AddStoreAddressCommandHandler(
                    _mapper,
                    null!,
                    _mockStoreRepository.Object,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("businessEntityAddressRepository");

            _ = ((Action)(() => _sut = new AddStoreAddressCommandHandler(
                    _mapper,
                    _mockBeaRepository.Object,
                    null!,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("storeRepository");

            _ = ((Action)(() => _sut = new AddStoreAddressCommandHandler(
                    _mapper,
                    _mockBeaRepository.Object,
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
        var command = new AddStoreAddressCommand
        {
            StoreId = 9999,
            Model = new StoreAddressCreateModel { AddressId = 1, AddressTypeId = 2 },
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
        var command = new AddStoreAddressCommand
        {
            StoreId = 2534,
            Model = new StoreAddressCreateModel { AddressId = 0, AddressTypeId = 0 },
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockStoreRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var failingValidator = new Setup.Fakes.FakeFailureValidator<StoreAddressCreateModel>("AddressId", "Bad address");
        _sut = new AddStoreAddressCommandHandler(
            _mapper,
            _mockBeaRepository.Object,
            _mockStoreRepository.Object,
            failingValidator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Count(e => e.ErrorMessage == "Bad address").Should().Be(1);
    }

    [Fact]
    public async Task Handle_throws_ValidationException_with_Rule03_when_address_already_existsAsync()
    {
        var command = new AddStoreAddressCommand
        {
            StoreId = 2534,
            Model = new StoreAddressCreateModel { AddressId = 100, AddressTypeId = 2 },
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockStoreRepository.Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreAddressCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeaRepository.Setup(x => x.ExistsAsync(2534, 100, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Should().Contain(e => e.ErrorCode == "Rule-03");
    }

    [Fact]
    public async Task Handle_persists_entity_with_audit_fieldsAsync()
    {
        var command = new AddStoreAddressCommand
        {
            StoreId = 2534,
            Model = new StoreAddressCreateModel { AddressId = 100, AddressTypeId = 2 },
            ModifiedDate = DefaultAuditDate,
            RowGuid = new Guid("5ec92f1e-232b-430e-a729-ea59c943e3fc")
        };

        _mockStoreRepository.Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreAddressCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeaRepository.Setup(x => x.ExistsAsync(2534, 100, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockBeaRepository.Setup(x => x.AddAsync(It.IsAny<BusinessEntityAddressEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessEntityAddressEntity e, CancellationToken _) => e);

        await _sut.Handle(command, CancellationToken.None);

        _mockBeaRepository.Verify(x => x.AddAsync(
            It.Is<BusinessEntityAddressEntity>(e =>
                e.BusinessEntityId == 2534 &&
                e.AddressId == 100 &&
                e.AddressTypeId == 2 &&
                e.ModifiedDate == DefaultAuditDate &&
                e.Rowguid == command.RowGuid),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_returns_address_idAsync()
    {
        var command = new AddStoreAddressCommand
        {
            StoreId = 2534,
            Model = new StoreAddressCreateModel { AddressId = 100, AddressTypeId = 2 },
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockStoreRepository.Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreAddressCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeaRepository.Setup(x => x.ExistsAsync(2534, 100, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockBeaRepository.Setup(x => x.AddAsync(It.IsAny<BusinessEntityAddressEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessEntityAddressEntity e, CancellationToken _) => e);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(100);
    }

    [Fact]
    public async Task Handle_does_not_swallow_DbUpdateException_from_repositoryAsync()
    {
        var command = new AddStoreAddressCommand
        {
            StoreId = 2534,
            Model = new StoreAddressCreateModel { AddressId = 100, AddressTypeId = 2 },
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockStoreRepository.Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreAddressCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockBeaRepository.Setup(x => x.ExistsAsync(2534, 100, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockBeaRepository.Setup(x => x.AddAsync(It.IsAny<BusinessEntityAddressEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Race-condition collision"));

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
