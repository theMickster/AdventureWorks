using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

public sealed class ReadUnitMeasureListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IUnitMeasureRepository> _mockRepository = new();
    private ReadUnitMeasureListQueryHandler _sut;

    public ReadUnitMeasureListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(UnitMeasureToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadUnitMeasureListQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadUnitMeasureListQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadUnitMeasureListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("unitMeasureRepository");
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
            .ReturnsAsync((IReadOnlyList<UnitMeasure>)null!);

        var result = await _sut.Handle(new ReadUnitMeasureListQuery(), CancellationToken.None);
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UnitMeasure>());

        result = await _sut.Handle(new ReadUnitMeasureListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_returns_correctly_mapped_list()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);

        _mockRepository.Setup(x => x.ListAllAsync(cancellationToken))
            .ReturnsAsync(new List<UnitMeasure>
            {
                new() { UnitMeasureCode = "EA", Name = "Each", ModifiedDate = modifiedDate },
                new() { UnitMeasureCode = "LB", Name = "Pound", ModifiedDate = modifiedDate }
            });

        var result = await _sut.Handle(new ReadUnitMeasureListQuery(), cancellationToken);

        using (new AssertionScope())
        {
            result.Count.Should().Be(2);
            result[0].UnitMeasureCode.Should().Be("EA");
            result[0].Name.Should().Be("Each");
            result[1].UnitMeasureCode.Should().Be("LB");
        }

        _mockRepository.Verify(x => x.ListAllAsync(cancellationToken), Times.Once);
    }
}
