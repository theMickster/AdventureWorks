using AdventureWorks.API.Controllers.v1.SalesPersons;
using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.SalesPersons;

[ExcludeFromCodeCoverage]
public sealed class PatchSalesPersonControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<PatchSalesPersonController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly PatchSalesPersonController _sut;

    public PatchSalesPersonControllerTests()
    {
        _sut = new PatchSalesPersonController(_mockLogger.Object, _mockMediator.Object);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private static SalesPersonSalesConfigUpdateModel GetValidModel(int id = 275)
    {
        return new SalesPersonSalesConfigUpdateModel
        {
            Id = id,
            TerritoryId = 2,
            SalesQuota = 300000,
            Bonus = 2000,
            CommissionPct = 0.12m
        };
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new PatchSalesPersonController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new PatchSalesPersonController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PatchAsync_null_input_returns_bad_request()
    {
        var result = await _sut.PatchAsync(275, null, CancellationToken.None);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The sales person sales config input model cannot be null.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task PatchAsync_invalid_id_returns_bad_request(int salesPersonId)
    {
        var result = await _sut.PatchAsync(salesPersonId, GetValidModel(275), CancellationToken.None);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The sales person id must be a positive integer.");
        }
    }

    [Fact]
    public async Task PatchAsync_mismatched_ids_returns_bad_request()
    {
        var result = await _sut.PatchAsync(275, GetValidModel(276), CancellationToken.None);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The sales person id parameter must match the id of the sales person sales config update request payload.");
        }
    }

    [Fact]
    public async Task PatchAsync_succeeds()
    {
        const int salesPersonId = 275;

        var salesPersonModel = new SalesPersonModel
        {
            Id = salesPersonId,
            FirstName = "Michael",
            LastName = "Blythe",
            JobTitle = "Sales Representative",
            EmailAddress = "michael@aw.com"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateSalesPersonSalesConfigCommand>(), It.IsAny<CancellationToken>()));

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesPersonQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(salesPersonModel);

        var result = await _sut.PatchAsync(salesPersonId, GetValidModel(), CancellationToken.None);

        var objectResult = result as OkObjectResult;
        var outputModel = objectResult!.Value! as SalesPersonModel;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            outputModel.Should().NotBeNull();
            outputModel!.Id.Should().Be(salesPersonId);
        }
    }
}
