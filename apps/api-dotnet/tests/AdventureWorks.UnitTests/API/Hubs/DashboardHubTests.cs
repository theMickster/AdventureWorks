using AdventureWorks.API.Hubs;
using AdventureWorks.Test.Common.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.API.Hubs;

public sealed class DashboardHubTests : UnitTestBase
{
    private readonly Mock<ILogger<DashboardHub>> _mockLogger = new();
    private readonly Mock<IGroupManager> _mockGroups = new();
    private readonly Mock<HubCallerContext> _mockContext = new();

    public DashboardHubTests()
    {
        _mockContext.Setup(x => x.ConnectionId).Returns("test-connection-id");
        _mockContext.Setup(x => x.User).Returns((System.Security.Claims.ClaimsPrincipal?)null);
    }

    private DashboardHub CreateSut()
    {
        var sut = new DashboardHub(_mockLogger.Object);
        sut.Groups = _mockGroups.Object;
        sut.Context = _mockContext.Object;
        return sut;
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        var sut = CreateSut();
        sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public async Task OnConnectedAsync_adds_connection_to_groupAsync()
    {
        var sut = CreateSut();
        _mockGroups.Setup(x => x.AddToGroupAsync("test-connection-id", "Dashboard", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await sut.OnConnectedAsync();

        _mockGroups.Verify(
            x => x.AddToGroupAsync("test-connection-id", "Dashboard", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_removes_connection_from_groupAsync()
    {
        var sut = CreateSut();
        _mockGroups.Setup(x => x.RemoveFromGroupAsync("test-connection-id", "Dashboard", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await sut.OnDisconnectedAsync(null);

        _mockGroups.Verify(
            x => x.RemoveFromGroupAsync("test-connection-id", "Dashboard", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
