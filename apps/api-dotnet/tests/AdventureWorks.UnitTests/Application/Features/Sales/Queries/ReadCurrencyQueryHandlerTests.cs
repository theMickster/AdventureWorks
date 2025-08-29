using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadCurrencyQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ICurrencyRepository> _mockRepository = new();
    private ReadCurrencyQueryHandler _sut;

    public ReadCurrencyQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(CurrencyEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadCurrencyQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadCurrencyQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadCurrencyQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("currencyRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_null_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null!);

        var result = await _sut.Handle(new ReadCurrencyQuery { Code = "ZZZ" }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_correctly_Async()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);

        _mockRepository.Setup(x => x.GetByIdAsync("USD", cancellationToken))
            .ReturnsAsync(new Currency
            {
                CurrencyCode = "USD",
                Name = "US Dollar",
                ModifiedDate = modifiedDate
            });

        var result = await _sut.Handle(new ReadCurrencyQuery { Code = "USD" }, cancellationToken);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.CurrencyCode.Should().Be("USD");
            result.Name.Should().Be("US Dollar");
            result.ModifiedDate.Should().Be(modifiedDate);
        }

        _mockRepository.Verify(x => x.GetByIdAsync("USD", cancellationToken), Times.Once);
    }
}
