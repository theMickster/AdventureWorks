using AdventureWorks.API.Controllers.v1.SpecialOffer;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.SpecialOffer;

[ExcludeFromCodeCoverage]
public sealed class ReadSpecialOfferControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadSpecialOfferController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadSpecialOfferController _sut;

    public ReadSpecialOfferControllerTests()
    {
        _sut = new ReadSpecialOfferController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadSpecialOfferController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadSpecialOfferController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public void Controller_has_authorize_attribute()
    {
        typeof(ReadSpecialOfferController)
            .IsDefined(typeof(AuthorizeAttribute), true)
            .Should().BeTrue();
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        var today = DateTime.UtcNow.Date;

        _mockMediator.Setup(
                x => x.Send(It.Is<ReadSpecialOfferQuery>(q => q.Id == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecialOfferModel
            {
                SpecialOfferId = 1,
                Description = "Holiday Promotion",
                DiscountPct = 0.10m,
                Type = "Discount",
                Category = "Customer",
                StartDate = today.AddDays(-7),
                EndDate = today.AddDays(7),
                MinQty = 1,
                MaxQty = 10,
                IsActive = true,
                ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc)
            });

        var result = await _sut.GetByIdAsync(1);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<SpecialOfferModel>();
        }
    }

    [Fact]
    public async Task GetById_returns_not_found_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSpecialOfferQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SpecialOfferModel)null!);

        var result = await _sut.GetByIdAsync(999);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate the special offer.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetById_returns_bad_request_Async(int id)
    {
        var result = await _sut.GetByIdAsync(id);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            outputModel.Should().Be("A valid special offer id must be specified.");
        }
    }

    [Fact]
    public async Task GetList_returns_ok_Async()
    {
        var today = DateTime.UtcNow.Date;

        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadSpecialOfferListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<SpecialOfferModel>
                {
                    new()
                    {
                        SpecialOfferId = 1,
                        Description = "Holiday Promotion",
                        DiscountPct = 0.10m,
                        Type = "Discount",
                        Category = "Customer",
                        StartDate = today.AddDays(-7),
                        EndDate = today.AddDays(7),
                        MinQty = 1,
                        MaxQty = 10,
                        IsActive = true,
                        ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new()
                    {
                        SpecialOfferId = 2,
                        Description = "Clearance Promotion",
                        DiscountPct = 0.25m,
                        Type = "Discount",
                        Category = "Reseller",
                        StartDate = today.AddDays(-30),
                        EndDate = today.AddDays(-7),
                        MinQty = 5,
                        MaxQty = null,
                        IsActive = false,
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
    public async Task GetList_returns_not_found_with_empty_list_Async()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadSpecialOfferListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SpecialOfferModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate records in the special offer list.");
        }
    }
}
