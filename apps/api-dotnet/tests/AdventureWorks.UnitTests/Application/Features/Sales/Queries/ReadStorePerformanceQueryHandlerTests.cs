using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadStorePerformanceQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _mapperConfiguration;
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private ReadStorePerformanceQueryHandler _sut;

    public ReadStorePerformanceQueryHandlerTests()
    {
        _mapperConfiguration = new MapperConfiguration(config =>
            config.AddMaps(typeof(StorePerformanceProjectionToStorePerformanceModelProfile).Assembly)
        );
        _mapper = _mapperConfiguration.CreateMapper();

        _sut = new ReadStorePerformanceQueryHandler(_mapper, _mockStoreRepository.Object);
    }

    [Fact]
    public void AutoMapper_configuration_is_valid()
    {
        var act = () => _mapperConfiguration.AssertConfigurationIsValid();

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStorePerformanceQueryHandler(null!, _mockStoreRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStorePerformanceQueryHandler(_mapper, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("storeRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_when_request_is_nullAsync()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_returns_null_when_store_not_foundAsync()
    {
        _mockStoreRepository
            .Setup(x => x.GetPerformanceAsync(2534, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StorePerformanceProjection?)null);

        var result = await _sut.Handle(new ReadStorePerformanceQuery { StoreId = 2534 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_populated_model_with_computed_average_order_valueAsync()
    {
        var year = DateTime.UtcNow.Year;

        _mockStoreRepository
            .Setup(x => x.GetPerformanceAsync(2534, year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StorePerformanceProjection
            {
                BusinessEntityId = 2534,
                Name = "Bike World",
                RevenueYtd = 25_000m,
                OrderCount = 5,
                CustomerCount = 12,
                Year = year
            });

        var result = await _sut.Handle(new ReadStorePerformanceQuery { StoreId = 2534 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.StoreId.Should().Be(2534);
            result.StoreName.Should().Be("Bike World");
            result.RevenueYtd.Should().Be(25_000m);
            result.OrderCount.Should().Be(5);
            result.CustomerCount.Should().Be(12);
            result.Year.Should().Be(year);
            result.AverageOrderValue.Should().Be(5_000m);
        }
    }

    [Fact]
    public async Task Handle_returns_zero_average_order_value_when_order_count_is_zeroAsync()
    {
        var year = DateTime.UtcNow.Year;

        _mockStoreRepository
            .Setup(x => x.GetPerformanceAsync(2534, year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StorePerformanceProjection
            {
                BusinessEntityId = 2534,
                Name = "Quiet Store",
                RevenueYtd = 0m,
                OrderCount = 0,
                CustomerCount = 0,
                Year = year
            });

        var result = await _sut.Handle(new ReadStorePerformanceQuery { StoreId = 2534 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.StoreId.Should().Be(2534);
            result.StoreName.Should().Be("Quiet Store");
            result.RevenueYtd.Should().Be(0m);
            result.OrderCount.Should().Be(0);
            result.CustomerCount.Should().Be(0);
            result.AverageOrderValue.Should().Be(0m);
            result.Year.Should().Be(year);
        }
    }

    [Fact]
    public async Task Handle_uses_current_utc_year_when_calling_repositoryAsync()
    {
        var expectedYear = DateTime.UtcNow.Year;

        _mockStoreRepository
            .Setup(x => x.GetPerformanceAsync(2534, expectedYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StorePerformanceProjection
            {
                BusinessEntityId = 2534,
                Name = "Bike World",
                RevenueYtd = 0m,
                OrderCount = 0,
                CustomerCount = 0,
                Year = expectedYear
            });

        var result = await _sut.Handle(new ReadStorePerformanceQuery { StoreId = 2534 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Year.Should().Be(expectedYear);
            _mockStoreRepository.Verify(
                x => x.GetPerformanceAsync(2534, expectedYear, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task Handle_forwards_cancellation_token_to_repositoryAsync()
    {
        using var cts = new CancellationTokenSource();
        _mockStoreRepository
            .Setup(x => x.GetPerformanceAsync(2534, It.IsAny<int>(), cts.Token))
            .ReturnsAsync((StorePerformanceProjection?)null);

        await _sut.Handle(new ReadStorePerformanceQuery { StoreId = 2534 }, cts.Token);

        _mockStoreRepository.Verify(
            x => x.GetPerformanceAsync(2534, It.IsAny<int>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task Handle_returns_populated_model_when_store_has_customers_but_no_ordersAsync()
    {
        // AC Scenario: "Store with no orders returns zero values" — covers the sub-case where
        // the store exists AND has registered customers, but none of those customers have orders
        // in the current calendar year. CustomerCount must reflect the existing customers while
        // revenue/order/average all collapse to zero.
        var year = DateTime.UtcNow.Year;

        _mockStoreRepository
            .Setup(x => x.GetPerformanceAsync(2534, year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StorePerformanceProjection
            {
                BusinessEntityId = 2534,
                Name = "Dormant Store",
                RevenueYtd = 0m,
                OrderCount = 0,
                CustomerCount = 7,
                Year = year
            });

        var result = await _sut.Handle(new ReadStorePerformanceQuery { StoreId = 2534 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.StoreId.Should().Be(2534);
            result.StoreName.Should().Be("Dormant Store");
            result.RevenueYtd.Should().Be(0m);
            result.OrderCount.Should().Be(0);
            result.AverageOrderValue.Should().Be(0m);
            result.CustomerCount.Should().Be(7);
            result.Year.Should().Be(year);
        }
    }
}
