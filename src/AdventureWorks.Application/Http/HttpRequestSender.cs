using Microsoft.AspNetCore.JsonPatch;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AdventureWorks.Application.PersistenceContracts.Http;

namespace AdventureWorks.Application.Http;
public sealed class HttpRequestSender : IHttpRequestSender
{
    private readonly HttpClient _httpClient;

    public HttpRequestSender(
        HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Send a GET request to the specified Uri as an asynchronous operation.
    /// </summary>
    /// <param name="uri">The Uri the request is sent to.</param>
    /// <param name="authToken">The GTP Authorization token.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns></returns>
    public Task<HttpResponseMessage> GetAsync(Uri uri, string authToken, CancellationToken cancellationToken = default)
    {
        if (uri is null)
        {
            throw new ArgumentNullException(nameof(uri));
        }

        if (string.IsNullOrEmpty(authToken))
        {
            throw new ArgumentException($"'{nameof(authToken)}' cannot be null or empty.", nameof(authToken));
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        return _httpClient.GetAsync(uri, cancellationToken);
    }

    /// <summary>
    /// Send a POST request to the specified Uri as an asynchronous operation.
    /// </summary>
    /// <param name="uri">The Uri the request is sent to.</param>
    /// <param name="payload">The HTTP request content sent to the server.</param>
    /// <param name="authToken">The GTP Authorization token.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns></returns>
    public Task<HttpResponseMessage> PostAsync(Uri uri, object payload, string authToken, CancellationToken cancellationToken = default)
    {
        if (uri is null)
        {
            throw new ArgumentNullException(nameof(uri));
        }

        if (authToken is null)
        {
            throw new ArgumentNullException(nameof(authToken));
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        return _httpClient.PostAsync(uri, content, cancellationToken);
    }

    /// <summary>
    /// Sends a PATCH request to a Uri designated as a string as an asynchronous operation.
    /// </summary>
    /// <typeparam name="T">the type to update</typeparam>
    /// <param name="uri">The Uri the request is sent to.</param>
    /// <param name="jsonPatchDocument">The HTTP request content sent to the server.</param>
    /// <param name="authToken">The GTP Authorization token.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns></returns>
    public Task<HttpResponseMessage> PatchAsync<T>(Uri uri, JsonPatchDocument<T> jsonPatchDocument, string authToken, CancellationToken cancellationToken = default) where T : class
    {
        if (uri is null)
        {
            throw new ArgumentNullException(nameof(uri));
        }

        if (authToken is null)
        {
            throw new ArgumentNullException(nameof(authToken));
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        var content = new StringContent(JsonSerializer.Serialize(jsonPatchDocument), Encoding.UTF8, "application/json-patch+json");
        return _httpClient.PatchAsync(uri, content, cancellationToken);
    }

    /// <summary>
    /// Send a DELETE request to the specified Uri as an asynchronous operation.
    /// </summary>
    /// <param name="uri">The Uri the request is sent to.</param>
    /// <param name="authToken">The GTP Authorization token.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns></returns>
    public Task<HttpResponseMessage> DeleteAsync(Uri uri, string authToken, CancellationToken cancellationToken = default)
    {
        if (uri is null)
        {
            throw new ArgumentNullException(nameof(uri));
        }

        if (authToken is null)
        {
            throw new ArgumentNullException(nameof(authToken));
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        return _httpClient.DeleteAsync(uri, cancellationToken);
    }
}
