using AdventureWorks.API.Controllers.v1.Stores;
using AdventureWorks.Application.Features.Sales.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Stores;

[ExcludeFromCodeCoverage]
public sealed class DeleteStoreAddressControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<DeleteStoreAddressController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly DeleteStoreAddressController _sut;

    public DeleteStoreAddressControllerTests()
    {
        _sut = new DeleteStoreAddressController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new DeleteStoreAddressController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new DeleteStoreAddressController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mediator");
        }
    }

    [Theory]
    [InlineData(0, 100, 2, "A valid store id must be specified.")]
    [InlineData(-1, 100, 2, "A valid store id must be specified.")]
    [InlineData(2534, 0, 2, "A valid address id must be specified.")]
    [InlineData(2534, -3, 2, "A valid address id must be specified.")]
    [InlineData(2534, 100, 0, "A valid address type id must be specified.")]
    [InlineData(2534, 100, -7, "A valid address type id must be specified.")]
    public async Task DeleteAsync_invalid_route_values_return_bad_requestAsync(int storeId, int addressId, int addressTypeId, string expectedMessage)
    {
        var result = await _sut.DeleteAsync(storeId, addressId, addressTypeId);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be(expectedMessage);
        }
    }

    [Fact]
    public async Task DeleteAsync_propagates_KeyNotFoundException_when_address_missingAsync()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<DeleteStoreAddressCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Address not found"));

        Func<Task> act = async () => await _sut.DeleteAsync(2534, 100, 2);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_valid_input_returns_no_contentAsync()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<DeleteStoreAddressCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var result = await _sut.DeleteAsync(2534, 100, 2);

        var noContent = result as NoContentResult;

        using (new AssertionScope())
        {
            noContent.Should().NotBeNull();
            noContent!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }
    }

    [Fact]
    public async Task DeleteAsync_threads_cancellation_token_to_mediatorAsync()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<DeleteStoreAddressCommand>(), token))
            .ReturnsAsync(Unit.Value);

        await _sut.DeleteAsync(2534, 100, 2, token);

        _mockMediator.Verify(x => x.Send(
            It.Is<DeleteStoreAddressCommand>(c =>
                c.StoreId == 2534 && c.AddressId == 100 && c.AddressTypeId == 2),
            token), Times.Once);
    }
}
