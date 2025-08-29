using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Purchasing;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadShipMethodListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IShipMethodRepository> _mockRepository = new();
    private ReadShipMethodListQueryHandler _sut;

    public ReadShipMethodListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ShipMethodEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadShipMethodListQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadShipMethodListQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadShipMethodListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("shipMethodRepository");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<ShipMethod>)null!);

        var result = await _sut.Handle(new ReadShipMethodListQuery(), CancellationToken.None);
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShipMethod>());

        result = await _sut.Handle(new ReadShipMethodListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetListAsync_returns_valid_listAsync()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);

        _mockRepository.Setup(x => x.ListAllAsync(cancellationToken))
            .ReturnsAsync(new List<ShipMethod>
            {
                new() { ShipMethodId = 1, Name = "CARGO TRANSPORT 5", ShipBase = 3.95m, ShipRate = 1.25m, ModifiedDate = modifiedDate },
                new() { ShipMethodId = 2, Name = "ZY - EXPRESS", ShipBase = 2.95m, ShipRate = 1.50m, ModifiedDate = modifiedDate }
            });

        var result = await _sut.Handle(new ReadShipMethodListQuery(), cancellationToken);

        using (new AssertionScope())
        {
            result.Count.Should().Be(2);
            result[0].ShipMethodId.Should().Be(1);
            result[0].ModifiedDate.Should().Be(modifiedDate);
            result[1].ShipMethodId.Should().Be(2);
        }

        _mockRepository.Verify(x => x.ListAllAsync(cancellationToken), Times.Once);
    }
}
