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
public sealed class CreateStoreContactControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<CreateStoreContactController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly CreateStoreContactController _sut;

    public CreateStoreContactControllerTests()
    {
        _sut = new CreateStoreContactController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new CreateStoreContactController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new CreateStoreContactController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PostAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PostAsync(2534, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The store contact input model cannot be null.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task PostAsync_invalid_storeId_returns_bad_requestAsync(int storeId)
    {
        var input = new StoreContactCreateModel { PersonId = 100, ContactTypeId = 11 };

        var result = await _sut.PostAsync(storeId, input);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("A valid store id must be specified.");
        }
    }

    [Fact]
    public async Task PostAsync_propagates_ValidationExceptionAsync()
    {
        var input = new StoreContactCreateModel { PersonId = 0, ContactTypeId = 0 };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<AddStoreContactCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = "PersonId", ErrorCode = "Rule-02", ErrorMessage = "Bad person" }
            }));

        Func<Task> act = async () => await _sut.PostAsync(2534, input);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PostAsync_propagates_KeyNotFoundException_when_store_missingAsync()
    {
        var input = new StoreContactCreateModel { PersonId = 100, ContactTypeId = 11 };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<AddStoreContactCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Store not found"));

        Func<Task> act = async () => await _sut.PostAsync(9999, input);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task PostAsync_valid_input_returns_createdAsync()
    {
        const int storeId = 2534;

        var input = new StoreContactCreateModel { PersonId = 100, ContactTypeId = 11 };

        var output = new StoreContactModel
        {
            Id = 100,
            StoreId = storeId,
            ContactTypeId = 11,
            ContactTypeName = "Owner",
            FirstName = "Pat",
            LastName = "Smith"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<AddStoreContactCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(input.PersonId);
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadStoreContactQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(output);

        var result = await _sut.PostAsync(storeId, input);

        var createdResult = result as CreatedAtRouteResult;

        using (new AssertionScope())
        {
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            createdResult!.RouteName.Should().Be("GetStoreContactByCompositeKey");

            var routeValues = createdResult!.RouteValues;
            routeValues.Should().ContainKey("storeId");
            routeValues!["storeId"].Should().Be(storeId);
            routeValues.Should().ContainKey("personId");
            routeValues!["personId"].Should().Be(input.PersonId);
            routeValues.Should().ContainKey("contactTypeId");
            routeValues!["contactTypeId"].Should().Be(input.ContactTypeId);

            var returned = createdResult.Value as StoreContactModel;
            returned.Should().NotBeNull();
            returned!.Id.Should().Be(100);
            returned!.StoreId.Should().Be(storeId);
            returned!.ContactTypeId.Should().Be(11);
        }
    }
}
