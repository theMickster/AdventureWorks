using AdventureWorks.Functions.SalesOrderSaga.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Environment-specific value only, no code branching: SQL-auth to the local `tosk-mssql`
        // container in local.settings.json, `Authentication=Active Directory Managed Identity`
        // (no secret) in Azure app settings. See Architecture Decision 4 in this app's CLAUDE.md.
        var sqlConnectionString = context.Configuration["SqlConnectionString"]
            ?? throw new InvalidOperationException("Missing required configuration value 'SqlConnectionString'.");

        services.AddDbContext<SalesOrderSagaDbContext>(options => options.UseSqlServer(sqlConnectionString));
    })
    .Build();

host.Run();
