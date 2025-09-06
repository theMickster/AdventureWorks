using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

public sealed class ReadProductModelListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductModelRepository> _mockRepository = new();
    private ReadProductModelListQueryHandler _sut;

    public ReadProductModelListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ProductModelToLookupModelsProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadProductModelListQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadProductModelListQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadProductModelListQueryHandler(
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
    public async Task Handle_returns_empty_list_when_no_records()
    {
        _mockRepository.Setup(x => x.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<ProductModel>)null!);

        var result = await _sut.Handle(new ReadProductModelListQuery(), CancellationToken.None);
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductModel>());

        result = await _sut.Handle(new ReadProductModelListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_returns_correctly_mapped_list()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);

        _mockRepository.Setup(x => x.ListAllAsync(cancellationToken))
            .ReturnsAsync(new List<ProductModel>
            {
                new() { ProductModelId = 1, Name = "Classic Vest", Rowguid = Guid.NewGuid(), ModifiedDate = modifiedDate },
                new() { ProductModelId = 2, Name = "Long-Sleeve Logo Jersey", Rowguid = Guid.NewGuid(), ModifiedDate = modifiedDate }
            });

        var result = await _sut.Handle(new ReadProductModelListQuery(), cancellationToken);

        using (new AssertionScope())
        {
            result.Count.Should().Be(2);
            result[0].ProductModelId.Should().Be(1);
            result[0].Name.Should().Be("Classic Vest");
            result[1].ProductModelId.Should().Be(2);
        }

        _mockRepository.Verify(x => x.ListAllAsync(cancellationToken), Times.Once);
    }
}
