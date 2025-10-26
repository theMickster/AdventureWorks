using AdventureWorks.API.Controllers.v1.SalesPersons;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.SalesPersons;

[ExcludeFromCodeCoverage]
public sealed class ReadSalesPersonPerformanceControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadSalesPersonController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadSalesPersonController _sut;

    public ReadSalesPersonPerformanceControllerTests()
    {
        _sut = new ReadSalesPersonController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task GetSalesPersonPerformanceAsync_returns_ok_Async()
    {
        const int salesPersonId = 275;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadSalesPersonPerformanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesPersonPerformanceModel
            {
                SalesYtd = 1750000m,
                SalesLastYear = 1500000m,
                OrderCount = 450,
                TotalRevenue = 10475367.08m
            });

        var result = await _sut.GetSalesPersonPerformanceAsync(salesPersonId, CancellationToken.None);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<SalesPersonPerformanceModel>();
        }
    }

    [Fact]
    public async Task GetSalesPersonPerformanceAsync_returns_not_found_Async()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadSalesPersonPerformanceQuery>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((SalesPersonPerformanceModel?)null);

        var result = await _sut.GetSalesPersonPerformanceAsync(999999, CancellationToken.None);
        var objectResult = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            (objectResult.Value as string).Should().Be("Unable to locate the sales person.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetSalesPersonPerformanceAsync_returns_bad_request_Async(int salesPersonId)
    {
        var result = await _sut.GetSalesPersonPerformanceAsync(salesPersonId, CancellationToken.None);
        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            (objectResult.Value as string).Should().Be("A valid sales person id must be specified.");
        }
    }

    [Fact]
    public async Task GetSalesPersonPerformanceAsync_sends_correct_query_Async()
    {
        const int salesPersonId = 275;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadSalesPersonPerformanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesPersonPerformanceModel());

        await _sut.GetSalesPersonPerformanceAsync(salesPersonId, CancellationToken.None);

        _mockMediator.Verify(
            x => x.Send(
                It.Is<ReadSalesPersonPerformanceQuery>(q => q.Id == salesPersonId),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "because the controller should send the query with the correct id");
    }
}
