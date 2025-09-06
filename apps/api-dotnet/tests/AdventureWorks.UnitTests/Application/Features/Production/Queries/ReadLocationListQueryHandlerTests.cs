using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

public sealed class ReadLocationListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ILocationRepository> _mockRepository = new();
    private ReadLocationListQueryHandler _sut;

    public ReadLocationListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(LocationToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadLocationListQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadLocationListQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadLocationListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("locationRepository");
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
            .ReturnsAsync((IReadOnlyList<Location>)null!);

        var result = await _sut.Handle(new ReadLocationListQuery(), CancellationToken.None);
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Location>());

        result = await _sut.Handle(new ReadLocationListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_returns_correctly_mapped_list()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);

        _mockRepository.Setup(x => x.ListAllAsync(cancellationToken))
            .ReturnsAsync(new List<Location>
            {
                new() { LocationId = (short)1, Name = "Tool Crib", CostRate = 0.00m, Availability = 96.46m, ModifiedDate = modifiedDate },
                new() { LocationId = (short)2, Name = "Sheet Metal Racks", CostRate = 0.00m, Availability = 0.00m, ModifiedDate = modifiedDate }
            });

        var result = await _sut.Handle(new ReadLocationListQuery(), cancellationToken);

        using (new AssertionScope())
        {
            result.Count.Should().Be(2);
            result[0].LocationId.Should().Be((short)1);
            result[0].Name.Should().Be("Tool Crib");
            result[1].LocationId.Should().Be((short)2);
        }

        _mockRepository.Verify(x => x.ListAllAsync(cancellationToken), Times.Once);
    }
}
