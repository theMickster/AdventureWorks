using AdventureWorks.API.Controllers.v1.SalesPersons;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.Test.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.SalesPersons;

[ExcludeFromCodeCoverage]
public sealed class ReadSalesPersonControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadSalesPersonController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadSalesPersonController _sut;

    public ReadSalesPersonControllerTests()
    {
        _sut = new ReadSalesPersonController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadSalesPersonController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadSalesPersonController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        const int id = 274;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesPersonQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesPersonModel { Id = id, FirstName = "Stephen", LastName = "Jiang", JobTitle = "Sales Manager" });

        var result = await _sut.GetByIdAsync(274);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetById_returns_not_found_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesPersonQuery>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((SalesPersonModel?)null);

        var result = await _sut.GetByIdAsync(999);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate the sales person.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task GetById_returns_bad_request_Async(int salesPersonId)
    {
        var result = await _sut.GetByIdAsync(salesPersonId);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("A valid sales person id must be specified.");
        }
    }

    [Fact]
    public async Task GetSalesPersonListAsync_returns_ok_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesPersonListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesPersonSearchResultModel { Results = new List<SalesPersonModel> { new() { FirstName = "Test", LastName = "User", JobTitle = "Sales Rep" } } });

        var result = await _sut.GetSalesPersonListAsync(new SalesPersonParameter());
        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetSalesPersonListAsync_null_results_bad_request_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesPersonListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesPersonSearchResultModel { Results = null! });

        var result = await _sut.GetSalesPersonListAsync(new SalesPersonParameter());
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate results based upon input query parameters.");

            _mockLogger.VerifyLoggingMessageContains("Unable to locate results based upon input query parameters", null, LogLevel.Error);
        }
    }

    [Fact]
    public async Task GetSalesPersonListAsync_empty_results_bad_request_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesPersonListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesPersonSearchResultModel { Results = new List<SalesPersonModel>() });

        var result = await _sut.GetSalesPersonListAsync(new SalesPersonParameter());
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate results based upon input query parameters.");

            _mockLogger.VerifyLoggingMessageContains("Unable to locate results based upon input query parameters", null, LogLevel.Error);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_returns_ok_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesPersonListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesPersonSearchResultModel { Results = new List<SalesPersonModel> { new() { FirstName = "Test", LastName = "User", JobTitle = "Sales Rep" } } });

        var result = await _sut.SearchSalesPersonsAsync(new SalesPersonParameter(), new SalesPersonSearchModel());
        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_null_results_bad_request_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesPersonListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesPersonSearchResultModel { Results = null! });

        var result = await _sut.SearchSalesPersonsAsync(new SalesPersonParameter(), new SalesPersonSearchModel());
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate results based upon client input parameters.");

            _mockLogger.VerifyLoggingMessageContains("Unable to locate results based upon client input parameters", null, LogLevel.Error);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_empty_results_bad_request_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesPersonListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesPersonSearchResultModel { Results = new List<SalesPersonModel>() });

        var result = await _sut.SearchSalesPersonsAsync(new SalesPersonParameter(), new SalesPersonSearchModel());
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate results based upon client input parameters.");

            _mockLogger.VerifyLoggingMessageContains("Unable to locate results based upon client input parameters", null, LogLevel.Error);
        }
    }
}
