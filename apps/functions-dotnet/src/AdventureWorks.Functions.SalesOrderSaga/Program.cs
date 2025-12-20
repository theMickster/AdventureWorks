using System.Text.Json;
using System.Text.Json.Serialization;
using AdventureWorks.Functions.SalesOrderSaga.Persistence;
using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((_, config) => config.AddUserSecrets<Program>(optional: true))
    .ConfigureServices((context, services) =>
    {
        var sqlConnectionString = context.Configuration["SqlConnectionString"]
            ?? throw new InvalidOperationException("Missing required configuration value 'SqlConnectionString'.");

        services.AddDbContext<SalesOrderSagaDbContext>(options => options.UseSqlServer(sqlConnectionString));

        services.Configure<WorkerOptions>(workerOptions =>
        {
            workerOptions.Serializer = new JsonObjectSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Converters = { new JsonStringEnumConverter() }
            });
        });
    })
    .Build();

host.Run();
