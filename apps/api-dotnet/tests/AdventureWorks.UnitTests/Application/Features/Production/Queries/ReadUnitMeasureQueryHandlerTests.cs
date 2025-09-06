using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

public sealed class ReadUnitMeasureQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IUnitMeasureRepository> _mockRepository = new();
    private ReadUnitMeasureQueryHandler _sut;

    public ReadUnitMeasureQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(UnitMeasureToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadUnitMeasureQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadUnitMeasureQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadUnitMeasureQueryHandler(
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
    public async Task Handle_returns_null_when_entity_not_found()
    {
        _mockRepository.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UnitMeasure)null!);

        var result = await _sut.Handle(new ReadUnitMeasureQuery { Code = "ZZZ" }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_correctly_mapped_model()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);

        _mockRepository.Setup(x => x.GetByCodeAsync("EA", cancellationToken))
            .ReturnsAsync(new UnitMeasure
            {
                UnitMeasureCode = "EA",
                Name = "Each",
                ModifiedDate = modifiedDate
            });

        var result = await _sut.Handle(new ReadUnitMeasureQuery { Code = "EA" }, cancellationToken);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeOfType<UnitMeasureModel>();
            result!.UnitMeasureCode.Should().Be("EA");
            result.Name.Should().Be("Each");
            result.ModifiedDate.Should().Be(modifiedDate);
        }

        _mockRepository.Verify(x => x.GetByCodeAsync("EA", cancellationToken), Times.Once);
    }
}
