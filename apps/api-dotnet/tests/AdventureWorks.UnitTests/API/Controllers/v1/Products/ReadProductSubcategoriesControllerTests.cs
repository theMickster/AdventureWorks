using AdventureWorks.API.Controllers.v1.Products;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Products;

[ExcludeFromCodeCoverage]
public sealed class ReadProductSubcategoriesControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadProductSubcategoriesController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadProductSubcategoriesController _sut;

    public ReadProductSubcategoriesControllerTests()
    {
        _sut = new ReadProductSubcategoriesController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadProductSubcategoriesController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadProductSubcategoriesController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetSubcategoriesAsync_without_categoryId_returns_ok_Async()
    {
        var subcategories = new List<ProductSubcategoryModel>
        {
            new() { SubcategoryId = 1, ProductCategoryId = 1, Name = "Mountain Bikes", CategoryName = "Bikes" },
            new() { SubcategoryId = 2, ProductCategoryId = 1, Name = "Road Bikes", CategoryName = "Bikes" }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductSubcategoriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subcategories);

        var result = await _sut.GetSubcategoriesAsync();
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var returnedList = objectResult.Value as List<ProductSubcategoryModel>;
            returnedList.Should().NotBeNull();
            returnedList!.Count.Should().Be(2);
        }
    }

    [Fact]
    public async Task GetSubcategoriesAsync_with_categoryId_returns_ok_Async()
    {
        var subcategories = new List<ProductSubcategoryModel>
        {
            new() { SubcategoryId = 1, ProductCategoryId = 1, Name = "Mountain Bikes", CategoryName = "Bikes" }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductSubcategoriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subcategories);

        var result = await _sut.GetSubcategoriesAsync(1);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var returnedList = objectResult.Value as List<ProductSubcategoryModel>;
            returnedList.Should().NotBeNull();
            returnedList!.Count.Should().Be(1);
        }
    }
}
