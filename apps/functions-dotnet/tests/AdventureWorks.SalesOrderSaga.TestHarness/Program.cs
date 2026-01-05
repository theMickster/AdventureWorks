using AdventureWorks.SalesOrderSaga.Contracts;
using AdventureWorks.SalesOrderSaga.TestHarness;
using Azure.Messaging.ServiceBus;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException("The sales-order saga test harness can run only in Development.");

builder.Services.AddSingleton<IAuthorizationStore, AuthorizationStore>();
builder.Services.AddSingleton<MonotonicOrderIdAllocator>();
builder.Services.AddSingleton<IFixtureStore, SqlFixtureStore>();
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connection = configuration.GetConnectionString("servicebus") ?? configuration["ServiceBusConnection"]
        ?? throw new InvalidOperationException("A Service Bus emulator connection is required.");
    return new ServiceBusClient(connection);
});
builder.Services.AddSingleton<ISagaEventPublisher, ServiceBusSagaEventPublisher>();
builder.Services.AddHttpClient<ISagaStatusClient, SagaStatusClient>((sp, client) =>
{
    var url = sp.GetRequiredService<IConfiguration>()["Functions:BaseUrl"]
        ?? throw new InvalidOperationException("Functions:BaseUrl is required.");
    client.BaseAddress = new Uri(url.EndsWith('/') ? url : url + "/");
});
builder.Services.AddScoped<ScenarioRunner>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapPost("/authorizations", (PaymentAuthorizationRequest request, HttpRequest http, IAuthorizationStore store) =>
{
    try { return Results.Ok(store.Receive(request, http.Headers["Idempotency-Key"].FirstOrDefault())); }
    catch (HarnessConflictException ex) { return Results.Conflict(new { error = ex.Message }); }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

app.MapGet("/test-control/authorizations", (IAuthorizationStore store) => Results.Ok(store.List()));

app.MapPost("/test-control/orders", async (CreateOrderRequest? request, IFixtureStore fixtures, ISagaEventPublisher publisher, CancellationToken ct) =>
{
    var fixture = await fixtures.CreateAsync(request ?? new(), ct);
    await publisher.PublishOrderCreatedAsync(fixture, ct);
    return Results.Ok(fixture);
});

app.MapPost("/test-control/authorizations/{salesOrderId:int}/approve", (int salesOrderId, PaymentDecisionRequest? request, IAuthorizationStore store, ISagaEventPublisher publisher, CancellationToken ct) =>
    DecideAsync(salesOrderId, AuthorizationState.Approved, request?.AuthorizationCode, store, publisher, ct));
app.MapPost("/test-control/authorizations/{salesOrderId:int}/decline", (int salesOrderId, PaymentDecisionRequest? request, IAuthorizationStore store, ISagaEventPublisher publisher, CancellationToken ct) =>
    DecideAsync(salesOrderId, AuthorizationState.Declined, request?.Reason, store, publisher, ct));

app.MapGet("/test-control/sagas/{salesOrderId:int}", async (int salesOrderId, ISagaStatusClient statuses, IFixtureStore fixtures, CancellationToken ct) =>
{
    var status = await statuses.GetAsync(salesOrderId, ct);
    var evidence = await fixtures.GetEvidenceAsync(salesOrderId, ct);
    return status is null && evidence is null ? Results.NotFound() : Results.Ok(new { status, evidence });
});

app.MapPost("/test-control/scenarios/approved", (ScenarioRunner runner, CancellationToken ct) => runner.RunAsync(AuthorizationState.Approved, ct));
app.MapPost("/test-control/scenarios/declined", (ScenarioRunner runner, CancellationToken ct) => runner.RunAsync(AuthorizationState.Declined, ct));
app.MapDelete("/test-control/fixtures", async (bool confirm, IFixtureStore fixtures, CancellationToken ct) =>
    !confirm ? Results.BadRequest(new { error = "Set confirm=true. Only AW-SAGA-HARNESS marked fixtures are deleted." }) : Results.Ok(new { deletedFixtures = await fixtures.CleanupAsync(ct) }));

app.Run();

static async Task<IResult> DecideAsync(int salesOrderId, AuthorizationState decision, string? detail, IAuthorizationStore store, ISagaEventPublisher publisher, CancellationToken ct)
{
    try
    {
        var authorization = store.Decide(salesOrderId, decision, detail);
        await publisher.PublishPaymentResultAsync(authorization, ct);
        return Results.Ok(authorization);
    }
    catch (HarnessConflictException ex) { return Results.Conflict(new { error = ex.Message }); }
    catch (HarnessNotReadyException ex) { return Results.BadRequest(new { error = ex.Message }); }
}

public partial class Program;
