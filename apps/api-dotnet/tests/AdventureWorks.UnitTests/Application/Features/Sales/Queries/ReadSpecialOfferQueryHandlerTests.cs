using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadSpecialOfferQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISpecialOfferRepository> _mockRepository = new();
    private ReadSpecialOfferQueryHandler _sut;

    public ReadSpecialOfferQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SpecialOfferEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadSpecialOfferQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadSpecialOfferQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadSpecialOfferQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("specialOfferRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_null_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SpecialOffer)null!);

        var result = await _sut.Handle(new ReadSpecialOfferQuery { Id = 999 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_correctly_Async()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var today = DateTime.UtcNow.Date;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);

        _mockRepository.Setup(x => x.GetByIdAsync(1, cancellationToken))
            .ReturnsAsync(new SpecialOffer
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
            });

        var result = await _sut.Handle(new ReadSpecialOfferQuery { Id = 1 }, cancellationToken);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.SpecialOfferId.Should().Be(1);
            result.Description.Should().Be("Holiday Promotion");
            result.DiscountPct.Should().Be(0.10m);
            result.Type.Should().Be("Discount");
            result.Category.Should().Be("Customer");
            result.MinQty.Should().Be(1);
            result.MaxQty.Should().Be(10);
            result.IsActive.Should().BeTrue();
            result.ModifiedDate.Should().Be(modifiedDate);
        }

        _mockRepository.Verify(x => x.GetByIdAsync(1, cancellationToken), Times.Once);
    }
}
