using AdventureWorks.API.Controllers.v1.Stores;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Stores;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreContactControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadStoreContactController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadStoreContactController _sut;

    public ReadStoreContactControllerTests()
    {
        _sut = new ReadStoreContactController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadStoreContactController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadStoreContactController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task GetAllAsync_invalid_storeId_returns_bad_request(int storeId)
    {
        var result = await _sut.GetAllAsync(storeId);
        var objectResult = result as BadRequestObjectResult;
        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("A valid store id must be specified.");
        }
    }

    [Fact]
    public async Task GetAllAsync_returns_200_with_contact_listAsync()
    {
        var contacts = new List<StoreContactModel>
        {
            new() { Id = 987, FirstName = "Steve", LastName = "Jones", StoreId = 2534, ContactTypeId = 11, ContactTypeName = "Owner" },
            new() { Id = 988, FirstName = "Peter", LastName = "Jones", StoreId = 2534, ContactTypeId = 12, ContactTypeName = "Store Contact" }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreContactListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contacts);

        var result = await _sut.GetAllAsync(2534);

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            var returnedList = objectResult.Value as List<StoreContactModel>;
            returnedList.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task GetAllAsync_returns_200_with_empty_listAsync()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreContactListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StoreContactModel>());

        var result = await _sut.GetAllAsync(9999);

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            var returnedList = objectResult.Value as List<StoreContactModel>;
            returnedList.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task GetAllAsync_sends_correct_storeId_to_mediatorAsync()
    {
        const int storeId = 2534;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreContactListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StoreContactModel>());

        await _sut.GetAllAsync(storeId);

        _mockMediator.Verify(x => x.Send(
            It.Is<ReadStoreContactListQuery>(q => q.StoreId == storeId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 100, 11, "A valid store id must be specified.")]
    [InlineData(-1, 100, 11, "A valid store id must be specified.")]
    [InlineData(2534, 0, 11, "A valid person id must be specified.")]
    [InlineData(2534, -3, 11, "A valid person id must be specified.")]
    [InlineData(2534, 100, 0, "A valid contact type id must be specified.")]
    [InlineData(2534, 100, -7, "A valid contact type id must be specified.")]
    public async Task GetByCompositeKeyAsync_invalid_route_values_return_bad_requestAsync(int storeId, int personId, int contactTypeId, string expectedMessage)
    {
        var result = await _sut.GetByCompositeKeyAsync(storeId, personId, contactTypeId);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be(expectedMessage);
        }
    }

    [Fact]
    public async Task GetByCompositeKeyAsync_returns_not_found_when_query_returns_nullAsync()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreContactQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreContactModel?)null);

        var result = await _sut.GetByCompositeKeyAsync(2534, 100, 11);

        var notFound = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            notFound.Should().NotBeNull();
            notFound!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task GetByCompositeKeyAsync_returns_ok_with_modelAsync()
    {
        var output = new StoreContactModel
        {
            Id = 100,
            StoreId = 2534,
            ContactTypeId = 11,
            ContactTypeName = "Owner",
            FirstName = "Pat",
            LastName = "Smith"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreContactQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(output);

        var result = await _sut.GetByCompositeKeyAsync(2534, 100, 11);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value.Should().Be(output);
        }
    }

    [Fact]
    public async Task GetByCompositeKeyAsync_sends_correct_composite_key_to_mediatorAsync()
    {
        const int storeId = 2534;
        const int personId = 100;
        const int contactTypeId = 11;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreContactQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreContactModel?)null);

        await _sut.GetByCompositeKeyAsync(storeId, personId, contactTypeId);

        _mockMediator.Verify(x => x.Send(
            It.Is<ReadStoreContactQuery>(q =>
                q.StoreId == storeId && q.PersonId == personId && q.ContactTypeId == contactTypeId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
