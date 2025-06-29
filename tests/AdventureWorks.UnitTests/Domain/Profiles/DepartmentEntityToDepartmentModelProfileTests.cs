using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class DepartmentEntityToDepartmentModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public DepartmentEntityToDepartmentModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(DepartmentEntityToDepartmentModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var entity = new DepartmentEntity
        {
            DepartmentId = 5,
            Name = "Engineering",
            GroupName = "Research and Development",
            ModifiedDate = new DateTime(2024, 1, 15, 10, 30, 0)
        };

        var result = _mapper.Map<DepartmentModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(5);
            result.Name.Should().Be("Engineering");
            result.GroupName.Should().Be("Research and Development");
            result.ModifiedDate.Should().Be(new DateTime(2024, 1, 15, 10, 30, 0));
        }
    }
}
