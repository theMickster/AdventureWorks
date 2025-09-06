using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

public sealed class ReadProductModelQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductModelRepository> _mockRepository = new();
    private ReadProductModelQueryHandler _sut;

    public ReadProductModelQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ProductModelToLookupModelsProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadProductModelQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadProductModelQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadProductModelQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("productModelRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_when_request_is_null()
    {
        await ((Func<Task>)(() => _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_returns_null_when_entity_not_found()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductModel)null!);

        var result = await _sut.Handle(new ReadProductModelQuery { Id = 9999 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_correctly_mapped_model()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);

        _mockRepository.Setup(x => x.GetByIdAsync(1, cancellationToken))
            .ReturnsAsync(new ProductModel
            {
                ProductModelId = 1,
                Name = "Classic Vest",
                CatalogDescription = "<catalog>vest</catalog>",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = modifiedDate
            });

        var result = await _sut.Handle(new ReadProductModelQuery { Id = 1 }, cancellationToken);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeOfType<ProductModelDetailModel>();
            result!.ProductModelId.Should().Be(1);
            result.Name.Should().Be("Classic Vest");
            result.CatalogDescription.Should().Be("<catalog>vest</catalog>");
            result.ModifiedDate.Should().Be(modifiedDate);
        }

        _mockRepository.Verify(x => x.GetByIdAsync(1, cancellationToken), Times.Once);
    }
}
