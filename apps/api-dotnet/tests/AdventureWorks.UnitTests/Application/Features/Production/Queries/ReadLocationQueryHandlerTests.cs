using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

public sealed class ReadLocationQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ILocationRepository> _mockRepository = new();
    private ReadLocationQueryHandler _sut;

    public ReadLocationQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(LocationToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadLocationQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadLocationQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadLocationQueryHandler(
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
    public async Task Handle_returns_null_when_entity_not_found()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Location)null!);

        var result = await _sut.Handle(new ReadLocationQuery { Id = 9999 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_correctly_mapped_model()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);

        _mockRepository.Setup(x => x.GetByIdAsync(1, cancellationToken))
            .ReturnsAsync(new Location
            {
                LocationId = (short)1,
                Name = "Tool Crib",
                CostRate = 0.00m,
                Availability = 96.46m,
                ModifiedDate = modifiedDate
            });

        var result = await _sut.Handle(new ReadLocationQuery { Id = 1 }, cancellationToken);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeOfType<LocationModel>();
            result!.LocationId.Should().Be((short)1);
            result.Name.Should().Be("Tool Crib");
            result.CostRate.Should().Be(0.00m);
            result.Availability.Should().Be(96.46m);
            result.ModifiedDate.Should().Be(modifiedDate);
        }

        _mockRepository.Verify(x => x.GetByIdAsync(1, cancellationToken), Times.Once);
    }
}
