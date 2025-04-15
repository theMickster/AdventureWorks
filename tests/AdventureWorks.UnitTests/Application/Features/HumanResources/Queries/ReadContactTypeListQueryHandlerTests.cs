using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

public sealed class ReadContactTypeListQueryHandlerTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IContactTypeRepository> _mockRepository = new();
    private ReadContactTypeListQueryHandler _sut;

    public ReadContactTypeListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(PersonTypeEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadContactTypeListQueryHandler(_mapper, _mockRepository.Object);
    }
    
    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadContactTypeListQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadContactTypeListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("contactTypeRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_empty_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync((IReadOnlyList<ContactTypeEntity>)null!);

        var result = await _sut.Handle(new ReadContactTypeListQuery(), CancellationToken.None);
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<ContactTypeEntity>());

        result = await _sut.Handle(new ReadContactTypeListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_returns_valid_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<ContactTypeEntity>
            {
                new() {ContactTypeId = 1, Name = "Home"}
                ,new() {ContactTypeId = 2, Name = "Billing"}
                ,new() {ContactTypeId = 3, Name = "Mailing"}
            });

        var result = await _sut.Handle(new ReadContactTypeListQuery(), CancellationToken.None);
        result.Count.Should().Be(3);
    }
}
