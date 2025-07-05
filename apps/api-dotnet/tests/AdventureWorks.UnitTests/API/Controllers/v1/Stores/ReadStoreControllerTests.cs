using AdventureWorks.API.Controllers.v1.Stores;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.Test.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Stores;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadStoreController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadStoreController _sut;

    public ReadStoreControllerTests()
    {
        _sut = new ReadStoreController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadStoreController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadStoreController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        const int id = 7;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreModel { Id = id, Name = "test" });

        var result = await _sut.GetByIdAsync(7);

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
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreQuery>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((StoreModel?)null);

        var result = await _sut.GetByIdAsync(7);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate the store.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task GetById_returns_bad_request_Async(int addressId)
    {
        var result = await _sut.GetByIdAsync(addressId);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("A valid store id must be specified.");
        }
    }

    [Fact]
    public async Task GetStoreListAsync_returns_ok_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreSearchResultModel { Results = [new(){Id = 1, Name = "test" }] });

        var result = await _sut.GetStoreListAsync(new StoreParameter());
        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();

            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetStoreListAsync_null_results_bad_request_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreSearchResultModel { Results = null! });

        var result = await _sut.GetStoreListAsync(new StoreParameter());
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
    public async Task GetStoreListAsync_empty_results_bad_request_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreSearchResultModel { Results = new List<StoreModel>() });

        var result = await _sut.GetStoreListAsync(new StoreParameter());
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
    public async Task SearchStoresAsync_returns_ok_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreSearchResultModel { Results = [new() { Id = 1, Name = "test" }] });

        var result = await _sut.SearchStoresAsync(new StoreParameter(), new StoreSearchModel());
        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();

            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task SearchStoresAsync_null_results_bad_request_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreListQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new StoreSearchResultModel { Results = null! });

        var result = await _sut.SearchStoresAsync(new StoreParameter(), new StoreSearchModel());
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
    public async Task SearchStoresAsync_empty_results_bad_request_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreSearchResultModel { Results = new List<StoreModel>() });

        var result = await _sut.SearchStoresAsync(new StoreParameter(), new StoreSearchModel());
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
