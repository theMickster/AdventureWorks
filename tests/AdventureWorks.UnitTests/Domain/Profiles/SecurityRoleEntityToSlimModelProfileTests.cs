using AdventureWorks.Domain.Entities.Shield;
using AdventureWorks.Domain.Models.Shield;
using AdventureWorks.Domain.Profiles;
using AutoMapper;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class SecurityRoleEntityToSlimModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public SecurityRoleEntityToSlimModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SecurityRoleEntityToSlimModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var entity = new SecurityRoleEntity()
        {
            Id = 12,
            Name = "Super Awesome Permission",
            RecordId = new Guid(),
            Description = "Hello World from SecurityFunctionEntity",
            CreatedBy = 1,
        };

        var result = _mapper.Map<SecurityRoleSlimModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(12);
            result.Name.Should().Be("Super Awesome Permission");
            result.Code.Should().BeNull();
        }
    }

}
