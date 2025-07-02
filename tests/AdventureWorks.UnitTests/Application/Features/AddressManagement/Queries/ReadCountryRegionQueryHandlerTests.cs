using AdventureWorks.Application.Features.AddressManagement.Profiles;
using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.AddressManagement.Queries;

public sealed class ReadCountryRegionQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ICountryRegionRepository> _mockCountryRegionRepository = new();
    private ReadCountryRegionQueryHandler _sut;

    public ReadCountryRegionQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(CountryRegionEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadCountryRegionQueryHandler(_mapper, _mockCountryRegionRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadCountryRegionQueryHandler(
                    null!,
                    _mockCountryRegionRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadCountryRegionQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("repository");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_Async()
    {
        _mockCountryRegionRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((CountryRegionEntity)null!);

        var result = await _sut.Handle(new ReadCountryRegionQuery{Code = "UK" }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_correctly_Async()
    {
        _mockCountryRegionRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(new CountryRegionEntity { CountryRegionCode = "UK", Name = "United Kingdom" });

        var result = await _sut.Handle(new ReadCountryRegionQuery { Code = "UK" }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Code.Should().Be("UK");
        }
    }


}
