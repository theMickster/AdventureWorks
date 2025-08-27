using AdventureWorks.API.libs.Middleware;
using AdventureWorks.Application.Interfaces;
using AdventureWorks.Common.Constants;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.API.libs.Middleware;

[ExcludeFromCodeCoverage]
public sealed class ExceptionHandlerMiddlewareTests : UnitTestBase
{
    private const string TestCorrelationId = "test-cid-123";
    private const string SanitizedErrorMessage = "An unexpected error occurred.";
    private const string LeakySecretMessage =
        "Failed to retrieve newly created store address for StoreId=292, AddressId=985, AddressTypeId=3.";

    private readonly Mock<ILogger<ExceptionHandlerMiddleware>> _loggerMock;
    private readonly Mock<ICorrelationIdAccessor> _correlationIdAccessorMock;

    public ExceptionHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionHandlerMiddleware>>();
        _correlationIdAccessorMock = new Mock<ICorrelationIdAccessor>();
        _correlationIdAccessorMock.SetupGet(x => x.CorrelationId).Returns(TestCorrelationId);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    [Fact]
    public void Constructor_NullNext_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ExceptionHandlerMiddleware(null!, _loggerMock.Object));
        Assert.Equal("next", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ExceptionHandlerMiddleware(next, null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public async Task Invoke_NullContext_ThrowsArgumentNullException()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new ExceptionHandlerMiddleware(next, _loggerMock.Object);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await middleware.Invoke(null!, _correlationIdAccessorMock.Object));
        Assert.Equal("context", exception.ParamName);
    }

    [Fact]
    public async Task Invoke_NullCorrelationIdAccessor_ThrowsArgumentNullException()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new ExceptionHandlerMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await middleware.Invoke(context, null!));
        Assert.Equal("correlationIdAccessor", exception.ParamName);
    }

    [Fact]
    public async Task Invoke_UnknownExceptionType_DoesNotEchoExceptionMessage()
    {
        RequestDelegate next = _ => throw new InvalidOperationException(LeakySecretMessage);
        var middleware = new ExceptionHandlerMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.Invoke(context, _correlationIdAccessorMock.Object);

        var body = await ReadResponseBodyAsync(context);

        using (new AssertionScope())
        {
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            body.Should().NotContain("Failed to retrieve");
            body.Should().NotContain("StoreId=292");
            body.Should().NotContain("AddressId=985");
            body.Should().NotContain("AddressTypeId=3");
            body.Should().Contain(SanitizedErrorMessage);
            body.Should().Contain(TestCorrelationId);
            context.Response.Headers[ConfigurationConstants.CorrelationIdHeaderName]
                .ToString().Should().Be(TestCorrelationId);

            // Half of the security-vs-debuggability contract documented on
            // GenericServerErrorMessage and in CLAUDE.md: the original exception
            // (with its full Message) MUST reach the structured log even though it's
            // sanitized out of the response body. Verifying the call locks in operator
            // visibility into 500s — without this, a future "log noise cleanup" could
            // silently destroy diagnostic context while every body assertion still passes.
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex is InvalidOperationException && ex.Message == LeakySecretMessage),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task Invoke_ValidationException_EchoesValidationErrors()
    {
        var failure = new ValidationFailure("Name", "Name is required.")
        {
            ErrorCode = "Rule-01"
        };
        RequestDelegate next = _ => throw new ValidationException(new[] { failure });
        var middleware = new ExceptionHandlerMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.Invoke(context, _correlationIdAccessorMock.Object);

        var body = await ReadResponseBodyAsync(context);

        using (new AssertionScope())
        {
            context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            body.Should().Contain("propertyName");
            body.Should().Contain("errorCode");
            body.Should().Contain("errorMessage");
            body.Should().Contain("correlationId");
            body.Should().Contain("Name is required.");
            body.Should().Contain("Rule-01");
            body.Should().Contain(TestCorrelationId);
        }
    }

    [Fact]
    public async Task Invoke_KeyNotFoundException_EchoesMessage()
    {
        const string notFoundMessage = "Some lookup not found.";
        RequestDelegate next = _ => throw new KeyNotFoundException(notFoundMessage);
        var middleware = new ExceptionHandlerMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.Invoke(context, _correlationIdAccessorMock.Object);

        var body = await ReadResponseBodyAsync(context);

        using (new AssertionScope())
        {
            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            body.Should().Contain(notFoundMessage);
            body.Should().Contain("correlationId");
            body.Should().Contain("timestamp");
            body.Should().Contain(TestCorrelationId);
        }
    }

    [Fact]
    public async Task Invoke_UnknownException_BodyContainsCorrelationIdAndTimestamp()
    {
        RequestDelegate next = _ => throw new InvalidOperationException("anything at all");
        var middleware = new ExceptionHandlerMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.Invoke(context, _correlationIdAccessorMock.Object);

        var body = await ReadResponseBodyAsync(context);

        using (new AssertionScope())
        {
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            body.Should().Contain("correlationId");
            body.Should().Contain("timestamp");
            body.Should().Contain(SanitizedErrorMessage);
            body.Should().Contain(TestCorrelationId);
        }
    }
}
