using AdventureWorks.Application.Features.AddressManagement.Profiles;
using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.AddressManagement.Queries;

public sealed class ReadCountryRegionListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ICountryRegionRepository> _mockCountryRegionRepository = new();
    private ReadCountryRegionListQueryHandler _sut;

    public ReadCountryRegionListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(CountryRegionEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadCountryRegionListQueryHandler(_mapper, _mockCountryRegionRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadCountryRegionListQueryHandler(
                    null!,
                    _mockCountryRegionRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadCountryRegionListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("repository");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockCountryRegionRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync((IReadOnlyList<CountryRegionEntity>)null!);

        var result = await _sut.Handle( new ReadCountryRegionListQuery(), CancellationToken.None );
        result.Should().BeEmpty();

        _mockCountryRegionRepository.Reset();

        _mockCountryRegionRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<CountryRegionEntity>());

        result = await _sut.Handle(new ReadCountryRegionListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetListAsync_returns_valid_listAsync()
    {
        _mockCountryRegionRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<CountryRegionEntity>
            {
                new (){Name = "France", CountryRegionCode = "FR"}
                ,new() {Name = "Japan", CountryRegionCode = "JP"}
            });

        var result = await _sut.Handle(new ReadCountryRegionListQuery(), CancellationToken.None);
        result.Count.Should().Be(2);
    }
}
