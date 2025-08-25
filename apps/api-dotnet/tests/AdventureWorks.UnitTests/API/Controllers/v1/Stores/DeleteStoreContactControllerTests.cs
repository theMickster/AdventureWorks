using AdventureWorks.API.Controllers.v1.Stores;
using AdventureWorks.Application.Features.Sales.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Stores;

[ExcludeFromCodeCoverage]
public sealed class DeleteStoreContactControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<DeleteStoreContactController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly DeleteStoreContactController _sut;

    public DeleteStoreContactControllerTests()
    {
        _sut = new DeleteStoreContactController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new DeleteStoreContactController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new DeleteStoreContactController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mediator");
        }
    }

    [Theory]
    [InlineData(0, 100, 11, "A valid store id must be specified.")]
    [InlineData(-1, 100, 11, "A valid store id must be specified.")]
    [InlineData(2534, 0, 11, "A valid person id must be specified.")]
    [InlineData(2534, -3, 11, "A valid person id must be specified.")]
    [InlineData(2534, 100, 0, "A valid contact type id must be specified.")]
    [InlineData(2534, 100, -7, "A valid contact type id must be specified.")]
    public async Task DeleteAsync_invalid_route_values_return_bad_requestAsync(int storeId, int personId, int contactTypeId, string expectedMessage)
    {
        var result = await _sut.DeleteAsync(storeId, personId, contactTypeId);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be(expectedMessage);
        }
    }

    [Fact]
    public async Task DeleteAsync_propagates_KeyNotFoundException_when_contact_missingAsync()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<DeleteStoreContactCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Contact not found"));

        Func<Task> act = async () => await _sut.DeleteAsync(2534, 100, 11);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_valid_input_returns_no_contentAsync()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<DeleteStoreContactCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var result = await _sut.DeleteAsync(2534, 100, 11);

        var noContent = result as NoContentResult;

        using (new AssertionScope())
        {
            noContent.Should().NotBeNull();
            noContent!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }
    }
}
