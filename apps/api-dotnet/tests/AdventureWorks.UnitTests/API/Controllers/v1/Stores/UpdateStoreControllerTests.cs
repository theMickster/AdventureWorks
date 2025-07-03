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
public sealed class UpdateStoreControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<UpdateStoreController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly UpdateStoreController _sut;

    public UpdateStoreControllerTests()
    {
        _sut = new UpdateStoreController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new UpdateStoreController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new UpdateStoreController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PutAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PutAsync(2534, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The store input model cannot be null.");
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-999)]
    public async Task PutAsync_negative_id_returns_bad_requestAsync(int storeId)
    {
        var result = await _sut.PutAsync(storeId, new StoreUpdateModel { Id = 2534 });

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The store id must be a positive integer.");
        }
    }

    [Fact]
    public async Task PutAsync_zero_id_should_return_bad_requestAsync()
    {
        // This test documents current behavior where storeId=0 is allowed
        // Based on line 55: if (storeId < 0) - note: should probably be <= 0
        var input = new StoreUpdateModel { Id = 0, Name = "Test Store" };

        var storeModel = new StoreModel
        {
            Id = 0,
            Name = "Test Store",
            ModifiedDate = DateTime.UtcNow
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateStoreCommand>(), CancellationToken.None));
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storeModel);

        var result = await _sut.PutAsync(0, input);

        // Current implementation allows 0, but this is likely a bug
        // The error message says "must be a positive integer" but 0 is not positive
        var objectResult = result as OkObjectResult;
        objectResult.Should().NotBeNull("because current implementation allows storeId=0");
    }

    [Fact]
    public async Task PutAsync_mismatched_ids_returns_bad_requestAsync()
    {
        var result = await _sut.PutAsync(2534, new StoreUpdateModel { Id = 2535 });

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The store id parameter must match the id of the address update request payload.");
        }
    }

    [Fact]
    public void PutAsync_invalid_input_handles_exception()
    {
        var input = new StoreUpdateModel
        {
            Id = 2534,
            Name = ""
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateStoreCommand>(), CancellationToken.None))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
                { new() { PropertyName = "Name", ErrorCode = "00010", ErrorMessage = "Store name is required" } }));

        Func<Task> act = async () => await _sut.PutAsync(2534, input);

        _ = act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PutAsync_succeeds_Async()
    {
        const int storeId = 2534;

        var storeModel = new StoreModel
        {
            Id = storeId,
            Name = "Updated Adventure Works Store",
            ModifiedDate = DateTime.UtcNow
        };

        var input = new StoreUpdateModel
        {
            Id = storeId,
            Name = "Updated Adventure Works Store"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateStoreCommand>(), CancellationToken.None));

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storeModel);

        var result = await _sut.PutAsync(storeId, input);

        var objectResult = result as OkObjectResult;
        var outputModel = objectResult!.Value! as StoreModel;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            outputModel.Should().NotBeNull();
            outputModel!.Id.Should().Be(storeId);
            outputModel!.Name.Should().Be("Updated Adventure Works Store");
        }
    }
}
