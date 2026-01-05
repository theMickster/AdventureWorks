using System.Net.Http.Json;
using Aspire.Hosting.ApplicationModel;
using AdventureWorks.AppHost;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

const string SqlServerHealthCheckKey = "sql-server-tcp";
const int SqlServerPort = 1433;
const int AngularDevServerPort = 4200;
const string TopicName = "sales-order-events";
const string SagaSubscription = "sales-order-saga";
const string PaymentSubscription = "sales-order-payment-results";

var builder = DistributedApplication.CreateBuilder(args);
builder.Services.AddSingleton<IDistributedApplicationLifecycleHook, ExternalContainerLifecycleHook>();
builder.Services.AddHealthChecks().AddCheck(SqlServerHealthCheckKey, new TcpPortHealthCheck("localhost", SqlServerPort));

var sqlContainerName = builder.Configuration["SqlServer:ContainerName"] ?? "tosk-mssql";
builder.AddResource(new ExternalContainerResource(sqlContainerName))
    .WithAnnotation(new HealthCheckAnnotation(SqlServerHealthCheckKey));

var defaultConnection = builder.AddConnectionString("DefaultConnection");
var storage = builder.AddAzureStorage("functions-storage").RunAsEmulator();
var serviceBus = builder.AddAzureServiceBus("servicebus").RunAsEmulator();
var events = serviceBus.AddServiceBusTopic(TopicName);
events.AddServiceBusSubscription(SagaSubscription);
events.AddServiceBusSubscription(PaymentSubscription);

var harness = builder.AddProject<Projects.AdventureWorks_SalesOrderSaga_TestHarness>("saga-test-harness")
    .WithHttpEndpoint(name: "http")
    .WithReference(defaultConnection)
    .WithReference(serviceBus)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ServiceBusSalesOrderEventsTopicName", TopicName)
    .WithHttpHealthCheck("/health")
    .WaitFor(serviceBus);

var functions = builder.AddAzureFunctionsProject<Projects.AdventureWorks_SalesOrderSaga>("sales-order-functions")
    .WithHostStorage(storage)
    .WithReference(defaultConnection)
    .WithReference(serviceBus)
    .WithEnvironment("ServiceBusConnection", serviceBus)
    .WithEnvironment("ServiceBusSalesOrderEventsTopicName", TopicName)
    .WithEnvironment("ServiceBusSalesOrderSagaSubscriptionName", SagaSubscription)
    .WithEnvironment("ServiceBusSalesOrderPaymentSubscriptionName", PaymentSubscription)
    .WithEnvironment("PaymentAuthorization__BaseUrl", harness.GetEndpoint("http"))
    .WithExternalHttpEndpoints()
    .WaitFor(storage)
    .WaitFor(serviceBus);

harness.WithEnvironment("Functions__BaseUrl", functions.GetEndpoint("http"))
    .WaitFor(functions);
AddHarnessCommands(harness);

builder.AddProject<Projects.AdventureWorks_DbUp>("dbup")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithExplicitStart();

var api = builder.AddProject<Projects.AdventureWorks_API>("api")
    .WithReference(defaultConnection)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

builder.AddJavaScriptApp("angular-web", "../../../apps/angular-web", "start")
    .WithHttpEndpoint(port: AngularDevServerPort, name: "http", isProxied: false)
    .WithEnvironment("NODE_ENV", "development")
    .WaitFor(api);

builder.Build().Run();

static void AddHarnessCommands(IResourceBuilder<ProjectResource> harness)
{
    AddHttpCommand(harness, "start-order", "Start test order", HttpMethod.Post, "/test-control/orders", "{}", [Number("quantity", "Quantity", "1")]);
    AddHttpCommand(harness, "list-payments", "List payments", HttpMethod.Get, "/test-control/authorizations");
    AddHttpCommand(harness, "approve", "Approve payment", HttpMethod.Post, "/test-control/authorizations/{salesOrderId}/approve", "{}", [Number("salesOrderId", "Sales order ID")]);
    AddHttpCommand(harness, "decline", "Decline payment", HttpMethod.Post, "/test-control/authorizations/{salesOrderId}/decline", "{\"reason\":\"manual smoke decline\"}", [Number("salesOrderId", "Sales order ID")]);
    AddHttpCommand(harness, "saga-status", "Check saga status", HttpMethod.Get, "/test-control/sagas/{salesOrderId}", arguments: [Number("salesOrderId", "Sales order ID")]);
    AddHttpCommand(harness, "scenario-approved", "Run approved scenario", HttpMethod.Post, "/test-control/scenarios/approved", "{}");
    AddHttpCommand(harness, "scenario-declined", "Run declined scenario", HttpMethod.Post, "/test-control/scenarios/declined", "{}");
    AddHttpCommand(harness, "cleanup", "Delete harness fixtures", HttpMethod.Delete, "/test-control/fixtures?confirm=true", confirmation: "Delete every database fixture whose comment starts with AW-SAGA-HARNESS:? This cannot be undone.");
}

static InteractionInput Number(string name, string label, string? value = null) => new()
{
    Name = name, Label = label, InputType = InputType.Number, Required = value is null, Value = value
};

static void AddHttpCommand(
    IResourceBuilder<ProjectResource> harness,
    string name,
    string displayName,
    HttpMethod method,
    string path,
    string? body = null,
    IReadOnlyList<InteractionInput>? arguments = null,
    string? confirmation = null)
{
    harness.WithCommand(name, displayName, async context =>
    {
        var requestPath = path;
        var requestBody = body;
        var url = await harness.GetEndpoint("http").GetValueAsync(context.CancellationToken)
            ?? throw new InvalidOperationException("The test harness HTTP endpoint is not allocated.");
        foreach (var argument in context.Arguments)
            requestPath = requestPath.Replace($"{{{argument.Name}}}", Uri.EscapeDataString(argument.Value ?? string.Empty), StringComparison.Ordinal);
        var quantity = context.Arguments.GetString("quantity");
        if (name == "start-order" && quantity is not null)
            requestBody = $"{{\"quantity\":{quantity},\"unitPrice\":2024.994}}";
        using var client = new HttpClient { BaseAddress = new Uri(url) };
        using var request = new HttpRequestMessage(method, requestPath);
        if (requestBody is not null) request.Content = JsonContent.Create(System.Text.Json.JsonDocument.Parse(requestBody).RootElement);
        using var response = await client.SendAsync(request, context.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(context.CancellationToken);
        return response.IsSuccessStatusCode
            ? CommandResults.Success("Harness command completed.", json, CommandResultFormat.Json)
            : CommandResults.Failure($"Harness returned {(int)response.StatusCode}.", json, CommandResultFormat.Json);
    }, new CommandOptions
    {
        Description = displayName,
        Arguments = arguments ?? [],
        ConfirmationMessage = confirmation,
        UpdateState = context =>
            context.ResourceSnapshot.State?.Text == KnownResourceStates.Running && context.ResourceSnapshot.HealthStatus == HealthStatus.Healthy
                ? ResourceCommandState.Enabled
                : ResourceCommandState.Disabled
    });
}
