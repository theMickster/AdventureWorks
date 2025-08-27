using AdventureWorks.API.Controllers.v1.Stores;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Stores;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreCustomersControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadStoreCustomersController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadStoreCustomersController _sut;

    public ReadStoreCustomersControllerTests()
    {
        _sut = new ReadStoreCustomersController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadStoreCustomersController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadStoreCustomersController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task GetAsync_invalid_storeId_returns_bad_request(int storeId)
    {
        var result = await _sut.GetAsync(storeId, new StoreCustomerParameter());
        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("A valid store id must be specified.");
        }
    }

    [Fact]
    public async Task GetAsync_returns_not_found_when_query_returns_nullAsync()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreCustomerListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreCustomerSearchResultModel?)null);

        var result = await _sut.GetAsync(2534, new StoreCustomerParameter());

        var notFound = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            notFound.Should().NotBeNull();
            notFound!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            notFound!.Value!.ToString().Should().Be("Unable to locate store customers.");
        }
    }

    [Fact]
    public async Task GetAsync_returns_ok_with_modelAsync()
    {
        var output = new StoreCustomerSearchResultModel
        {
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 1,
            Results = new List<StoreCustomerModel>
            {
                new()
                {
                    CustomerId = 11000,
                    AccountNumber = "AW00011000",
                    PersonName = "Jon Yang",
                    LifetimeSpend = 8_249.00m,
                    OrderCount = 3,
                    LastOrderDate = new DateTime(2025, 11, 15, 0, 0, 0, DateTimeKind.Utc)
                }
            }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreCustomerListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(output);

        var result = await _sut.GetAsync(2534, new StoreCustomerParameter());

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value.Should().Be(output);
        }
    }

    [Fact]
    public async Task GetAsync_sends_correct_storeId_and_parameters_to_mediatorAsync()
    {
        const int storeId = 2534;
        var parameters = new StoreCustomerParameter { PageNumber = 2, PageSize = 25 };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreCustomerListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreCustomerSearchResultModel?)null);

        await _sut.GetAsync(storeId, parameters);

        _mockMediator.Verify(x => x.Send(
            It.Is<ReadStoreCustomerListQuery>(q => q.StoreId == storeId && q.Parameters == parameters),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_forwards_cancellation_token_to_mediatorAsync()
    {
        const int storeId = 2534;
        var parameters = new StoreCustomerParameter();

        using var cts = new CancellationTokenSource();

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreCustomerListQuery>(), cts.Token))
            .ReturnsAsync((StoreCustomerSearchResultModel?)null);

        await _sut.GetAsync(storeId, parameters, cts.Token);

        _mockMediator.Verify(x => x.Send(
            It.Is<ReadStoreCustomerListQuery>(q => q.StoreId == storeId),
            cts.Token), Times.Once);
    }
}
