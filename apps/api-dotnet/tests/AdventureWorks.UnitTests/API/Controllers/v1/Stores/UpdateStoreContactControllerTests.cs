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
public sealed class UpdateStoreContactControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<UpdateStoreContactController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly UpdateStoreContactController _sut;

    public UpdateStoreContactControllerTests()
    {
        _sut = new UpdateStoreContactController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new UpdateStoreContactController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new UpdateStoreContactController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PatchAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PatchAsync(2534, 100, 11, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The store contact input model cannot be null.");
        }
    }

    [Theory]
    [InlineData(0, 100, 11, "A valid store id must be specified.")]
    [InlineData(-1, 100, 11, "A valid store id must be specified.")]
    [InlineData(2534, 0, 11, "A valid person id must be specified.")]
    [InlineData(2534, -5, 11, "A valid person id must be specified.")]
    [InlineData(2534, 100, 0, "A valid contact type id must be specified.")]
    [InlineData(2534, 100, -2, "A valid contact type id must be specified.")]
    public async Task PatchAsync_invalid_route_values_return_bad_requestAsync(int storeId, int personId, int contactTypeId, string expectedMessage)
    {
        var input = new StoreContactUpdateModel { ContactTypeId = 12 };

        var result = await _sut.PatchAsync(storeId, personId, contactTypeId, input);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be(expectedMessage);
        }
    }

    [Fact]
    public async Task PatchAsync_propagates_ValidationExceptionAsync()
    {
        var input = new StoreContactUpdateModel { ContactTypeId = 0 };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateStoreContactTypeCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = "ContactTypeId", ErrorCode = "Rule-01", ErrorMessage = "Bad type" }
            }));

        Func<Task> act = async () => await _sut.PatchAsync(2534, 100, 11, input);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PatchAsync_propagates_KeyNotFoundException_when_contact_missingAsync()
    {
        var input = new StoreContactUpdateModel { ContactTypeId = 12 };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateStoreContactTypeCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Contact not found"));

        Func<Task> act = async () => await _sut.PatchAsync(2534, 100, 11, input);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task PatchAsync_valid_input_returns_okAsync()
    {
        const int storeId = 2534;
        var input = new StoreContactUpdateModel { ContactTypeId = 12 };

        var output = new StoreContactModel
        {
            Id = 100,
            StoreId = storeId,
            ContactTypeId = 12,
            ContactTypeName = "Manager",
            FirstName = "Pat",
            LastName = "Smith"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateStoreContactTypeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadStoreContactQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(output);

        var result = await _sut.PatchAsync(storeId, 100, 11, input);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var returned = okResult!.Value as StoreContactModel;
            returned.Should().NotBeNull();
            returned!.ContactTypeId.Should().Be(12);
            returned!.ContactTypeName.Should().Be("Manager");
        }
    }
}
