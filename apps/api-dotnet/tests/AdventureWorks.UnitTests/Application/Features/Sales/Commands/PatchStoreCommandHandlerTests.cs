using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

[ExcludeFromCodeCoverage]
public sealed class PatchStoreCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private readonly Mock<IValidator<StoreUpdateModel>> _mockValidator = new();
    private PatchStoreCommandHandler _sut;

    public PatchStoreCommandHandlerTests()
    {
        _sut = new PatchStoreCommandHandler(_mockStoreRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new PatchStoreCommandHandler(
                    null!,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("storeRepository");

            _ = ((Action)(() => _sut = new PatchStoreCommandHandler(
                    _mockStoreRepository.Object,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("validator");
        }
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_request_is_nullAsync()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_PatchDocument_is_nullAsync()
    {
        var command = new PatchStoreCommand
        {
            StoreId = 2534,
            PatchDocument = null!,
            ModifiedDate = DefaultAuditDate
        };

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_store_not_foundAsync()
    {
        var patchDoc = new JsonPatchDocument<StoreUpdateModel>();
        patchDoc.Replace(x => x.Name, "New Name");

        var command = new PatchStoreCommand
        {
            StoreId = 9999,
            PatchDocument = patchDoc,
            ModifiedDate = DefaultAuditDate
        };

        _mockStoreRepository.Setup(x => x.GetByIdAsync(9999))
            .ReturnsAsync((StoreEntity?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_applies_patch_and_updates_Name_successfullyAsync()
    {
        const int storeId = 2534;
        const string newName = "Patched Store Name";

        var patchDoc = new JsonPatchDocument<StoreUpdateModel>();
        patchDoc.Replace(x => x.Name, newName);

        var command = new PatchStoreCommand
        {
            StoreId = storeId,
            PatchDocument = patchDoc,
            ModifiedDate = DefaultAuditDate
        };

        var entity = new StoreEntity
        {
            BusinessEntityId = storeId,
            Name = "Old Store Name",
            SalesPersonId = 99,
            ModifiedDate = DateTime.MinValue
        };

        _mockStoreRepository.Setup(x => x.GetByIdAsync(storeId))
            .ReturnsAsync(entity);

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<StoreUpdateModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockStoreRepository.Setup(x => x.UpdateAsync(It.IsAny<StoreEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            _mockStoreRepository.Verify(x => x.UpdateAsync(It.Is<StoreEntity>(e =>
                e.Name == newName &&
                e.BusinessEntityId == storeId &&
                e.ModifiedDate == DefaultAuditDate)), Times.Once);
        }
    }

    [Fact]
    public async Task Handle_ensures_StoreId_immutability_when_Id_is_patchedAsync()
    {
        const int storeId = 2534;

        var patchDoc = new JsonPatchDocument<StoreUpdateModel>();
        patchDoc.Replace(x => x.Id, 9999);

        var command = new PatchStoreCommand
        {
            StoreId = storeId,
            PatchDocument = patchDoc,
            ModifiedDate = DefaultAuditDate
        };

        var entity = new StoreEntity
        {
            BusinessEntityId = storeId,
            Name = "Test Store",
            SalesPersonId = 99,
            ModifiedDate = DateTime.MinValue
        };

        _mockStoreRepository.Setup(x => x.GetByIdAsync(storeId))
            .ReturnsAsync(entity);

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<StoreUpdateModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockStoreRepository.Setup(x => x.UpdateAsync(It.IsAny<StoreEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        _mockStoreRepository.Verify(x => x.UpdateAsync(It.Is<StoreEntity>(e =>
            e.BusinessEntityId == storeId)), Times.Once);
    }

    [Fact]
    public async Task Handle_throws_ValidationException_when_patched_model_has_empty_nameAsync()
    {
        const int storeId = 2534;

        var patchDoc = new JsonPatchDocument<StoreUpdateModel>();
        patchDoc.Replace(x => x.Name, "");

        var command = new PatchStoreCommand
        {
            StoreId = storeId,
            PatchDocument = patchDoc,
            ModifiedDate = DefaultAuditDate
        };

        var entity = new StoreEntity
        {
            BusinessEntityId = storeId,
            Name = "Valid Store Name",
            SalesPersonId = 99,
            ModifiedDate = DateTime.MinValue
        };

        _mockStoreRepository.Setup(x => x.GetByIdAsync(storeId))
            .ReturnsAsync(entity);

        var realValidator = new UpdateStoreValidator();
        var sut = new PatchStoreCommandHandler(_mockStoreRepository.Object, realValidator);

        var act = async () => await sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Should().Contain(e => e.ErrorMessage == StoreBaseModelValidator<StoreUpdateModel>.MessageStoreNameEmpty);
    }

    [Fact]
    public async Task Handle_returns_Unit_value_on_successAsync()
    {
        const int storeId = 2534;

        var patchDoc = new JsonPatchDocument<StoreUpdateModel>();
        patchDoc.Replace(x => x.Name, "Updated Name");

        var command = new PatchStoreCommand
        {
            StoreId = storeId,
            PatchDocument = patchDoc,
            ModifiedDate = DefaultAuditDate
        };

        var entity = new StoreEntity
        {
            BusinessEntityId = storeId,
            Name = "Old Name",
            SalesPersonId = 99,
            ModifiedDate = DateTime.MinValue
        };

        _mockStoreRepository.Setup(x => x.GetByIdAsync(storeId))
            .ReturnsAsync(entity);

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<StoreUpdateModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockStoreRepository.Setup(x => x.UpdateAsync(It.IsAny<StoreEntity>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(MediatR.Unit.Value);
    }

    [Fact]
    public async Task Handle_passes_correct_ModifiedDate_to_entityAsync()
    {
        const int storeId = 2534;

        var patchDoc = new JsonPatchDocument<StoreUpdateModel>();
        patchDoc.Replace(x => x.Name, "New Name");

        var command = new PatchStoreCommand
        {
            StoreId = storeId,
            PatchDocument = patchDoc,
            ModifiedDate = DefaultAuditDate
        };

        var entity = new StoreEntity
        {
            BusinessEntityId = storeId,
            Name = "Old Name",
            SalesPersonId = 99,
            ModifiedDate = DateTime.MinValue
        };

        _mockStoreRepository.Setup(x => x.GetByIdAsync(storeId))
            .ReturnsAsync(entity);

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<StoreUpdateModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockStoreRepository.Setup(x => x.UpdateAsync(It.IsAny<StoreEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        _mockStoreRepository.Verify(x => x.UpdateAsync(It.Is<StoreEntity>(e =>
            e.ModifiedDate == DefaultAuditDate)), Times.Once);
    }
}
