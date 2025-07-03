using AdventureWorks.UserSetup.Console.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AdventureWorks.UserSetup.Console.DbContexts;

internal class AdventureWorksUserSetupContext : DbContext, IAdventureWorksUserSetupContext
{
    private readonly ILogger<AdventureWorksUserSetupContext> _logger;

    public AdventureWorksUserSetupContext(DbContextOptions<AdventureWorksUserSetupContext> options) : base(options)
    {
        _logger = new NullLogger<AdventureWorksUserSetupContext>();
    }

    public AdventureWorksUserSetupContext(
        DbContextOptions<AdventureWorksUserSetupContext> options,
        ILoggerFactory factory) : base(options)
    {
        _logger = factory.CreateLogger<AdventureWorksUserSetupContext>();
    }

    public DbSet<UserAccount> UserAccounts { get; set; } = null!;

    public async Task UpdateUserAccountsAsync(UserAccount entity)
    {
        Entry(entity).State = EntityState.Modified;
        await SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assemblyWithConfigurations = GetType().Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfigurations);
    }
}
