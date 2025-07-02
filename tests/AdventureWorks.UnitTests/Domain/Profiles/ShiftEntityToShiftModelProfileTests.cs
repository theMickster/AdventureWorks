using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class ShiftEntityToShiftModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public ShiftEntityToShiftModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ShiftEntityToShiftModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var entity = new ShiftEntity
        {
            ShiftId = 1,
            Name = "Day",
            StartTime = new TimeSpan(7, 0, 0),
            EndTime = new TimeSpan(15, 0, 0),
            ModifiedDate = new DateTime(2024, 1, 15, 10, 30, 0)
        };

        var result = _mapper.Map<ShiftModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("Day");
            result.StartTime.Should().Be(new TimeSpan(7, 0, 0));
            result.EndTime.Should().Be(new TimeSpan(15, 0, 0));
            result.ModifiedDate.Should().Be(new DateTime(2024, 1, 15, 10, 30, 0));
        }
    }
}
