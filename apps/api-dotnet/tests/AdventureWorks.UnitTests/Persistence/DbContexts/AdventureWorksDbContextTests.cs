using AdventureWorks.Domain.Entities.Person;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UnitTests.Persistence.DbContexts;

[ExcludeFromCodeCoverage]
public sealed class AdventureWorksDbContextTests : PersistenceUnitTestBase
{
    [Fact]
    public void PersonCreditCards_DbSet_is_resolvable_from_context()
    {
        DbContext.PersonCreditCards.Should().NotBeNull();
    }

    [Fact]
    public async Task PersonCreditCards_DbSet_round_trips_entityAsync()
    {
        var entity = new PersonCreditCard
        {
            BusinessEntityId = 1,
            CreditCardId = 100,
            ModifiedDate = StandardModifiedDate
        };

        DbContext.PersonCreditCards.Add(entity);
        await DbContext.SaveChangesAsync();

        var fetched = await DbContext.PersonCreditCards
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.BusinessEntityId == 1 && x.CreditCardId == 100);

        fetched.Should().NotBeNull();
        fetched!.ModifiedDate.Should().Be(StandardModifiedDate);
    }
}
