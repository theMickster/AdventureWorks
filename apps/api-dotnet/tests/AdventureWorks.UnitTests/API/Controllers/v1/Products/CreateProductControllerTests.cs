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
public sealed class CreateProductControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<CreateProductController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly CreateProductController _sut;

    public CreateProductControllerTests()
    {
        _sut = new CreateProductController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new CreateProductController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new CreateProductController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PostAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PostAsync(null);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value.Should().Be("The product input model cannot be null.");
        }
    }

    [Fact]
    public void PostAsync_invalid_input_handles_exception()
    {
        var input = new ProductCreateModel
        {
            Name = "",
            ProductNumber = "BK-TEST-01",
            SafetyStockLevel = 100,
            ReorderPoint = 75,
            StandardCost = 0m,
            ListPrice = 0m,
            DaysToManufacture = 0,
            SellStartDate = DateTime.UtcNow
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateProductCommand>(), CancellationToken.None))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
                { new() { PropertyName = "Name", ErrorCode = "Rule-01", ErrorMessage = "Product name is required" } }));

        Func<Task> act = async () => await _sut.PostAsync(input);

        _ = act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PostAsync_valid_input_returns_createdAsync()
    {
        const int newProductId = 999;

        var productModel = new ProductDetailModel
        {
            Id = newProductId,
            Name = "Test Mountain Bike",
            ProductNumber = "BK-TEST-01",
            ModifiedDate = DateTime.UtcNow
        };

        var input = new ProductCreateModel
        {
            Name = "Test Mountain Bike",
            ProductNumber = "BK-TEST-01",
            SafetyStockLevel = 100,
            ReorderPoint = 75,
            StandardCost = 1912.15m,
            ListPrice = 3399.99m,
            DaysToManufacture = 4,
            SellStartDate = DateTime.UtcNow
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateProductCommand>(), CancellationToken.None))
            .ReturnsAsync(newProductId);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadProductQuery>(), CancellationToken.None))
            .ReturnsAsync(productModel);

        var result = await _sut.PostAsync(input);

        var createdResult = result as CreatedAtRouteResult;

        using (new AssertionScope())
        {
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            createdResult!.RouteName.Should().Be("GetProductById");

            var routeValues = createdResult!.RouteValues;
            routeValues.Should().ContainKey("id");
            routeValues!["id"].Should().Be(newProductId);

            var returnedModel = createdResult.Value as ProductDetailModel;
            returnedModel.Should().NotBeNull();
            returnedModel!.Id.Should().Be(newProductId);
            returnedModel!.Name.Should().Be("Test Mountain Bike");
        }
    }
}
