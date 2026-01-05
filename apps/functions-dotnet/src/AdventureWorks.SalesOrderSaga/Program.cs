using System.Text.Json;
using System.Text.Json.Serialization;
using AdventureWorks.SalesOrderSaga.Infrastructure;
using AdventureWorks.SalesOrderSaga.Persistence;
using Azure.Core.Serialization;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();
builder.Configuration.AddUserSecrets<Program>(optional: true);

var sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["SqlConnectionString"]
    ?? throw new InvalidOperationException("Missing required SQL connection configuration.");
builder.Services.AddDbContext<SalesOrderSagaDbContext>(options => options.UseSqlServer(sqlConnectionString));

var paymentBaseUrl = builder.Configuration["PaymentAuthorization:BaseUrl"]
    ?? throw new InvalidOperationException("Missing required configuration value 'PaymentAuthorization:BaseUrl'.");
builder.Services.AddHttpClient("PaymentAuthorization", client => client.BaseAddress = new Uri(paymentBaseUrl, UriKind.Absolute));

var serviceBusConnection = builder.Configuration["ServiceBusConnection"]
    ?? builder.Configuration.GetConnectionString("servicebus");
var serviceBusClient = serviceBusConnection is not null
    ? new ServiceBusClient(serviceBusConnection)
    : new ServiceBusClient(
        builder.Configuration["ServiceBusConnection:fullyQualifiedNamespace"]
            ?? throw new InvalidOperationException("Missing required Service Bus connection configuration."),
        new DefaultAzureCredential());
builder.Services.AddSingleton(serviceBusClient);
builder.Services.AddSingleton<ISalesOrderSagaEventPublisher, SalesOrderSagaEventPublisher>();
builder.Services.Configure<WorkerOptions>(options =>
{
    options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    });
});

builder.Build().Run();
