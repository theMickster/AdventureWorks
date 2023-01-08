using System.Security.Cryptography;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Polly;

namespace AdventureWorks.UnitTests.Setup;

[ExcludeFromCodeCoverage]
internal abstract class PersistenceUnitTestBase : UnitTestBase
{
    protected AdventureWorksDbContext DbContext;

    protected PersistenceUnitTestBase()
    {
        var options = new DbContextOptionsBuilder<AdventureWorksDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        DbContext = new AdventureWorksDbContext(options);

        DbContext.Database.EnsureCreated();

        DbContext.SaveChanges();

        Setup();
    }

    protected sealed override void Setup()
    {

    }
    
}
