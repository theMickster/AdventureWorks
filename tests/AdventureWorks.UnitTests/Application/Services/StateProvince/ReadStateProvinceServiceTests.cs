using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.StateProvince;
using AdventureWorks.Application.Services.StateProvince;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Profiles;
using AutoMapper;
using Moq;

namespace AdventureWorks.UnitTests.Application.Services.StateProvince;

[ExcludeFromCodeCoverage]
public sealed class ReadStateProvinceServiceTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IStateProvinceRepository> _mockStateProvinceRepository = new();
    private ReadStateProvinceService _sut;

    public ReadStateProvinceServiceTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(StateProvinceEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadStateProvinceService(_mapper, _mockStateProvinceRepository.Object);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ReadStateProvinceService)
                .Should().Implement<IReadStateProvinceService>();

            typeof(ReadStateProvinceService)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }


    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStateProvinceService(
                    null!,
                    _mockStateProvinceRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStateProvinceService(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("stateProvinceRepository");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_Async()
    {
        _mockStateProvinceRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((StateProvinceEntity)null!);

        var result = await _sut.GetByIdAsync(12).ConfigureAwait(false);

        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetByIdAsync_returns_correctly_Async()
    {
        _mockStateProvinceRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new StateProvinceEntity
                { StateProvinceId = 1, Name = "A State", CountryRegionCode = "UK", TerritoryId = 7, IsOnlyStateProvinceFlag = false});

        var result = await _sut.GetByIdAsync(15).ConfigureAwait(false);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Name.Should().Be("A State");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockStateProvinceRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync((IReadOnlyList<StateProvinceEntity>)null!);

        var result = await _sut.GetListAsync().ConfigureAwait(false);
        result.Should().BeEmpty();

        _mockStateProvinceRepository.Reset();

        _mockStateProvinceRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<StateProvinceEntity>());

        result = await _sut.GetListAsync().ConfigureAwait(false);
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

        var result = await _sut.GetListAsync().ConfigureAwait(false);
        result.Count.Should().Be(2);
    }
}
