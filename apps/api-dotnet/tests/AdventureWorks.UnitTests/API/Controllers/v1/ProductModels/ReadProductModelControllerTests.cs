using AdventureWorks.API.Controllers.v1.ProductModels;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.ProductModels;

[ExcludeFromCodeCoverage]
public sealed class ReadProductModelControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadProductModelController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadProductModelController _sut;

    public ReadProductModelControllerTests()
    {
        _sut = new ReadProductModelController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_when_logger_is_null()
    {
        _ = ((Action)(() => _ = new ReadProductModelController(null!, _mockMediator.Object)))
            .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
            .And.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_throws_when_mediator_is_null()
    {
        _ = ((Action)(() => _ = new ReadProductModelController(_mockLogger.Object, null!)))
            .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
            .And.ParamName.Should().Be("mediator");
    }

    [Fact]
    public void Controller_has_authorize_attribute()
    {
        typeof(ReadProductModelController)
            .IsDefined(typeof(AuthorizeAttribute), true)
            .Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_returns_bad_request_when_id_is_zero()
    {
        var result = await _sut.GetByIdAsync(0);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            outputModel.Should().Be("A valid product model id must be specified.");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_bad_request_when_id_is_negative()
    {
        var result = await _sut.GetByIdAsync(-1);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            outputModel.Should().Be("A valid product model id must be specified.");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_not_found_when_model_is_null()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductModelQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductModelDetailModel)null!);

        var result = await _sut.GetByIdAsync(999);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate the product model.");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_ok_with_model()
    {
        _mockMediator.Setup(
                x => x.Send(It.Is<ReadProductModelQuery>(q => q.Id == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductModelDetailModel
            {
                ProductModelId = 1,
                Name = "Classic Vest",
                CatalogDescription = null,
                ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc)
            });

        var result = await _sut.GetByIdAsync(1);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<ProductModelDetailModel>();
        }
    }

    [Fact]
    public async Task GetListAsync_returns_not_found_when_list_is_empty()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadProductModelListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductModelListModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate records in the product model list.");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_ok_with_models()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadProductModelListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductModelListModel>
            {
                new() { ProductModelId = 1, Name = "Classic Vest", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
                new() { ProductModelId = 2, Name = "Long-Sleeve Logo Jersey", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) }
            });

        var result = await _sut.GetListAsync();
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }
}
