using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

public sealed class ReadScrapReasonListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IScrapReasonRepository> _mockRepository = new();
    private ReadScrapReasonListQueryHandler _sut;

    public ReadScrapReasonListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ScrapReasonToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadScrapReasonListQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadScrapReasonListQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadScrapReasonListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("scrapReasonRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_when_request_is_null()
    {
        await ((Func<Task>)(() => _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_returns_empty_list_when_no_records()
    {
        _mockRepository.Setup(x => x.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<ScrapReason>)null!);

        var result = await _sut.Handle(new ReadScrapReasonListQuery(), CancellationToken.None);
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScrapReason>());

        result = await _sut.Handle(new ReadScrapReasonListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_returns_correctly_mapped_list()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);

        _mockRepository.Setup(x => x.ListAllAsync(cancellationToken))
            .ReturnsAsync(new List<ScrapReason>
            {
                new() { ScrapReasonId = (short)1, Name = "Brake assembly not as ordered", ModifiedDate = modifiedDate },
                new() { ScrapReasonId = (short)2, Name = "Color incorrect", ModifiedDate = modifiedDate }
            });

        var result = await _sut.Handle(new ReadScrapReasonListQuery(), cancellationToken);

        using (new AssertionScope())
        {
            result.Count.Should().Be(2);
            result[0].ScrapReasonId.Should().Be((short)1);
            result[0].Name.Should().Be("Brake assembly not as ordered");
            result[1].ScrapReasonId.Should().Be((short)2);
        }

        _mockRepository.Verify(x => x.ListAllAsync(cancellationToken), Times.Once);
    }
}
