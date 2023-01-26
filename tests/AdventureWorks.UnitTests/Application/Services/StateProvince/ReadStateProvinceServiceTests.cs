using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.StateProvince;
using AdventureWorks.Application.Services.StateProvince;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Profiles;
using AutoMapper;

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
            config.AddMaps(typeof(StateProvinceEntityToStateProvinceModelProfile).Assembly)
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


}
