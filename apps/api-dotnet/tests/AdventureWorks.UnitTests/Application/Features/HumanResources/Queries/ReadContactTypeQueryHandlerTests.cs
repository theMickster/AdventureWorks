using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

public sealed class ReadContactTypeQueryHandlerTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IContactTypeRepository> _mockRepository = new();
    private ReadContactTypeQueryHandler _sut;

    public ReadContactTypeQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(PersonTypeEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadContactTypeQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadContactTypeQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadContactTypeQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("contactTypeRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_null_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((ContactTypeEntity)null!);

        var result = await _sut.Handle(new ReadContactTypeQuery { Id = 12 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_correctly_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new ContactTypeEntity { ContactTypeId = 1, Name = "Home" });

        var result = await _sut.Handle( new ReadContactTypeQuery { Id = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.Name.Should().Be("Home");
        }
    }
}
