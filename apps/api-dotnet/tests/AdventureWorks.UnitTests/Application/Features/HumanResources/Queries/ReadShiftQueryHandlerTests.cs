using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Domain.Entities.HumanResources;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

public sealed class ReadShiftQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IShiftRepository> _mockRepository = new();
    private ReadShiftQueryHandler _sut;

    public ReadShiftQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ShiftEntityToShiftModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadShiftQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadShiftQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadShiftQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("shiftRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_null_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((ShiftEntity)null!);

        var result = await _sut.Handle(new ReadShiftQuery { Id = 12 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_correctly_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new ShiftEntity { ShiftId = 1, Name = "Day", StartTime = new TimeSpan(7, 0, 0), EndTime = new TimeSpan(15, 0, 0) });

        var result = await _sut.Handle(new ReadShiftQuery { Id = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.Name.Should().Be("Day");
        }
    }
}
