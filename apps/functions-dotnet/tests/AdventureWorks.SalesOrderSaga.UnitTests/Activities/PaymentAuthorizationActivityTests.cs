using System.Net;
using System.Net.Http.Json;
using AdventureWorks.SalesOrderSaga.Activities;
using AdventureWorks.SalesOrderSaga.Contracts;
using Microsoft.DurableTask;
using Moq;

namespace AdventureWorks.SalesOrderSaga.UnitTests.Activities;

/// <summary>Coverage for the single outbound payment-authorization request.</summary>
public sealed class PaymentAuthorizationActivityTests
{
    private static readonly PaymentAuthorizationRequest Request = new(
        71774, 69.98m, "USD", "sales-order-saga-71774", [new SagaOrderLine(707, 2, 34.99m)]);

    [Fact]
    public async Task RunAsync_PostsRequestWithIdempotencyKey_AndReturnsAcknowledgement()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new PaymentAuthorizationAcknowledgement("auth-1"))
        });
        var activity = new PaymentAuthorizationActivityCore(Factory(handler));

        var acknowledgement = await activity.RunAsync(new Mock<TaskActivityContext>().Object, Request);

        Assert.Equal("auth-1", acknowledgement.AuthorizationRequestId);
        var sent = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, sent.Method);
        Assert.Equal("/authorizations", sent.RequestUri!.AbsolutePath);
        Assert.Equal(Request.IdempotencyKey, Assert.Single(sent.Headers.GetValues("Idempotency-Key")));
        Assert.Equal(Request.SalesOrderId, sent.Body!.SalesOrderId);
        Assert.Equal(Request.Amount, sent.Body.Amount);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task RunAsync_OnNonSuccessStatus_Throws(HttpStatusCode status)
    {
        var activity = new PaymentAuthorizationActivityCore(Factory(new StubHandler(_ => new HttpResponseMessage(status))));

        await Assert.ThrowsAsync<HttpRequestException>(() => activity.RunAsync(new Mock<TaskActivityContext>().Object, Request));
    }

    [Fact]
    public async Task RunAsync_OnEmptyBody_Throws()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create<PaymentAuthorizationAcknowledgement?>(null) });
        var activity = new PaymentAuthorizationActivityCore(Factory(handler));

        await Assert.ThrowsAsync<InvalidOperationException>(() => activity.RunAsync(new Mock<TaskActivityContext>().Object, Request));
    }

    [Fact]
    public async Task RunAsync_WhenInputIsNull_Throws()
    {
        var activity = new PaymentAuthorizationActivityCore(Factory(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK))));

        await Assert.ThrowsAsync<ArgumentNullException>(() => activity.RunAsync(new Mock<TaskActivityContext>().Object, null));
    }

    private static IHttpClientFactory Factory(StubHandler handler)
    {
        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateClient("PaymentAuthorization"))
            .Returns(() => new HttpClient(handler) { BaseAddress = new Uri("http://payments.test/") });
        return factory.Object;
    }

    private sealed record CapturedRequest(HttpMethod Method, Uri? RequestUri, System.Net.Http.Headers.HttpRequestHeaders Headers, PaymentAuthorizationRequest? Body);

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<CapturedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? null : await request.Content.ReadFromJsonAsync<PaymentAuthorizationRequest>(cancellationToken);
            Requests.Add(new CapturedRequest(request.Method, request.RequestUri, request.Headers, body));
            return respond(request);
        }
    }
}
