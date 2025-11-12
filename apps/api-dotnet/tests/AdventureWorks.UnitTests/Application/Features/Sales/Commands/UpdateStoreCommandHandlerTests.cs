using AdventureWorks.Application.Features.Dashboard.Notifications;
using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;
using MediatR;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

public sealed class UpdateStoreCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private readonly Mock<IValidator<StoreUpdateModel>> _mockValidator = new();
    private readonly Mock<IPublisher> _mockPublisher = new();
    private UpdateStoreCommandHandler _sut;

    public UpdateStoreCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(StoreUpdateModelToStoreEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();

        _sut = new UpdateStoreCommandHandler(_mapper, _mockStoreRepository.Object, _mockValidator.Object, _mockPublisher.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public void Handle_throws_correct_exception()
    {
        ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None))).Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_correct_validation_errorsAsync()
    {
        var command = new UpdateStoreCommand
        {
            Model = new StoreUpdateModel
            {
                Name = "       ",
                SalesPersonId = 123
            },
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<StoreUpdateModel>("Name", "Store name is required");

        _sut = new UpdateStoreCommandHandler(_mapper, _mockStoreRepository.Object, validator, _mockPublisher.Object);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Count(x => x.ErrorMessage == "Store name is required").Should().Be(1);
    }

    [Fact]
    public async Task Handle_returns_successAsync()
    {
        var command = new UpdateStoreCommand
        {
            Model = new StoreUpdateModel
            {
                Id = 5678,
                Name = "Updated Store Name",
                SalesPersonId = 99
            },
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult { Errors = [] });

        _mockStoreRepository.Setup(x => x.GetByIdAsync(5678, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreEntity
            {
                BusinessEntityId = 5678,
                Name = "Old Store Name",
                SalesPersonId = 1,
                ModifiedDate = DateTime.MinValue
            });

        _mockStoreRepository.Setup(x => x.UpdateAsync(It.IsAny<StoreEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            _mockStoreRepository.Verify(x => x.GetByIdAsync(5678, It.IsAny<CancellationToken>()), Times.Once);
            _mockStoreRepository.Verify(x => x.UpdateAsync(It.IsAny<StoreEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Fact]
    public async Task Handle_publishes_notification_on_successAsync()
    {
        var command = new UpdateStoreCommand
        {
            Model = new StoreUpdateModel
            {
                Id = 5678,
                Name = "Updated Store Name",
                SalesPersonId = 99
            },
            ModifiedDate = DefaultAuditDate,
            UserName = "test@example.com"
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult { Errors = [] });

        _mockStoreRepository.Setup(x => x.GetByIdAsync(5678, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreEntity
            {
                BusinessEntityId = 5678,
                Name = "Old Store Name",
                SalesPersonId = 1,
                ModifiedDate = DateTime.MinValue
            });

        _mockStoreRepository.Setup(x => x.UpdateAsync(It.IsAny<StoreEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        _mockPublisher.Verify(
            x => x.Publish(It.Is<EntityChangedNotification>(n =>
                n.EntityType == "Store" &&
                n.EntityId == 5678 &&
                n.Action == "Updated" &&
                n.UserName == "test@example.com"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_throws_and_does_not_publish_when_store_not_foundAsync()
    {
        // Arrange
        _mockStoreRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreEntity?)null);

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult { Errors = [] });

        var command = new UpdateStoreCommand
        {
            Model = new StoreUpdateModel { Id = 9999, Name = "Test Store", SalesPersonId = 1 },
            ModifiedDate = DefaultAuditDate,
            UserName = "test-user"
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.Handle(command, CancellationToken.None));

        _mockPublisher.Verify(
            p => p.Publish(It.IsAny<EntityChangedNotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_does_not_publish_on_validation_failureAsync()
    {
        var command = new UpdateStoreCommand
        {
            Model = new StoreUpdateModel { Name = "       ", SalesPersonId = 123 },
            ModifiedDate = DefaultAuditDate,
            UserName = "test@example.com"
        };

        var validator = new FakeFailureValidator<StoreUpdateModel>("Name", "Store name is required");
        _sut = new UpdateStoreCommandHandler(_mapper, _mockStoreRepository.Object, validator, _mockPublisher.Object);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _sut.Handle(command, CancellationToken.None));

        _mockPublisher.Verify(
            x => x.Publish(It.IsAny<EntityChangedNotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
