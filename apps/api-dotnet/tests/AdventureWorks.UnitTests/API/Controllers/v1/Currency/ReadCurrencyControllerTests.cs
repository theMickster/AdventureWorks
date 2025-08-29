using AdventureWorks.API.Controllers.v1.Currency;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Currency;

[ExcludeFromCodeCoverage]
public sealed class ReadCurrencyControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadCurrencyController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadCurrencyController _sut;

    public ReadCurrencyControllerTests()
    {
        _sut = new ReadCurrencyController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadCurrencyController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadCurrencyController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public void Controller_has_authorize_attribute()
    {
        typeof(ReadCurrencyController)
            .IsDefined(typeof(AuthorizeAttribute), true)
            .Should().BeTrue();
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        _mockMediator.Setup(
                x => x.Send(It.Is<ReadCurrencyQuery>(q => q.Code == "USD"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrencyModel
            {
                CurrencyCode = "USD",
                Name = "US Dollar",
                ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc)
            });

        var result = await _sut.GetByIdAsync("USD");
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<CurrencyModel>();
        }
    }

    [Fact]
    public async Task GetById_returns_not_found_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadCurrencyQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyModel)null!);

        var result = await _sut.GetByIdAsync("ZZZ");
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate the currency.");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task GetById_returns_bad_request_Async(string code)
    {
        var result = await _sut.GetByIdAsync(code);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            outputModel.Should().Be("A valid currency code must be specified.");
        }
    }

    [Fact]
    public async Task GetList_returns_ok_Async()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadCurrencyListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<CurrencyModel>
                {
                    new()
                    {
                        CurrencyCode = "USD",
                        Name = "US Dollar",
                        ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new()
                    {
                        CurrencyCode = "EUR",
                        Name = "Euro",
                        ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc)
                    }
                });

        var result = await _sut.GetListAsync();
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetList_returns_ok_with_empty_list_Async()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadCurrencyListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CurrencyModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as OkObjectResult;
        var outputModel = objectResult!.Value as List<CurrencyModel>;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            outputModel.Should().NotBeNull();
            outputModel.Should().BeEmpty();
        }
    }
}
