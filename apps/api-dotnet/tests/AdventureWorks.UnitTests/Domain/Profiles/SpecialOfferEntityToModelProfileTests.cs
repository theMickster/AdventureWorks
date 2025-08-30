using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class SpecialOfferEntityToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public SpecialOfferEntityToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SpecialOfferEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var today = DateTime.UtcNow.Date;
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);
        var entity = new SpecialOffer
        {
            SpecialOfferId = 1,
            Description = "Holiday Promotion",
            DiscountPct = 0.10m,
            Type = "Discount",
            Category = "Customer",
            StartDate = today.AddDays(-7),
            EndDate = today.AddDays(7),
            MinQty = 1,
            MaxQty = 10,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = modifiedDate,
            SpecialOfferProducts = []
        };

        var result = _mapper.Map<SpecialOfferModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.SpecialOfferId.Should().Be(1);
            result.Description.Should().Be("Holiday Promotion");
            result.DiscountPct.Should().Be(0.10m);
            result.Type.Should().Be("Discount");
            result.Category.Should().Be("Customer");
            result.StartDate.Should().Be(today.AddDays(-7));
            result.EndDate.Should().Be(today.AddDays(7));
            result.MinQty.Should().Be(1);
            result.MaxQty.Should().Be(10);
            result.IsActive.Should().BeTrue();
            result.ModifiedDate.Should().Be(modifiedDate);
        }
    }

    [Fact]
    public void Map_entities_to_model_sets_is_active_false_when_dates_are_outside_today()
    {
        var today = DateTime.UtcNow.Date;
        var entity = new SpecialOffer
        {
            SpecialOfferId = 2,
            Description = "Expired Promotion",
            DiscountPct = 0.25m,
            Type = "Discount",
            Category = "Reseller",
            StartDate = today.AddDays(-30),
            EndDate = today.AddDays(-1),
            MinQty = 5,
            MaxQty = null,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = today,
            SpecialOfferProducts = []
        };

        var result = _mapper.Map<SpecialOfferModel>(entity);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Map_entities_to_model_sets_is_active_true_when_offer_starts_later_today()
    {
        var now = DateTime.UtcNow;
        var entity = new SpecialOffer
        {
            SpecialOfferId = 3,
            Description = "Upcoming Promotion",
            DiscountPct = 0.15m,
            Type = "Discount",
            Category = "Customer",
            StartDate = now.AddHours(1),
            EndDate = now.AddHours(5),
            MinQty = 1,
            MaxQty = null,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = now,
            SpecialOfferProducts = []
        };

        var result = _mapper.Map<SpecialOfferModel>(entity);

        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Map_entities_to_model_sets_is_active_true_when_offer_end_date_is_today()
    {
        var now = DateTime.UtcNow;
        var entity = new SpecialOffer
        {
            SpecialOfferId = 4,
            Description = "Today-only Promotion",
            DiscountPct = 0.20m,
            Type = "Discount",
            Category = "Customer",
            StartDate = now.AddHours(-5),
            EndDate = now.AddHours(-1),
            MinQty = 1,
            MaxQty = null,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = now,
            SpecialOfferProducts = []
        };

        var result = _mapper.Map<SpecialOfferModel>(entity);

        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Map_entities_to_model_sets_is_active_false_when_offer_end_date_was_yesterday()
    {
        var yesterday = DateTime.Now.Date.AddDays(-1);

        var entity = new SpecialOffer
        {
            StartDate = yesterday.AddDays(-7),
            EndDate = yesterday,
            Type = string.Empty,
            Category = string.Empty,
            Description = string.Empty
        };

        var result = _mapper.Map<SpecialOfferModel>(entity);

        result.IsActive.Should().BeFalse();
    }
}
