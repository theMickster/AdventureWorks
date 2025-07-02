using AdventureWorks.Application.Http;
using Microsoft.AspNetCore.JsonPatch;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;

namespace AdventureWorks.UnitTests.Application.Http;

/// <summary>
/// Unit tests for HttpRequestSender
/// Following xUnit, Moq, and FluentAssertions patterns
/// </summary>
public sealed class HttpRequestSenderTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly HttpRequestSender _httpRequestSender;

    public HttpRequestSenderTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://test.api.com")
        };
        _httpRequestSender = new HttpRequestSender(_httpClient);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullHttpClient_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new HttpRequestSender(null!));
        exception.ParamName.Should().Be("httpClient");
    }

    [Fact]
    public void Constructor_ValidHttpClient_CreatesInstance()
    {
        using var httpClient = new HttpClient();
        var sender = new HttpRequestSender(httpClient);
        sender.Should().NotBeNull();
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_NullUri_ThrowsArgumentNullException()
    {
        var authToken = "test-token";
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _httpRequestSender.GetAsync(null!, authToken));
        exception.ParamName.Should().Be("uri");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetAsync_NullOrEmptyAuthToken_ThrowsArgumentException(string? authToken)
    {
        var uri = new Uri("https://test.api.com/resource");
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _httpRequestSender.GetAsync(uri, authToken!));
        exception.ParamName.Should().Be("authToken");
    }

    [Fact]
    public async Task GetAsync_ValidParameters_SetsAuthorizationHeader()
    {
        var uri = new Uri("https://test.api.com/resource");
        var authToken = "test-bearer-token";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        await _httpRequestSender.GetAsync(uri, authToken);
        _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        _httpClient.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        _httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be(authToken);
    }

    [Fact]
    public async Task GetAsync_ValidParameters_CallsHttpClientGetAsync()
    {
        var uri = new Uri("https://test.api.com/resource");
        var authToken = "test-token";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == uri),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var result = await _httpRequestSender.GetAsync(uri, authToken);
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAsync_WithCancellationToken_PropagatesToken()
    {
        var uri = new Uri("https://test.api.com/resource");
        var authToken = "test-token";
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse)
            .Verifiable();
        var result = await _httpRequestSender.GetAsync(uri, authToken, cancellationToken);
        result.Should().NotBeNull();
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region PostAsync Tests

    [Fact]
    public async Task PostAsync_NullUri_ThrowsArgumentNullException()
    {
        var payload = new { Name = "Test" };
        var authToken = "test-token";
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _httpRequestSender.PostAsync(null!, payload, authToken));
        exception.ParamName.Should().Be("uri");
    }

    [Fact]
    public async Task PostAsync_NullAuthToken_ThrowsArgumentNullException()
    {
        var uri = new Uri("https://test.api.com/resource");
        var payload = new { Name = "Test" };
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _httpRequestSender.PostAsync(uri, payload, null!));
        exception.ParamName.Should().Be("authToken");
    }

    [Fact]
    public async Task PostAsync_ValidParameters_SetsAuthorizationHeader()
    {
        var uri = new Uri("https://test.api.com/resource");
        var payload = new { Name = "Test", Value = 123 };
        var authToken = "test-bearer-token";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.Created);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        await _httpRequestSender.PostAsync(uri, payload, authToken);
        _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        _httpClient.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        _httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be(authToken);
    }

    [Fact]
    public async Task PostAsync_ValidParameters_CallsHttpClientPostAsync()
    {
        var uri = new Uri("https://test.api.com/resource");
        var payload = new { Name = "Test", Value = 123 };
        var authToken = "test-token";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.Created);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == uri &&
                    req.Content != null &&
                    req.Content.Headers.ContentType!.MediaType == "application/json"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var result = await _httpRequestSender.PostAsync(uri, payload, authToken);
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostAsync_WithCancellationToken_PropagatesToken()
    {
        var uri = new Uri("https://test.api.com/resource");
        var payload = new { Name = "Test" };
        var authToken = "test-token";
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.Created);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse)
            .Verifiable();
        var result = await _httpRequestSender.PostAsync(uri, payload, authToken, cancellationToken);
        result.Should().NotBeNull();
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region PatchAsync Tests

    [Fact]
    public async Task PatchAsync_NullUri_ThrowsArgumentNullException()
    {
        var patchDoc = new JsonPatchDocument<TestModel>();
        var authToken = "test-token";
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _httpRequestSender.PatchAsync(null!, patchDoc, authToken));
        exception.ParamName.Should().Be("uri");
    }

    [Fact]
    public async Task PatchAsync_NullAuthToken_ThrowsArgumentNullException()
    {
        var uri = new Uri("https://test.api.com/resource");
        var patchDoc = new JsonPatchDocument<TestModel>();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _httpRequestSender.PatchAsync(uri, patchDoc, null!));
        exception.ParamName.Should().Be("authToken");
    }

    [Fact]
    public async Task PatchAsync_ValidParameters_SetsAuthorizationHeader()
    {
        var uri = new Uri("https://test.api.com/resource");
        var patchDoc = new JsonPatchDocument<TestModel>();
        patchDoc.Replace(x => x.Name, "Updated Name");
        var authToken = "test-bearer-token";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        await _httpRequestSender.PatchAsync(uri, patchDoc, authToken);
        _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        _httpClient.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        _httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be(authToken);
    }

    [Fact]
    public async Task PatchAsync_ValidParameters_CallsHttpClientPatchAsync()
    {
        var uri = new Uri("https://test.api.com/resource");
        var patchDoc = new JsonPatchDocument<TestModel>();
        patchDoc.Replace(x => x.Name, "Updated Name");
        var authToken = "test-token";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Patch &&
                    req.RequestUri == uri &&
                    req.Content != null &&
                    req.Content.Headers.ContentType!.MediaType == "application/json-patch+json"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var result = await _httpRequestSender.PatchAsync(uri, patchDoc, authToken);
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PatchAsync_WithCancellationToken_PropagatesToken()
    {
        var uri = new Uri("https://test.api.com/resource");
        var patchDoc = new JsonPatchDocument<TestModel>();
        var authToken = "test-token";
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse)
            .Verifiable();
        var result = await _httpRequestSender.PatchAsync(uri, patchDoc, authToken, cancellationToken);
        result.Should().NotBeNull();
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_NullUri_ThrowsArgumentNullException()
    {
        var authToken = "test-token";
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _httpRequestSender.DeleteAsync(null!, authToken));
        exception.ParamName.Should().Be("uri");
    }

    [Fact]
    public async Task DeleteAsync_NullAuthToken_ThrowsArgumentNullException()
    {
        var uri = new Uri("https://test.api.com/resource");
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _httpRequestSender.DeleteAsync(uri, null!));
        exception.ParamName.Should().Be("authToken");
    }

    [Fact]
    public async Task DeleteAsync_ValidParameters_SetsAuthorizationHeader()
    {
        var uri = new Uri("https://test.api.com/resource");
        var authToken = "test-bearer-token";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        await _httpRequestSender.DeleteAsync(uri, authToken);
        _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        _httpClient.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        _httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be(authToken);
    }

    [Fact]
    public async Task DeleteAsync_ValidParameters_CallsHttpClientDeleteAsync()
    {
        var uri = new Uri("https://test.api.com/resource");
        var authToken = "test-token";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri == uri),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
        var result = await _httpRequestSender.DeleteAsync(uri, authToken);
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAsync_WithCancellationToken_PropagatesToken()
    {
        var uri = new Uri("https://test.api.com/resource");
        var authToken = "test-token";
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse)
            .Verifiable();
        var result = await _httpRequestSender.DeleteAsync(uri, authToken, cancellationToken);
        result.Should().NotBeNull();
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region Test Helper Classes

    /// <summary>
    /// Test model for PATCH operations
    /// </summary>
    public sealed class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    #endregion
}
