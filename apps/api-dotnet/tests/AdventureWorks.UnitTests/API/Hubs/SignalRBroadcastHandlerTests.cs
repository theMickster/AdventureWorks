using AdventureWorks.API.Hubs;
using AdventureWorks.Application.Features.Dashboard.Notifications;
using AdventureWorks.Test.Common.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.API.Hubs;

public sealed class SignalRBroadcastHandlerTests : UnitTestBase
{
    private readonly Mock<IHubContext<DashboardHub>> _mockHubContext = new();
    private readonly Mock<ILogger<SignalRBroadcastHandler>> _mockLogger = new();
    private readonly Mock<IHubClients> _mockHubClients = new();
    private readonly Mock<IClientProxy> _mockClientProxy = new();
    private readonly SignalRBroadcastHandler _sut;

    public SignalRBroadcastHandlerTests()
    {
        _mockHubContext.Setup(x => x.Clients).Returns(_mockHubClients.Object);
        _mockHubClients.Setup(x => x.Group("Dashboard")).Returns(_mockClientProxy.Object);
        _sut = new SignalRBroadcastHandler(_mockHubContext.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public async Task Handle_broadcasts_EntityChanged_to_Dashboard_groupAsync()
    {
        var notification = new EntityChangedNotification
        {
            EntityType = "Store",
            EntityId = 42,
            Action = "Created",
            UserName = "user@example.com",
            Timestamp = DefaultAuditDate
        };

        _mockClientProxy
            .Setup(x => x.SendCoreAsync("EntityChanged", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(notification, CancellationToken.None);

        _mockClientProxy.Verify(
            x => x.SendCoreAsync("EntityChanged", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_catches_exception_and_does_not_rethrowAsync()
    {
        var notification = new EntityChangedNotification
        {
            EntityType = "Store",
            EntityId = 99,
            Action = "Updated",
            UserName = "user@example.com",
            Timestamp = DefaultAuditDate
        };

        _mockClientProxy
            .Setup(x => x.SendCoreAsync("EntityChanged", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Hub failure"));

        var act = async () => await _sut.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
