using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadSpecialOfferListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISpecialOfferRepository> _mockRepository = new();
    private ReadSpecialOfferListQueryHandler _sut;

    public ReadSpecialOfferListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SpecialOfferEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadSpecialOfferListQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadSpecialOfferListQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadSpecialOfferListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("specialOfferRepository");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<SpecialOffer>)null!);

        var result = await _sut.Handle(new ReadSpecialOfferListQuery(), CancellationToken.None);
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SpecialOffer>());

        result = await _sut.Handle(new ReadSpecialOfferListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetListAsync_returns_valid_listAsync()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var today = DateTime.UtcNow.Date;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);

        _mockRepository.Setup(x => x.ListAllAsync(cancellationToken))
            .ReturnsAsync(new List<SpecialOffer>
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
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = modifiedDate,
                    SpecialOfferProducts = []
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
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = modifiedDate,
                    SpecialOfferProducts = []
                }
            });

        var result = await _sut.Handle(new ReadSpecialOfferListQuery(), cancellationToken);

        using (new AssertionScope())
        {
            result.Count.Should().Be(2);
            result[0].SpecialOfferId.Should().Be(1);
            result[0].IsActive.Should().BeTrue();
            result[0].ModifiedDate.Should().Be(modifiedDate);
            result[1].SpecialOfferId.Should().Be(2);
            result[1].IsActive.Should().BeFalse();
        }

        _mockRepository.Verify(x => x.ListAllAsync(cancellationToken), Times.Once);
    }
}
