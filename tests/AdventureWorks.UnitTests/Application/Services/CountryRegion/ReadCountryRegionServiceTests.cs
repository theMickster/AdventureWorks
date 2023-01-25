using System.Collections.ObjectModel;
using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.CountryRegion;
using AdventureWorks.Application.Services.CountryRegion;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Profiles;
using AutoMapper;

namespace AdventureWorks.UnitTests.Application.Services.CountryRegion;

[ExcludeFromCodeCoverage]
public sealed class ReadCountryRegionServiceTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ICountryRegionRepository> _mockCountryRegionRepository = new();
    private ReadCountryRegionService _sut;

    public ReadCountryRegionServiceTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(CountryRegionEntityToCountryRegionModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadCountryRegionService(_mapper, _mockCountryRegionRepository.Object);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ReadCountryRegionService)
                .Should().Implement<IReadCountryRegionService>();

            typeof(ReadCountryRegionService)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }


    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadCountryRegionService(
                    null!,
                    _mockCountryRegionRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadCountryRegionService(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("countryRegionRepository");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_Async()
    {
        _mockCountryRegionRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((CountryRegionEntity)null!);

        var result = await _sut.GetByIdAsync("UK").ConfigureAwait(false);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_correctly_Async()
    {
        _mockCountryRegionRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(new CountryRegionEntity{CountryRegionCode = "UK", Name = "United Kingdom"});

        var result = await _sut.GetByIdAsync("UK").ConfigureAwait(false);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Code.Should().Be("UK");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockCountryRegionRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync( (IReadOnlyList<CountryRegionEntity>)null! );

        var result = await _sut.GetListAsync().ConfigureAwait(false);
        result.Should().BeEmpty();

        _mockCountryRegionRepository.Reset();

        _mockCountryRegionRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<CountryRegionEntity>());

        result = await _sut.GetListAsync().ConfigureAwait(false);
        result.Should().BeEmpty();
    }


    [Fact]
    public async Task GetListAsync_returns_valid_listAsync()
    {
        _mockCountryRegionRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<CountryRegionEntity>()
            {
                new (){Name = "France", CountryRegionCode = "FR"}
                ,new() {Name = "Japan", CountryRegionCode = "JP"}
            });

        var result = await _sut.GetListAsync().ConfigureAwait(false);
        result.Count.Should().Be(2);
    }
}