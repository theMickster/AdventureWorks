using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadSalesReasonQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISalesReasonRepository> _mockRepository = new();
    private ReadSalesReasonQueryHandler _sut;

    public ReadSalesReasonQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SalesReasonEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadSalesReasonQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadSalesReasonQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadSalesReasonQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("salesReasonRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_null_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SalesReason)null!);

        var result = await _sut.Handle(new ReadSalesReasonQuery { Id = 999 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_correctly_Async()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);

        _mockRepository.Setup(x => x.GetByIdAsync(1, cancellationToken))
            .ReturnsAsync(new SalesReason
            {
                SalesReasonId = 1,
                Name = "Price",
                ReasonType = "Other",
                ModifiedDate = modifiedDate
            });

        var result = await _sut.Handle(new ReadSalesReasonQuery { Id = 1 }, cancellationToken);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.SalesReasonId.Should().Be(1);
            result.Name.Should().Be("Price");
            result.ReasonType.Should().Be("Other");
            result.ModifiedDate.Should().Be(modifiedDate);
        }

        _mockRepository.Verify(x => x.GetByIdAsync(1, cancellationToken), Times.Once);
    }
}
