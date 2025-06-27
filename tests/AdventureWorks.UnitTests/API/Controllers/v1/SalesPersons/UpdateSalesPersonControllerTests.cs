using AdventureWorks.API.Controllers.v1.SalesPersons;
using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.SalesPersons;

[ExcludeFromCodeCoverage]
public sealed class UpdateSalesPersonControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<UpdateSalesPersonController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly UpdateSalesPersonController _sut;

    public UpdateSalesPersonControllerTests()
    {
        _sut = new UpdateSalesPersonController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new UpdateSalesPersonController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new UpdateSalesPersonController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PutAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PutAsync(274, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The sales person input model cannot be null.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task PutAsync_invalid_id_returns_bad_requestAsync(int salesPersonId)
    {
        var result = await _sut.PutAsync(salesPersonId, new SalesPersonUpdateModel { Id = 274 });

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The sales person id must be a positive integer.");
        }
    }

    [Fact]
    public async Task PutAsync_mismatched_ids_returns_bad_requestAsync()
    {
        var result = await _sut.PutAsync(274, new SalesPersonUpdateModel { Id = 275 });

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The sales person id parameter must match the id of the sales person update request payload.");
        }
    }

    [Fact]
    public void PutAsync_invalid_input_handles_exception()
    {
        var input = new SalesPersonUpdateModel
        {
            Id = 274,
            SalesQuota = -1000m,
            Bonus = 0m,
            CommissionPct = 0.012m
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateSalesPersonCommand>(), CancellationToken.None))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
                { new() { PropertyName = "SalesQuota", ErrorCode = "00010", ErrorMessage = "Sales quota must be positive" } }));

        Func<Task> act = async () => await _sut.PutAsync(274, input);

        _ = act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PutAsync_succeeds_Async()
    {
        const int salesPersonId = 274;

        var salesPersonModel = new SalesPersonModel
        {
            Id = salesPersonId,
            FirstName = "Stephen",
            LastName = "Jiang",
            JobTitle = "Sales Manager",
            SalesQuota = 300000m,
            Bonus = 5000m,
            CommissionPct = 0.015m,
            ModifiedDate = DateTime.UtcNow
        };

        var input = new SalesPersonUpdateModel
        {
            Id = salesPersonId,
            SalesQuota = 300000m,
            Bonus = 5000m,
            CommissionPct = 0.015m
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateSalesPersonCommand>(), CancellationToken.None));

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesPersonQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(salesPersonModel);

        var result = await _sut.PutAsync(salesPersonId, input);

        var objectResult = result as OkObjectResult;
        var outputModel = objectResult!.Value! as SalesPersonModel;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            outputModel.Should().NotBeNull();
            outputModel!.Id.Should().Be(salesPersonId);
            outputModel!.FirstName.Should().Be("Stephen");
            outputModel!.LastName.Should().Be("Jiang");
        }
    }
}
