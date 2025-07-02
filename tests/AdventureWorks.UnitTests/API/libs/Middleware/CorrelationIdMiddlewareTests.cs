using AdventureWorks.API.libs.Middleware;
using AdventureWorks.Application.Interfaces;
using AdventureWorks.Common.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.API.libs.Middleware;

public sealed class CorrelationIdMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _loggerMock;
    private readonly Mock<ICorrelationIdAccessor> _correlationIdAccessorMock;

    public CorrelationIdMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
        _correlationIdAccessorMock = new Mock<ICorrelationIdAccessor>();
    }

    [Fact]
    public void Constructor_NullNext_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new CorrelationIdMiddleware(null!, _loggerMock.Object));
        Assert.Equal("next", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new CorrelationIdMiddleware(_nextMock.Object, null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public async Task InvokeAsync_NullContext_ThrowsArgumentNullException()
    {
        var middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await middleware.InvokeAsync(null!, _correlationIdAccessorMock.Object));
    }

    [Fact]
    public async Task InvokeAsync_NullCorrelationIdAccessor_ThrowsArgumentNullException()
    {
        var middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);
        var context = new DefaultHttpContext();
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await middleware.InvokeAsync(context, null!));
    }

    [Fact]
    public async Task InvokeAsync_NoCorrelationIdInRequest_GeneratesNewId()
    {
        var middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);
        var context = new DefaultHttpContext();
        string? capturedCorrelationId = null;

        _correlationIdAccessorMock
            .Setup(x => x.SetCorrelationId(It.IsAny<string>()))
            .Callback<string>(id => capturedCorrelationId = id);
        await middleware.InvokeAsync(context, _correlationIdAccessorMock.Object);
        Assert.NotNull(capturedCorrelationId);
        Assert.True(Guid.TryParse(capturedCorrelationId, out _));
        _correlationIdAccessorMock.Verify(x => x.SetCorrelationId(It.IsAny<string>()), Times.Once);
        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ExistingCorrelationIdInRequest_UsesProvidedId()
    {
        var middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);
        var context = new DefaultHttpContext();
        var expectedCorrelationId = "existing-correlation-id-123";
        context.Request.Headers.Append(ConfigurationConstants.CorrelationIdHeaderName, expectedCorrelationId);

        string? capturedCorrelationId = null;
        _correlationIdAccessorMock
            .Setup(x => x.SetCorrelationId(It.IsAny<string>()))
            .Callback<string>(id => capturedCorrelationId = id);
        await middleware.InvokeAsync(context, _correlationIdAccessorMock.Object);
        Assert.Equal(expectedCorrelationId, capturedCorrelationId);
        _correlationIdAccessorMock.Verify(x => x.SetCorrelationId(expectedCorrelationId), Times.Once);
        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_SetsCorrelationIdForResponse()
    {
        var expectedCorrelationId = "test-correlation-id";
        var context = new DefaultHttpContext();
        context.Request.Headers.Append(ConfigurationConstants.CorrelationIdHeaderName, expectedCorrelationId);

        _correlationIdAccessorMock
            .Setup(x => x.SetCorrelationId(expectedCorrelationId));

        var middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);
        await middleware.InvokeAsync(context, _correlationIdAccessorMock.Object);

        _correlationIdAccessorMock.Verify(x => x.SetCorrelationId(expectedCorrelationId), Times.Once);
        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextDelegate()
    {
        var middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);
        var context = new DefaultHttpContext();
        await middleware.InvokeAsync(context, _correlationIdAccessorMock.Object);
        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task InvokeAsync_EmptyOrWhitespaceCorrelationId_GeneratesNewId(string emptyCorrelationId)
    {
        var middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers.Append(ConfigurationConstants.CorrelationIdHeaderName, emptyCorrelationId);

        string? capturedCorrelationId = null;
        _correlationIdAccessorMock
            .Setup(x => x.SetCorrelationId(It.IsAny<string>()))
            .Callback<string>(id => capturedCorrelationId = id);
        await middleware.InvokeAsync(context, _correlationIdAccessorMock.Object);
        Assert.NotNull(capturedCorrelationId);
        Assert.NotEqual(emptyCorrelationId, capturedCorrelationId);
        Assert.True(Guid.TryParse(capturedCorrelationId, out _));
    }
}
