using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Purchasing;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadShipMethodQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IShipMethodRepository> _mockRepository = new();
    private ReadShipMethodQueryHandler _sut;

    public ReadShipMethodQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ShipMethodEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadShipMethodQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadShipMethodQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadShipMethodQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("shipMethodRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_null_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShipMethod)null!);

        var result = await _sut.Handle(new ReadShipMethodQuery { Id = 999 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_correctly_Async()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);

        _mockRepository.Setup(x => x.GetByIdAsync(1, cancellationToken))
            .ReturnsAsync(new ShipMethod
            {
                ShipMethodId = 1,
                Name = "CARGO TRANSPORT 5",
                ShipBase = 3.95m,
                ShipRate = 1.25m,
                ModifiedDate = modifiedDate
            });

        var result = await _sut.Handle(new ReadShipMethodQuery { Id = 1 }, cancellationToken);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ShipMethodId.Should().Be(1);
            result.Name.Should().Be("CARGO TRANSPORT 5");
            result.ShipBase.Should().Be(3.95m);
            result.ShipRate.Should().Be(1.25m);
            result.ModifiedDate.Should().Be(modifiedDate);
        }

        _mockRepository.Verify(x => x.GetByIdAsync(1, cancellationToken), Times.Once);
    }
}
