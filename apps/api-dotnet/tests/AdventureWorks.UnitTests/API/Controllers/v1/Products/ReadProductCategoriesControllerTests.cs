using AdventureWorks.API.Controllers.v1.Products;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Products;

[ExcludeFromCodeCoverage]
public sealed class ReadProductCategoriesControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadProductCategoriesController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadProductCategoriesController _sut;

    public ReadProductCategoriesControllerTests()
    {
        _sut = new ReadProductCategoriesController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadProductCategoriesController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadProductCategoriesController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetCategoriesAsync_returns_ok_Async()
    {
        var categories = new List<ProductCategoryModel>
        {
            new() { CategoryId = 1, Name = "Bikes" },
            new() { CategoryId = 2, Name = "Components" },
            new() { CategoryId = 3, Name = "Clothing" },
            new() { CategoryId = 4, Name = "Accessories" }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductCategoriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var result = await _sut.GetCategoriesAsync();
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var returnedList = objectResult.Value as List<ProductCategoryModel>;
            returnedList.Should().NotBeNull();
            returnedList!.Count.Should().Be(4);
        }
    }
}
