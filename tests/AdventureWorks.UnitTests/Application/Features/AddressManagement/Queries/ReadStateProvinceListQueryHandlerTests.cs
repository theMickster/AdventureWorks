using AdventureWorks.Application.Features.AddressManagement.Profiles;
using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.AddressManagement.Queries;

public sealed class ReadStateProvinceListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IStateProvinceRepository> _mockStateProvinceRepository = new();
    private ReadStateProvinceListQueryHandler _sut;

    public ReadStateProvinceListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(StateProvinceEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadStateProvinceListQueryHandler(_mapper, _mockStateProvinceRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStateProvinceListQueryHandler(
                    null!,
                    _mockStateProvinceRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStateProvinceListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("repository");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockStateProvinceRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync((IReadOnlyList<StateProvinceEntity>)null!);

        var result = await _sut.Handle(new ReadStateProvinceListQuery(), CancellationToken.None);
        result.Should().BeEmpty();

        _mockStateProvinceRepository.Reset();

        _mockStateProvinceRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<StateProvinceEntity>());

        result = await _sut.Handle(new ReadStateProvinceListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetListAsync_returns_valid_listAsync()
    {
        _mockStateProvinceRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<StateProvinceEntity>
            {
                new (){ StateProvinceId = 1, Name = "France", CountryRegionCode = "FR", TerritoryId = 7, IsOnlyStateProvinceFlag = false}
                ,new() { StateProvinceId = 2, Name = "Japan", CountryRegionCode = "JP", TerritoryId = 8, IsOnlyStateProvinceFlag = true}
            });

        var result = await _sut.Handle(new ReadStateProvinceListQuery(), CancellationToken.None);
        result.Count.Should().Be(2);
    }
}
