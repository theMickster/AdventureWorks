using AdventureWorks.API.Controllers.v1.Customers;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Customers;

[ExcludeFromCodeCoverage]
public sealed class ReadCustomersControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadCustomersController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadCustomersController _sut;

    public ReadCustomersControllerTests()
    {
        _sut = new ReadCustomersController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadCustomersController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadCustomersController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetAsync_returns_ok_with_modelAsync()
    {
        var output = new CustomerSearchResultModel
        {
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 1,
            Results = new List<CustomerListItemModel>
            {
                new()
                {
                    CustomerId = 11000,
                    DisplayName = "Jon Yang",
                    CustomerType = "Individual",
                    LtvRank = 1,
                    TotalSpend = 8_249.00m,
                    OrderCount = 3,
                    IsInactive = false
                }
            }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadCustomerListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(output);

        var result = await _sut.GetAsync(new CustomerParameter());

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value.Should().Be(output);
        }
    }

    [Fact]
    public async Task GetAsync_sends_correct_parameters_to_mediatorAsync()
    {
        var parameters = new CustomerParameter { PageNumber = 2, PageSize = 25, Search = "yang" };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadCustomerListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerSearchResultModel());

        await _sut.GetAsync(parameters);

        _mockMediator.Verify(x => x.Send(
            It.Is<ReadCustomerListQuery>(q => q.Parameters == parameters),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_forwards_cancellation_token_to_mediatorAsync()
    {
        var parameters = new CustomerParameter();

        using var cts = new CancellationTokenSource();

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadCustomerListQuery>(), cts.Token))
            .ReturnsAsync(new CustomerSearchResultModel());

        await _sut.GetAsync(parameters, cts.Token);

        _mockMediator.Verify(x => x.Send(
            It.Is<ReadCustomerListQuery>(q => q.Parameters == parameters),
            cts.Token), Times.Once);
    }
}
