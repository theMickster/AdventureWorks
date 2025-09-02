using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Constants;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

[ExcludeFromCodeCoverage]
public sealed class ReassignStoreSalesPersonCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private readonly Mock<ISalesPersonRepository> _mockSalesPersonRepository = new();
    private readonly Mock<IValidator<StoreSalesPersonAssignmentCreateModel>> _mockValidator = new();
    private ReassignStoreSalesPersonCommandHandler _sut;

    public ReassignStoreSalesPersonCommandHandlerTests()
    {
        _sut = new ReassignStoreSalesPersonCommandHandler(
            _mockStoreRepository.Object,
            _mockSalesPersonRepository.Object,
            _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReassignStoreSalesPersonCommandHandler(
                    null!,
                    _mockSalesPersonRepository.Object,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("storeRepository");

            _ = ((Action)(() => _sut = new ReassignStoreSalesPersonCommandHandler(
                    _mockStoreRepository.Object,
                    null!,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("salesPersonRepository");

            _ = ((Action)(() => _sut = new ReassignStoreSalesPersonCommandHandler(
                    _mockStoreRepository.Object,
                    _mockSalesPersonRepository.Object,
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
    public async Task Handle_throws_ArgumentNullException_when_request_Model_is_nullAsync()
    {
        var command = new ReassignStoreSalesPersonCommand
        {
            StoreId = 2534,
            Model = null!,
            AssignedDate = DefaultAuditDate
        };

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request.Model");
    }

    [Fact]
    public async Task Handle_throws_ValidationException_with_Rule01_when_SalesPersonId_is_invalidAsync()
    {
        var realValidator = new ReassignStoreSalesPersonValidator();
        var sutWithRealValidator = new ReassignStoreSalesPersonCommandHandler(
            _mockStoreRepository.Object,
            _mockSalesPersonRepository.Object,
            realValidator);

        var command = new ReassignStoreSalesPersonCommand
        {
            StoreId = 2534,
            Model = new StoreSalesPersonAssignmentCreateModel { SalesPersonId = 0 },
            AssignedDate = DefaultAuditDate
        };

        var act = async () => await sutWithRealValidator.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Should().Contain(e => e.ErrorCode == "Rule-01");
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_store_does_not_existAsync()
    {
        var command = new ReassignStoreSalesPersonCommand
        {
            StoreId = 9999,
            Model = new StoreSalesPersonAssignmentCreateModel { SalesPersonId = 1 },
            AssignedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreSalesPersonAssignmentCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockStoreRepository.Setup(x => x.GetStoreByIdAsync(9999, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreEntity?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_throws_ValidationException_with_Rule02_when_sales_person_already_assignedAsync()
    {
        const int storeId = 2534;
        const int salesPersonId = 42;

        var command = new ReassignStoreSalesPersonCommand
        {
            StoreId = storeId,
            Model = new StoreSalesPersonAssignmentCreateModel { SalesPersonId = salesPersonId },
            AssignedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreSalesPersonAssignmentCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockStoreRepository.Setup(x => x.GetStoreByIdAsync(storeId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreEntity { BusinessEntityId = storeId, Name = "Test Store", SalesPersonId = salesPersonId });

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Should().Contain(e => e.ErrorCode == "Rule-02");
    }

    [Fact]
    public async Task Handle_throws_ValidationException_with_Rule02_when_TOCTOU_same_person_caught_from_repositoryAsync()
    {
        const int storeId = 2534;
        const int newSalesPersonId = 99;

        var command = new ReassignStoreSalesPersonCommand
        {
            StoreId = storeId,
            Model = new StoreSalesPersonAssignmentCreateModel { SalesPersonId = newSalesPersonId },
            AssignedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreSalesPersonAssignmentCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockStoreRepository.Setup(x => x.GetStoreByIdAsync(storeId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreEntity { BusinessEntityId = storeId, Name = "Test Store", SalesPersonId = 1 });
        _mockSalesPersonRepository.Setup(x => x.ExistsAsync(newSalesPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockStoreRepository.Setup(x => x.ReassignSalesPersonAsync(storeId, newSalesPersonId, DefaultAuditDate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(SalesConstants.SameSalesPersonSentinel));

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Should().Contain(e => e.ErrorCode == "Rule-02");
    }

    [Fact]
    public async Task Handle_throws_ValidationException_with_Rule03_when_sales_person_does_not_existAsync()
    {
        const int storeId = 2534;
        const int newSalesPersonId = 99;

        var command = new ReassignStoreSalesPersonCommand
        {
            StoreId = storeId,
            Model = new StoreSalesPersonAssignmentCreateModel { SalesPersonId = newSalesPersonId },
            AssignedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreSalesPersonAssignmentCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockStoreRepository.Setup(x => x.GetStoreByIdAsync(storeId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreEntity { BusinessEntityId = storeId, Name = "Test Store", SalesPersonId = 1 });
        _mockSalesPersonRepository.Setup(x => x.ExistsAsync(newSalesPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Should().Contain(e => e.ErrorCode == "Rule-03");
    }

    [Fact]
    public async Task Handle_reassigns_and_returns_storeId_on_success_Async()
    {
        const int storeId = 2534;
        const int newSalesPersonId = 7;
        var assignedDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var command = new ReassignStoreSalesPersonCommand
        {
            StoreId = storeId,
            Model = new StoreSalesPersonAssignmentCreateModel { SalesPersonId = newSalesPersonId },
            AssignedDate = assignedDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreSalesPersonAssignmentCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockStoreRepository.Setup(x => x.GetStoreByIdAsync(storeId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreEntity { BusinessEntityId = storeId, Name = "Test Store", SalesPersonId = 1 });
        _mockSalesPersonRepository.Setup(x => x.ExistsAsync(newSalesPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockStoreRepository.Setup(x => x.ReassignSalesPersonAsync(storeId, newSalesPersonId, assignedDate, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(storeId);

        _mockStoreRepository.Verify(x => x.ReassignSalesPersonAsync(
            storeId, newSalesPersonId, assignedDate, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
