using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;
using FluentValidation.Results;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class GetSalesOrderAnalyticsQueryHandlerTests : UnitTestBase
{
    private readonly Mock<ISalesOrderRepository> _mockSalesOrderRepository = new();
    private readonly Mock<IValidator<GetSalesOrderAnalyticsQuery>> _mockValidator = new();
    private GetSalesOrderAnalyticsQueryHandler _sut;

    public GetSalesOrderAnalyticsQueryHandlerTests()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<GetSalesOrderAnalyticsQuery>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _sut = new GetSalesOrderAnalyticsQueryHandler(_mockSalesOrderRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public async Task Handle_returns_populated_model_with_filter()
    {
        // Arrange
        var filter = new SalesOrderSearchModel
        {
            OrderDateFrom = new DateTime(2013, 1, 1),
            OrderDateTo = new DateTime(2013, 12, 31),
            TerritoryId = 2
        };
        var query = new GetSalesOrderAnalyticsQuery { Filter = filter };

        var expectedModel = new SalesOrderAnalyticsModel
        {
            TotalRevenue = 500000m,
            OrderCount = 42,
            PercentageOfTotal = 15.5m,
            MonthlyTrend =
            [
                new SalesOrderMonthlyTrendModel { Year = 2013, Month = 1, Revenue = 50000m },
                new SalesOrderMonthlyTrendModel { Year = 2013, Month = 2, Revenue = 45000m }
            ]
        };

        _mockSalesOrderRepository
            .Setup(x => x.GetSalesOrderAnalyticsAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedModel);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalRevenue.Should().Be(500000m);
        result.OrderCount.Should().Be(42);
        result.PercentageOfTotal.Should().Be(15.5m);
        result.MonthlyTrend.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_returns_populated_model_when_filter_is_null()
    {
        // Arrange
        var query = new GetSalesOrderAnalyticsQuery { Filter = null };

        var expectedModel = new SalesOrderAnalyticsModel
        {
            TotalRevenue = 1000000m,
            OrderCount = 31465,
            PercentageOfTotal = 100m,
            MonthlyTrend = [new SalesOrderMonthlyTrendModel { Year = 2011, Month = 5, Revenue = 100000m }]
        };

        _mockSalesOrderRepository
            .Setup(x => x.GetSalesOrderAnalyticsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedModel);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrderCount.Should().Be(31465);
        _mockSalesOrderRepository.Verify(x =>
            x.GetSalesOrderAnalyticsAsync(null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_returns_zeros_and_empty_trend_when_no_orders_match()
    {
        // Arrange
        var query = new GetSalesOrderAnalyticsQuery
        {
            Filter = new SalesOrderSearchModel { SalesPersonId = 99999 }
        };

        var emptyModel = new SalesOrderAnalyticsModel
        {
            TotalRevenue = 0m,
            OrderCount = 0,
            PercentageOfTotal = 0m,
            MonthlyTrend = []
        };

        _mockSalesOrderRepository
            .Setup(x => x.GetSalesOrderAnalyticsAsync(It.IsAny<SalesOrderSearchModel?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyModel);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.TotalRevenue.Should().Be(0m);
        result.OrderCount.Should().Be(0);
        result.PercentageOfTotal.Should().Be(0m);
        result.MonthlyTrend.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_throws_argument_null_exception_when_request_is_null()
    {
        // Act
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_forwards_cancellation_token_to_repository()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var query = new GetSalesOrderAnalyticsQuery { Filter = null };

        _mockSalesOrderRepository
            .Setup(x => x.GetSalesOrderAnalyticsAsync(null, token))
            .ReturnsAsync(new SalesOrderAnalyticsModel { MonthlyTrend = [] });

        // Act
        await _sut.Handle(query, token);

        // Assert
        _mockSalesOrderRepository.Verify(x =>
            x.GetSalesOrderAnalyticsAsync(null, token), Times.Once);
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_query_is_invalid()
    {
        // Arrange
        var handler = new GetSalesOrderAnalyticsQueryHandler(
            _mockSalesOrderRepository.Object,
            new FakeFailureValidator<GetSalesOrderAnalyticsQuery>("Filter.OrderDateFrom", "OrderDateFrom must be less than or equal to OrderDateTo"));

        var query = new GetSalesOrderAnalyticsQuery
        {
            Filter = new SalesOrderSearchModel
            {
                OrderDateFrom = new DateTime(2014, 6, 30),
                OrderDateTo = new DateTime(2013, 1, 1)
            }
        };

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
}
