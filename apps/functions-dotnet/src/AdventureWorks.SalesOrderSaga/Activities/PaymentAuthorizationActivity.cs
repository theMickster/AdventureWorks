using System.Net.Http.Json;
using AdventureWorks.SalesOrderSaga.Models;
using AdventureWorks.SalesOrderSaga.Contracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace AdventureWorks.SalesOrderSaga.Activities;

/// <summary>Performs the one idempotent outbound authorization request for a reserved order.</summary>
public sealed class PaymentAuthorizationActivityCore(IHttpClientFactory httpClientFactory)
    : TaskActivity<PaymentAuthorizationRequest, PaymentAuthorizationAcknowledgement>
{
    public override async Task<PaymentAuthorizationAcknowledgement> RunAsync(TaskActivityContext context, PaymentAuthorizationRequest? input)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);

        using var request = new HttpRequestMessage(HttpMethod.Post, "authorizations")
        {
            Content = JsonContent.Create(input)
        };
        request.Headers.Add("Idempotency-Key", input.IdempotencyKey);

        using var response = await httpClientFactory.CreateClient("PaymentAuthorization")
            .SendAsync(request, CancellationToken.None);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PaymentAuthorizationAcknowledgement>(CancellationToken.None)
            ?? throw new InvalidOperationException("Payment authorization service returned an empty acknowledgement.");
    }
}

/// <summary>Functions adapter for <see cref="PaymentAuthorizationActivityCore"/>.</summary>
public sealed class PaymentAuthorizationActivity(IHttpClientFactory httpClientFactory)
{
    [Function(nameof(PaymentAuthorizationActivity))]
    public Task<PaymentAuthorizationAcknowledgement> RunAsync([ActivityTrigger] PaymentAuthorizationRequest input) =>
        new PaymentAuthorizationActivityCore(httpClientFactory).RunAsync(
            new FunctionsTaskActivityContext(nameof(PaymentAuthorizationActivity)), input);
}
