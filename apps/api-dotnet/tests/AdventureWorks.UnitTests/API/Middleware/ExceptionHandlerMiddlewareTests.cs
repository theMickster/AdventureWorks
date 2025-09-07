using AdventureWorks.API.libs.Middleware;
using AdventureWorks.Application.Exceptions;
using AdventureWorks.Application.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AdventureWorks.UnitTests.API.Middleware;

public sealed class ExceptionHandlerMiddlewareTests : UnitTestBase
{
    private readonly Mock<ILogger<ExceptionHandlerMiddleware>> _mockLogger = new();
    private readonly Mock<ICorrelationIdAccessor> _mockCorrelationIdAccessor = new();
    private readonly Mock<RequestDelegate> _mockNext = new();
    private readonly ExceptionHandlerMiddleware _sut;

    public ExceptionHandlerMiddlewareTests()
    {
        _mockCorrelationIdAccessor.Setup(x => x.CorrelationId).Returns("test-correlation-id");
        _sut = new ExceptionHandlerMiddleware(_mockNext.Object, _mockLogger.Object);
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await new StreamReader(context.Response.Body).ReadToEndAsync();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenNextIsNull()
    {
        var act = () => new ExceptionHandlerMiddleware(null!, _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("next");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new ExceptionHandlerMiddleware(_mockNext.Object, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task Invoke_ThrowsArgumentNullException_WhenContextIsNull()
    {
        var act = async () => await _sut.Invoke(null!, _mockCorrelationIdAccessor.Object);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Invoke_ThrowsArgumentNullException_WhenCorrelationIdAccessorIsNull()
    {
        var act = async () => await _sut.Invoke(CreateContext(), null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Invoke_CallsNext_WhenNoExceptionThrown()
    {
        var context = CreateContext();
        _mockNext.Setup(x => x(context)).Returns(Task.CompletedTask);

        await _sut.Invoke(context, _mockCorrelationIdAccessor.Object);

        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task Invoke_Returns500_WhenUnhandledExceptionThrown()
    {
        var context = CreateContext();
        _mockNext.Setup(x => x(context)).ThrowsAsync(new InvalidOperationException("internal detail"));

        await _sut.Invoke(context, _mockCorrelationIdAccessor.Object);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Invoke_DoesNotExposeExceptionMessage_WhenUnhandledExceptionThrown()
    {
        var context = CreateContext();
        _mockNext.Setup(x => x(context)).ThrowsAsync(new InvalidOperationException("secret internal detail"));

        await _sut.Invoke(context, _mockCorrelationIdAccessor.Object);

        var body = await ReadResponseBodyAsync(context);
        body.Should().NotContain("secret internal detail",
            "raw exception messages must not be exposed in the response");
    }

    [Fact]
    public async Task Invoke_Returns500WithExpectedShape_WhenUnhandledExceptionThrown()
    {
        var context = CreateContext();
        _mockNext.Setup(x => x(context)).ThrowsAsync(new InvalidOperationException("internal detail"));

        await _sut.Invoke(context, _mockCorrelationIdAccessor.Object);

        var body = await ReadResponseBodyAsync(context);
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        using (new AssertionScope())
        {
            root.TryGetProperty("error", out var errorProp).Should().BeTrue();
            root.TryGetProperty("correlationId", out _).Should().BeTrue();
            root.TryGetProperty("timestamp", out _).Should().BeTrue();
            errorProp.GetString().Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Invoke_Returns400_WhenValidationExceptionThrown()
    {
        var context = CreateContext();
        var failures = new[] { new ValidationFailure("Name", "Name is required") { ErrorCode = "Rule-01" } };
        _mockNext.Setup(x => x(context)).ThrowsAsync(new ValidationException(failures));

        await _sut.Invoke(context, _mockCorrelationIdAccessor.Object);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Invoke_Returns400WithValidationErrorShape_WhenValidationExceptionThrown()
    {
        var context = CreateContext();
        var failures = new[] { new ValidationFailure("Name", "Name is required") { ErrorCode = "Rule-01" } };
        _mockNext.Setup(x => x(context)).ThrowsAsync(new ValidationException(failures));

        await _sut.Invoke(context, _mockCorrelationIdAccessor.Object);

        var body = await ReadResponseBodyAsync(context);
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        root.ValueKind.Should().Be(JsonValueKind.Array, "validation errors are serialized as a JSON array");
        var error = root[0];
        using (new AssertionScope())
        {
            error.TryGetProperty("propertyName", out _).Should().BeTrue();
            error.TryGetProperty("errorCode", out _).Should().BeTrue();
            error.TryGetProperty("errorMessage", out _).Should().BeTrue();
            error.TryGetProperty("correlationId", out _).Should().BeTrue();
        }
    }

    [Fact]
    public async Task Invoke_Returns404_WhenKeyNotFoundExceptionThrown()
    {
        var context = CreateContext();
        _mockNext.Setup(x => x(context)).ThrowsAsync(new KeyNotFoundException("resource not found"));

        await _sut.Invoke(context, _mockCorrelationIdAccessor.Object);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Invoke_Returns409_WhenConflictExceptionThrown()
    {
        var context = CreateContext();
        _mockNext.Setup(x => x(context)).ThrowsAsync(new ConflictException("resource conflict"));

        await _sut.Invoke(context, _mockCorrelationIdAccessor.Object);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Invoke_LogsError_WhenExceptionThrown()
    {
        var context = CreateContext();
        var exception = new InvalidOperationException("test error");
        _mockNext.Setup(x => x(context)).ThrowsAsync(exception);

        await _sut.Invoke(context, _mockCorrelationIdAccessor.Object);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
