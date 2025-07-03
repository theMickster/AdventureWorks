using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

public sealed class ReadPersonTypeListQueryHandlerTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IPersonTypeRepository> _mockRepository = new();
    private ReadPersonTypeListQueryHandler _sut;

    public ReadPersonTypeListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(PersonTypeEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadPersonTypeListQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadPersonTypeListQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadPersonTypeListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("personTypeRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_empty_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync((IReadOnlyList<PersonTypeEntity>)null!);

        var result = await _sut.Handle(new ReadPersonTypeListQuery(), CancellationToken.None);
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<PersonTypeEntity>());

        result = await _sut.Handle(new ReadPersonTypeListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_returns_valid_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<PersonTypeEntity>
            {
                new() {PersonTypeId = 1, PersonTypeName = "Home", PersonTypeDescription = "test"}
                ,new() {PersonTypeId = 2, PersonTypeName = "Billing", PersonTypeDescription = "test02"}
                ,new() {PersonTypeId = 3, PersonTypeName = "Mailing", PersonTypeDescription = "test03"}
            });

        var result = await _sut.Handle(new ReadPersonTypeListQuery(), CancellationToken.None);
        result.Count.Should().Be(3);
    }
}
