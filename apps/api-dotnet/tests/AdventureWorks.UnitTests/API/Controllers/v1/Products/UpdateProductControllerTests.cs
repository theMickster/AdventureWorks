using AdventureWorks.API.Controllers.v1.Products;
using AdventureWorks.Application.Features.Production.Commands;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Products;

[ExcludeFromCodeCoverage]
public sealed class UpdateProductControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<UpdateProductController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly UpdateProductController _sut;

    public UpdateProductControllerTests()
    {
        _sut = new UpdateProductController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new UpdateProductController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new UpdateProductController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PutAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PutAsync(1, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The product input model cannot be null.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task PutAsync_invalid_id_returns_bad_requestAsync(int id)
    {
        var result = await _sut.PutAsync(id, new ProductUpdateModel { Id = 1, Name = "test", ProductNumber = "TP-001" });

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The product id must be a positive integer.");
        }
    }

    [Fact]
    public async Task PutAsync_mismatched_ids_returns_bad_requestAsync()
    {
        var result = await _sut.PutAsync(1, new ProductUpdateModel { Id = 2, Name = "test", ProductNumber = "TP-001" });

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The product id parameter must match the id of the product update request payload.");
        }
    }

    [Fact]
    public void PutAsync_invalid_input_handles_exception()
    {
        var input = new ProductUpdateModel
        {
            Id = 1,
            Name = "",
            ProductNumber = "TP-001"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateProductCommand>(), CancellationToken.None))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
                { new() { PropertyName = "Name", ErrorCode = "Rule-01", ErrorMessage = "Product name is required" } }));

        Func<Task> act = async () => await _sut.PutAsync(1, input);

        _ = act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PutAsync_succeeds_Async()
    {
        const int productId = 1;

        var productModel = new ProductDetailModel
        {
            Id = productId,
            Name = "Updated Mountain Bike",
            ProductNumber = "BK-M82S-38",
            ModifiedDate = DateTime.UtcNow
        };

        var input = new ProductUpdateModel
        {
            Id = productId,
            Name = "Updated Mountain Bike",
            ProductNumber = "BK-M82S-38",
            SafetyStockLevel = 100,
            ReorderPoint = 75,
            StandardCost = 1912.15m,
            ListPrice = 3399.99m,
            DaysToManufacture = 4,
            SellStartDate = DateTime.UtcNow
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateProductCommand>(), CancellationToken.None));

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productModel);

        var result = await _sut.PutAsync(productId, input);

        var objectResult = result as OkObjectResult;
        var outputModel = objectResult!.Value! as ProductDetailModel;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            outputModel.Should().NotBeNull();
            outputModel!.Id.Should().Be(productId);
            outputModel!.Name.Should().Be("Updated Mountain Bike");
        }
    }
}
