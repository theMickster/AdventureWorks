using System.Net;
using AdventureWorks.Functions.SalesOrderSaga.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;

namespace AdventureWorks.Functions.SalesOrderSaga.Functions;

/// <summary>
/// HTTP-testable read of a sales order saga's current state — the first HTTP surface for this
/// app (US 807). Resolves the durable instance ID via <see cref="SalesOrderSagaStarter.BuildInstanceId"/>
/// so it never has to reimplement the format, then reports the instance's runtime status and,
/// once completed, its <see cref="SalesOrderSagaResult"/>. Returns 404 when no saga has ever
/// run for the given <c>salesOrderId</c>.
/// </summary>
public static class SalesOrderSagaStatusFunction
{
    [Function(nameof(SalesOrderSagaStatusFunction))]
    public static async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "saga-status/{salesOrderId:int}")] HttpRequestData request,
        [DurableClient] DurableTaskClient durableTaskClient,
        int salesOrderId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(durableTaskClient);

        var instanceId = SalesOrderSagaStarter.BuildInstanceId(salesOrderId);

        var metadata = await durableTaskClient.GetInstanceAsync(instanceId, getInputsAndOutputs: true, cancellationToken);
        if (metadata is null)
        {
            var notFound = request.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteAsJsonAsync(
                new { message = $"No sales order saga found for SalesOrderId {salesOrderId}." },
                cancellationToken);
            return notFound;
        }

        var result = metadata.IsCompleted ? metadata.ReadOutputAs<SalesOrderSagaResult>() : null;

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(
            new SalesOrderSagaStatusResponse(instanceId, metadata.RuntimeStatus.ToString(), result),
            cancellationToken);
        return response;
    }
}
