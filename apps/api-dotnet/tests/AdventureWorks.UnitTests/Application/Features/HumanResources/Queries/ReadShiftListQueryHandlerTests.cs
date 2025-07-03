using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Domain.Entities.HumanResources;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

public sealed class ReadShiftListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IShiftRepository> _mockRepository = new();
    private ReadShiftListQueryHandler _sut;

    public ReadShiftListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ShiftEntityToShiftModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadShiftListQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadShiftListQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadShiftListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("shiftRepository");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync((IReadOnlyList<ShiftEntity>)null!);

        var result = await _sut.Handle(new ReadShiftListQuery(), CancellationToken.None);
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<ShiftEntity>());

        result = await _sut.Handle(new ReadShiftListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetListAsync_returns_valid_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<ShiftEntity>
            {
                new() { ShiftId = 1, Name = "Day", StartTime = new TimeSpan(7, 0, 0), EndTime = new TimeSpan(15, 0, 0) }
                ,new() { ShiftId = 2, Name = "Evening", StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(23, 0, 0) }
                ,new() { ShiftId = 3, Name = "Night", StartTime = new TimeSpan(23, 0, 0), EndTime = new TimeSpan(7, 0, 0) }
            });

        var result = await _sut.Handle(new ReadShiftListQuery(), CancellationToken.None);
        result.Count.Should().Be(3);
    }
}
