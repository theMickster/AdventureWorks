using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

public sealed class UpdateStoreCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private readonly Mock<IValidator<StoreUpdateModel>> _mockValidator = new();
    private UpdateStoreCommandHandler _sut;

    public UpdateStoreCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(StoreUpdateModelToStoreEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();

        _sut = new UpdateStoreCommandHandler(_mapper, _mockStoreRepository.Object, _mockValidator.Object);
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

        _sut = new UpdateStoreCommandHandler(_mapper, _mockStoreRepository.Object, validator);

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

        _mockStoreRepository.Setup(x => x.GetByIdAsync(5678))
            .ReturnsAsync(new StoreEntity
            {
                BusinessEntityId = 5678,
                Name = "Old Store Name",
                SalesPersonId = 1,
                ModifiedDate = DateTime.MinValue
            });

        _mockStoreRepository.Setup(x => x.UpdateAsync(It.IsAny<StoreEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            _mockStoreRepository.Verify(x => x.GetByIdAsync(5678), Times.Once);
            _mockStoreRepository.Verify(x => x.UpdateAsync(It.IsAny<StoreEntity>()), Times.Once);
        }
    }
}
