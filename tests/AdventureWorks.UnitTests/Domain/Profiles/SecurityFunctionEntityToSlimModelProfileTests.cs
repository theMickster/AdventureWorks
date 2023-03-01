using AdventureWorks.Domain.Entities.Shield;
using AdventureWorks.Domain.Models.Shield;
using AdventureWorks.Domain.Profiles;
using AutoMapper;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class SecurityFunctionEntityToSlimModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public SecurityFunctionEntityToSlimModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SecurityFunctionEntityToSlimModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }


    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var entity = new SecurityFunctionEntity()
        {
            Id = 12,
            Name = "Super Awesome Permission",
            RecordId = new Guid(),
            Description = "Hello World from SecurityFunctionEntity",
            CreatedBy = 1,
        };

        var result = _mapper.Map<SecurityFunctionSlimModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(12);
            result.Name.Should().Be("Super Awesome Permission");
            result.Code.Should().BeNull();
        }
    }

}
