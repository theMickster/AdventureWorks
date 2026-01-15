using System.Net;
using System.Text.Json;
using AdventureWorks.SalesOrderSaga.Functions;
using AdventureWorks.SalesOrderSaga.Models;
using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace AdventureWorks.SalesOrderSaga.UnitTests.Functions;

/// <summary>Coverage for the saga status HTTP read, using minimal fake HTTP data types.</summary>
public sealed class SalesOrderSagaStatusFunctionTests
{
    private const int SalesOrderId = 71774;
    private static readonly string InstanceId = SalesOrderSagaStarter.BuildInstanceId(SalesOrderId);

    [Fact]
    public async Task RunAsync_WhenNoInstanceExists_Returns404()
    {
        var client = new Mock<DurableTaskClient>("test");
        client.Setup(c => c.GetInstanceAsync(InstanceId, true, It.IsAny<CancellationToken>())).ReturnsAsync((OrchestrationMetadata?)null);

        var response = await SalesOrderSagaStatusFunction.RunAsync(NewRequest(), client.Object, SalesOrderId, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(SalesOrderId.ToString(), ReadBody(response));
    }

    [Fact]
    public async Task RunAsync_WhenRunning_Returns200WithoutResult()
    {
        var client = new Mock<DurableTaskClient>("test");
        client.Setup(c => c.GetInstanceAsync(InstanceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrchestrationMetadata("SalesOrderSagaOrchestrator", InstanceId) { RuntimeStatus = OrchestrationRuntimeStatus.Running });

        var response = await SalesOrderSagaStatusFunction.RunAsync(NewRequest(), client.Object, SalesOrderId, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = JsonDocument.Parse(ReadBody(response)).RootElement;
        Assert.Equal(InstanceId, body.GetProperty("instanceId").GetString());
        Assert.Equal("Running", body.GetProperty("runtimeStatus").GetString());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("result").ValueKind);
    }

    [Fact]
    public async Task RunAsync_WhenCompleted_ReturnsTerminalResult()
    {
        var result = SalesOrderSagaResult.PaymentDeclined(SalesOrderId, new ReservationReceipt(SalesOrderId, [776], DateTimeOffset.UtcNow), "Card Declined");
        var client = new Mock<DurableTaskClient>("test");
        client.Setup(c => c.GetInstanceAsync(InstanceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrchestrationMetadata("SalesOrderSagaOrchestrator", InstanceId)
            {
                RuntimeStatus = OrchestrationRuntimeStatus.Completed,
                SerializedOutput = JsonSerializer.Serialize(result),
                DataConverter = Microsoft.DurableTask.Converters.JsonDataConverter.Default
            });

        var response = await SalesOrderSagaStatusFunction.RunAsync(NewRequest(), client.Object, SalesOrderId, TestContext.Current.CancellationToken);

        var body = JsonDocument.Parse(ReadBody(response)).RootElement;
        Assert.Equal("Completed", body.GetProperty("runtimeStatus").GetString());
        Assert.Equal("Card Declined", body.GetProperty("result").GetProperty("failureReason").GetString());
    }

    private static string ReadBody(HttpResponseData response)
    {
        response.Body.Position = 0;
        return new StreamReader(response.Body).ReadToEnd();
    }

    private static HttpRequestData NewRequest()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOptions<WorkerOptions>>(Options.Create(new WorkerOptions
        {
            Serializer = new JsonObjectSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web))
        }));
        var context = new Mock<FunctionContext>();
        context.SetupGet(c => c.InstanceServices).Returns(services.BuildServiceProvider());
        return new FakeRequest(context.Object);
    }

    private sealed class FakeRequest(FunctionContext context) : HttpRequestData(context)
    {
        public override Stream Body { get; } = new MemoryStream();
        public override HttpHeadersCollection Headers { get; } = new();
        public override IReadOnlyCollection<IHttpCookie> Cookies { get; } = [];
        public override Uri Url { get; } = new("http://localhost/api/saga-status/71774");
        public override IEnumerable<System.Security.Claims.ClaimsIdentity> Identities { get; } = [];
        public override string Method => "GET";
        public override HttpResponseData CreateResponse() => new FakeResponse(FunctionContext);
    }

    private sealed class FakeResponse(FunctionContext context) : HttpResponseData(context)
    {
        public override HttpStatusCode StatusCode { get; set; }
        public override HttpHeadersCollection Headers { get; set; } = new();
        public override Stream Body { get; set; } = new MemoryStream();
        public override HttpCookies Cookies => throw new NotSupportedException();
    }
}
