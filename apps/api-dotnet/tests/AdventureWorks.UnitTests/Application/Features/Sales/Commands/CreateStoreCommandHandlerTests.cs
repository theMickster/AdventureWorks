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

public sealed class CreateStoreCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private readonly Mock<IValidator<StoreCreateModel>> _mockValidator = new();
    private readonly Mock<IPublisher> _mockPublisher = new();
    private CreateStoreCommandHandler _sut;

    public CreateStoreCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(StoreCreateModelToStoreEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new CreateStoreCommandHandler(_mapper, _mockStoreRepository.Object, _mockValidator.Object, _mockPublisher.Object);
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
    public async Task Handle_publishes_notification_on_successAsync()
    {
        var command = new CreateStoreCommand
        {
            Model = new StoreCreateModel { Name = "Contoso Store", SalesPersonId = 42 },
            ModifiedDate = DefaultAuditDate,
            RowGuid = new Guid("5ec92f1e-232b-430e-a729-ea59c943e3fc"),
            UserName = "test@example.com"
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult { Errors = [] });

        _mockStoreRepository.Setup(x => x.AddAsync(It.IsAny<StoreEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreEntity
            {
                BusinessEntityId = 1234,
                Name = "Contoso Store",
                SalesPersonId = 42,
                ModifiedDate = DefaultAuditDate,
                Rowguid = command.RowGuid
            });

        await _sut.Handle(command, CancellationToken.None);

        _mockPublisher.Verify(
            x => x.Publish(It.Is<EntityChangedNotification>(n =>
                n.EntityType == "Store" &&
                n.EntityId == 1234 &&
                n.Action == "Created" &&
                n.UserName == "test@example.com"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_does_not_publish_on_validation_failureAsync()
    {
        var command = new CreateStoreCommand
        {
            Model = new StoreCreateModel { Name = "       ", SalesPersonId = 42 },
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid(),
            UserName = "test@example.com"
        };

        var validator = new FakeFailureValidator<StoreCreateModel>("Name", "Store name is required");
        _sut = new CreateStoreCommandHandler(_mapper, _mockStoreRepository.Object, validator, _mockPublisher.Object);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _sut.Handle(command, CancellationToken.None));

        _mockPublisher.Verify(
            x => x.Publish(It.IsAny<EntityChangedNotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_throws_correct_validation_errorsAsync()
    {
        var command = new CreateStoreCommand
        {
            Model = new StoreCreateModel
            {
                Name = "       ",
                SalesPersonId = 123
            },
            ModifiedDate = DefaultAuditDate,
            RowGuid = new Guid("5ec92f1e-232b-430e-a729-ea59c943e3fc")
        };

        var validator = new FakeFailureValidator<StoreCreateModel>("Name", "Store name is required");

        _sut = new CreateStoreCommandHandler(_mapper, _mockStoreRepository.Object, validator, _mockPublisher.Object);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Count(x => x.ErrorMessage == "Store name is required").Should().Be(1);
    }

    [Fact]
    public async Task Handle_returns_successAsync()
    {
        var command = new CreateStoreCommand
        {
            Model = new StoreCreateModel
            {
                Name = "Contoso Store",
                SalesPersonId = 42
            },
            ModifiedDate = DefaultAuditDate,
            RowGuid = new Guid("5ec92f1e-232b-430e-a729-ea59c943e3fc")
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult { Errors = [] });

        _mockStoreRepository.Setup(x => x.AddAsync(It.IsAny<StoreEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreEntity
            {
                BusinessEntityId = 1234,
                Name = "Contoso Store",
                SalesPersonId = 42,
                ModifiedDate = DefaultAuditDate,
                Rowguid = command.RowGuid
            });

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(1234);
    }
}
