using Microsoft.AspNetCore.JsonPatch;

namespace AdventureWorks.Application.PersistenceContracts.Http;

public interface IHttpRequestSender
{
    /// <summary>
    /// Send a GET request to the specified Uri as an asynchronous operation.
    /// </summary>
    /// <param name="uri">The Uri the request is sent to.</param>
    /// <param name="authToken">The GTP Authorization token.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns></returns>
    Task<HttpResponseMessage> GetAsync(Uri uri, string authToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a POST request to the specified Uri as an asynchronous operation.
    /// </summary>
    /// <param name="uri">The Uri the request is sent to.</param>
    /// <param name="payload">The HTTP request content sent to the server.</param>
    /// <param name="authToken">The GTP Authorization token.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns></returns>
    Task<HttpResponseMessage> PostAsync(Uri uri, object payload, string authToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PATCH request to a Uri designated as a string as an asynchronous operation.
    /// </summary>
    /// <typeparam name="T">the type to update</typeparam>
    /// <param name="uri">The Uri the request is sent to.</param>
    /// <param name="jsonPatchDocument">The HTTP request content sent to the server.</param>
    /// <param name="authToken">The GTP Authorization token.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns></returns>
    Task<HttpResponseMessage> PatchAsync<T>(Uri uri, JsonPatchDocument<T> jsonPatchDocument, string authToken, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Send a DELETE request to the specified Uri as an asynchronous operation.
    /// </summary>
    /// <param name="uri">The Uri the request is sent to.</param>
    /// <param name="authToken">The GTP Authorization token.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns></returns>
    Task<HttpResponseMessage> DeleteAsync(Uri uri, string authToken, CancellationToken cancellationToken = default);

}
