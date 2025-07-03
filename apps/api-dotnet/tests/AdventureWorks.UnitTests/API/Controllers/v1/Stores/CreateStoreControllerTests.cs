using AdventureWorks.API.Controllers.v1.Stores;
using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Stores;

[ExcludeFromCodeCoverage]
public sealed class CreateStoreControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<CreateStoreController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly CreateStoreController _sut;

    public CreateStoreControllerTests()
    {
        _sut = new CreateStoreController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new CreateStoreController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new CreateStoreController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
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
            objectResult!.Value.Should().Be("The store input model cannot be null.");
        }
    }

    [Fact]
    public void PostAsync_invalid_input_handles_exception()
    {
        var input = new StoreCreateModel
        {
            Name = ""
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateStoreCommand>(), CancellationToken.None))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
                { new() { PropertyName = "Name", ErrorCode = "00010", ErrorMessage = "Store name is required" } }));

        Func<Task> act = async () => await _sut.PostAsync(input);

        _ = act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PostAsync_valid_input_returns_createdAsync()
    {
        const int newStoreId = 9999;

        var storeModel = new StoreModel
        {
            Id = newStoreId,
            Name = "Adventure Works Cycle Store",
            ModifiedDate = DateTime.UtcNow
        };

        var input = new StoreCreateModel
        {
            Name = "Adventure Works Cycle Store",
            SalesPersonId = 274
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateStoreCommand>(), CancellationToken.None))
            .ReturnsAsync(newStoreId);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadStoreQuery>(), CancellationToken.None))
            .ReturnsAsync(storeModel);

        var result = await _sut.PostAsync(input);

        var createdResult = result as CreatedAtRouteResult;

        using (new AssertionScope())
        {
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            createdResult!.RouteName.Should().Be("GetStoreById");

            var routeValues = createdResult!.RouteValues;
            routeValues.Should().ContainKey("storeId");
            routeValues!["storeId"].Should().Be(newStoreId);

            var returnedModel = createdResult.Value as StoreModel;
            returnedModel.Should().NotBeNull();
            returnedModel!.Id.Should().Be(newStoreId);
            returnedModel!.Name.Should().Be("Adventure Works Cycle Store");
        }
    }
}
