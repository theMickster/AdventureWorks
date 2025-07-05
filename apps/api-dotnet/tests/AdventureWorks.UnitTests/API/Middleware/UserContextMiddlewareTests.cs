using AdventureWorks.API.libs.Middleware;
using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Application.Interfaces;
using AdventureWorks.Models.Features.Person;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AdventureWorks.UnitTests.API.Middleware;

public sealed class UserContextMiddlewareTests : UnitTestBase
{
    private readonly Mock<RequestDelegate> _mockNext = new();
    private readonly Mock<ILogger<UserContextMiddleware>> _mockLogger = new();
    private readonly Mock<IUserContextAccessor> _mockUserContextAccessor = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private UserContextMiddleware _sut;

    public UserContextMiddlewareTests()
    {
        _sut = new UserContextMiddleware(_mockNext.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenNextIsNull()
    {
        var act = () => new UserContextMiddleware(null!, _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("next");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new UserContextMiddleware(_mockNext.Object, null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task InvokeAsync_ThrowsArgumentNullException_WhenContextIsNull()
    {
        var act = async () => await _sut.InvokeAsync(
            null!, 
            _mockUserContextAccessor.Object,
            _mockMediator.Object);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task InvokeAsync_ThrowsArgumentNullException_WhenUserContextAccessorIsNull()
    {
        var httpContext = new DefaultHttpContext();

        var act = async () => await _sut.InvokeAsync(
            httpContext, 
            null!,
            _mockMediator.Object);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task InvokeAsync_ThrowsArgumentNullException_WhenMediatorIsNull()
    {
        var httpContext = new DefaultHttpContext();

        var act = async () => await _sut.InvokeAsync(
            httpContext, 
            _mockUserContextAccessor.Object,
            null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task InvokeAsync_SetsUnauthenticatedContext_WhenUserNotAuthenticated()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // Not authenticated

        UserContext? capturedContext = null;
        _mockUserContextAccessor
            .Setup(x => x.SetUserContext(It.IsAny<UserContext>()))
            .Callback<UserContext>(ctx => capturedContext = ctx);

        // Act
        await _sut.InvokeAsync(httpContext, _mockUserContextAccessor.Object, _mockMediator.Object);

        // Assert
        using (new AssertionScope())
        {
            capturedContext.Should().NotBeNull("because context should be set");
            capturedContext!.IsAuthenticated.Should().BeFalse("because user is not authenticated");
            capturedContext.EntraObjectId.Should().BeNull("because user is not authenticated");
            capturedContext.BusinessEntityId.Should().BeNull("because user is not authenticated");
        }
    }

    [Fact]
    public async Task InvokeAsync_DoesNotCallMediator_WhenNotAuthenticated()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        await _sut.InvokeAsync(httpContext, _mockUserContextAccessor.Object, _mockMediator.Object);

        // Assert
        _mockMediator.Verify(
            x => x.Send(It.IsAny<ReadEntraLinkedPersonQuery>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "because unauthenticated users should not trigger person lookup");
    }

    [Fact]
    public async Task InvokeAsync_ResolvesBusinessEntityId_WhenEntraUserLinked()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        var httpContext = CreateAuthenticatedContext(entraObjectId, "john.doe@adventure-works.com", "John Doe");

        var linkedPerson = new EntraLinkedPersonModel
        {
            BusinessEntityId = 100,
            EntraObjectId = entraObjectId,
            FirstName = "John",
            LastName = "Doe",
            PersonTypeId = 1,
            PersonTypeName = "Employee",
            IsEntraUser = true
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEntraLinkedPersonQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(linkedPerson);

        UserContext? capturedContext = null;
        _mockUserContextAccessor
            .Setup(x => x.SetUserContext(It.IsAny<UserContext>()))
            .Callback<UserContext>(ctx => capturedContext = ctx);

        // Act
        await _sut.InvokeAsync(httpContext, _mockUserContextAccessor.Object, _mockMediator.Object);

        // Assert
        using (new AssertionScope())
        {
            capturedContext.Should().NotBeNull();
            capturedContext!.IsAuthenticated.Should().BeTrue();
            capturedContext.EntraObjectId.Should().Be(entraObjectId);
            capturedContext.BusinessEntityId.Should().Be(100);
            capturedContext.PersonFullName.Should().Be("John Doe");
        }
    }

    [Fact]
    public async Task InvokeAsync_LogsDebug_OnSuccessfulResolution()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        var httpContext = CreateAuthenticatedContext(entraObjectId, "jane.smith@adventure-works.com", "Jane Smith");

        var linkedPerson = new EntraLinkedPersonModel
        {
            BusinessEntityId = 200,
            EntraObjectId = entraObjectId,
            FirstName = "Jane",
            LastName = "Smith",
            PersonTypeId = 1,
            PersonTypeName = "Employee",
            IsEntraUser = true
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEntraLinkedPersonQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(linkedPerson);

        // Act
        await _sut.InvokeAsync(httpContext, _mockUserContextAccessor.Object, _mockMediator.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Resolved Entra user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because successful resolution should be logged");
    }

    [Fact]
    public async Task InvokeAsync_LogsWarning_WhenEntraUserNotLinked()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        var httpContext = CreateAuthenticatedContext(entraObjectId, "unknown@adventure-works.com", "Unknown User");

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEntraLinkedPersonQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntraLinkedPersonModel?)null);

        // Act
        await _sut.InvokeAsync(httpContext, _mockUserContextAccessor.Object, _mockMediator.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not linked to AdventureWorks")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because unlinked user should trigger warning");
    }

    [Fact]
    public async Task InvokeAsync_SetsContextWithoutBusinessEntityId_WhenNotLinked()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        var httpContext = CreateAuthenticatedContext(entraObjectId, "unlinked@adventure-works.com", "Unlinked User");

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEntraLinkedPersonQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntraLinkedPersonModel?)null);

        UserContext? capturedContext = null;
        _mockUserContextAccessor
            .Setup(x => x.SetUserContext(It.IsAny<UserContext>()))
            .Callback<UserContext>(ctx => capturedContext = ctx);

        // Act
        await _sut.InvokeAsync(httpContext, _mockUserContextAccessor.Object, _mockMediator.Object);

        // Assert
        using (new AssertionScope())
        {
            capturedContext.Should().NotBeNull();
            capturedContext!.IsAuthenticated.Should().BeTrue();
            capturedContext.EntraObjectId.Should().Be(entraObjectId);
            capturedContext.BusinessEntityId.Should().BeNull("because user is not linked");
        }
    }

    [Fact]
    public async Task InvokeAsync_ContinuesPipeline_WhenMediatorThrows()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        var httpContext = CreateAuthenticatedContext(entraObjectId, "error@adventure-works.com", "Error User");

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEntraLinkedPersonQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        await _sut.InvokeAsync(httpContext, _mockUserContextAccessor.Object, _mockMediator.Object);

        // Assert - Should not throw, should continue pipeline
        _mockNext.Verify(x => x(httpContext), Times.Once, 
            "because pipeline should continue even on error");
    }

    [Fact]
    public async Task InvokeAsync_LogsError_WhenMediatorThrows()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        var httpContext = CreateAuthenticatedContext(entraObjectId, "error@adventure-works.com", "Error User");

        var exception = new InvalidOperationException("Database error");
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEntraLinkedPersonQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        await _sut.InvokeAsync(httpContext, _mockUserContextAccessor.Object, _mockMediator.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error resolving BusinessEntityId")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because error should be logged");
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware_Always()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        await _sut.InvokeAsync(httpContext, _mockUserContextAccessor.Object, _mockMediator.Object);

        // Assert
        _mockNext.Verify(x => x(httpContext), Times.Once, 
            "because next middleware should always be called");
    }

    [Fact]
    public async Task InvokeAsync_SetsUserContextAccessor_BeforeCallingNext()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        var setUserContextCalled = false;
        var nextCalled = false;

        _mockUserContextAccessor
            .Setup(x => x.SetUserContext(It.IsAny<UserContext>()))
            .Callback(() => setUserContextCalled = true);

        _mockNext
            .Setup(x => x(It.IsAny<HttpContext>()))
            .Callback(() =>
            {
                nextCalled = true;
                setUserContextCalled.Should().BeTrue("because SetUserContext should be called before next middleware");
            })
            .Returns(Task.CompletedTask);

        // Act
        await _sut.InvokeAsync(httpContext, _mockUserContextAccessor.Object, _mockMediator.Object);

        // Assert
        nextCalled.Should().BeTrue("because next middleware should be called");
    }

    // Helper method to create authenticated HttpContext
    private static DefaultHttpContext CreateAuthenticatedContext(Guid entraObjectId, string upn, string displayName)
    {
        var claims = new[]
        {
            new Claim("oid", entraObjectId.ToString()),
            new Claim("preferred_username", upn),
            new Claim("name", displayName)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        return new DefaultHttpContext
        {
            User = principal
        };
    }
}
