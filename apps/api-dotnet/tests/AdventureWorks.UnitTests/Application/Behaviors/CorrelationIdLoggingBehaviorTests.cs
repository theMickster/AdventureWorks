using AdventureWorks.Application.Behaviors;
using AdventureWorks.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.Application.Behaviors;

public sealed class CorrelationIdLoggingBehaviorTests
{
    private readonly Mock<ILogger<CorrelationIdLoggingBehavior<TestRequest, TestResponse>>> _loggerMock;
    private readonly Mock<ICorrelationIdAccessor> _correlationIdAccessorMock;

    public CorrelationIdLoggingBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<CorrelationIdLoggingBehavior<TestRequest, TestResponse>>>();
        _correlationIdAccessorMock = new Mock<ICorrelationIdAccessor>();
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new CorrelationIdLoggingBehavior<TestRequest, TestResponse>(null!, _correlationIdAccessorMock.Object));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullCorrelationIdAccessor_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new CorrelationIdLoggingBehavior<TestRequest, TestResponse>(_loggerMock.Object, null!));
        Assert.Equal("correlationIdAccessor", exception.ParamName);
    }

    [Fact]
    public async Task Handle_NullRequest_ThrowsArgumentNullException()
    {
        var behavior = new CorrelationIdLoggingBehavior<TestRequest, TestResponse>(
            _loggerMock.Object,
            _correlationIdAccessorMock.Object);

        var next = new RequestHandlerDelegate<TestResponse>(async (ct) => await Task.FromResult(new TestResponse()));
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await behavior.Handle(null!, next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NullNext_ThrowsArgumentNullException()
    {
        var behavior = new CorrelationIdLoggingBehavior<TestRequest, TestResponse>(
            _loggerMock.Object,
            _correlationIdAccessorMock.Object);
        var request = new TestRequest();
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await behavior.Handle(request, null!, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithCorrelationId_LogsProcessingAndCompletion()
    {
        var correlationId = "test-correlation-id-123";
        var expectedResponse = new TestResponse { Value = "success" };
        var nextCalled = false;

        _correlationIdAccessorMock.Setup(x => x.CorrelationId).Returns(correlationId);

        var next = new RequestHandlerDelegate<TestResponse>(async (ct) =>
        {
            nextCalled = true;
            return await Task.FromResult(expectedResponse);
        });

        var behavior = new CorrelationIdLoggingBehavior<TestRequest, TestResponse>(
            _loggerMock.Object,
            _correlationIdAccessorMock.Object);

        var request = new TestRequest();
        var result = await behavior.Handle(request, next, CancellationToken.None);
        Assert.Equal(expectedResponse, result);
        Assert.True(nextCalled);

        // Verify logging occurred (checking LogInformation was called twice - start and completion)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoCorrelationId_LogsWithNA()
    {
        var expectedResponse = new TestResponse { Value = "success" };

        _correlationIdAccessorMock.Setup(x => x.CorrelationId).Returns((string?)null);

        var next = new RequestHandlerDelegate<TestResponse>(async (ct) => await Task.FromResult(expectedResponse));

        var behavior = new CorrelationIdLoggingBehavior<TestRequest, TestResponse>(
            _loggerMock.Object,
            _correlationIdAccessorMock.Object);

        var request = new TestRequest();
        var result = await behavior.Handle(request, next, CancellationToken.None);
        Assert.Equal(expectedResponse, result);

        // Verify N/A was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("N/A")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ExceptionThrown_LogsErrorWithCorrelationId()
    {
        var correlationId = "test-correlation-id-123";
        var expectedException = new InvalidOperationException("Test exception");

        _correlationIdAccessorMock.Setup(x => x.CorrelationId).Returns(correlationId);

        var next = new RequestHandlerDelegate<TestResponse>(async (ct) => { await Task.CompletedTask; throw expectedException; });

        var behavior = new CorrelationIdLoggingBehavior<TestRequest, TestResponse>(
            _loggerMock.Object,
            _correlationIdAccessorMock.Object);

        var request = new TestRequest();
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await behavior.Handle(request, next, CancellationToken.None));

        Assert.Equal(expectedException, thrownException);

        // Verify error logging occurred
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CallsNextDelegate()
    {
        var expectedResponse = new TestResponse { Value = "test" };
        var nextCalled = false;

        _correlationIdAccessorMock.Setup(x => x.CorrelationId).Returns("test-id");

        var next = new RequestHandlerDelegate<TestResponse>(async (ct) =>
        {
            nextCalled = true;
            return await Task.FromResult(expectedResponse);
        });

        var behavior = new CorrelationIdLoggingBehavior<TestRequest, TestResponse>(
            _loggerMock.Object,
            _correlationIdAccessorMock.Object);

        var request = new TestRequest();
        var result = await behavior.Handle(request, next, CancellationToken.None);
        Assert.Equal(expectedResponse, result);
        Assert.True(nextCalled);
    }

    // Test request and response classes - public for Moq/Castle proxy generation
    public sealed record TestRequest;
    public sealed record TestResponse
    {
        public string Value { get; init; } = string.Empty;
    }
}
