using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.Test.Common.Extensions;
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

    }

    [Fact]
    public async Task Handle_returns_successAsync()
    {

    }
}
