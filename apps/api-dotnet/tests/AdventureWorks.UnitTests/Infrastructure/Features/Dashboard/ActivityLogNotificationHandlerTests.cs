using AdventureWorks.Application.Features.Dashboard.Notifications;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Dashboard;
using AdventureWorks.Infrastructure.Persistence.Features.Dashboard;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.Infrastructure.Features.Dashboard;

public sealed class ActivityLogNotificationHandlerTests : UnitTestBase
{
    private readonly Mock<IActivityLogRepository> _mockRepository = new();
    private readonly Mock<ILogger<ActivityLogNotificationHandler>> _mockLogger = new();

    private ActivityLogNotificationHandler CreateSut()
        => new(_mockRepository.Object, _mockLogger.Object);

    [Fact]
    public void Constructor_throws_ArgumentNullException_for_null_repository()
    {
        var act = () => new ActivityLogNotificationHandler(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("repository");
    }

    [Fact]
    public void Constructor_throws_ArgumentNullException_for_null_logger()
    {
        var act = () => new ActivityLogNotificationHandler(_mockRepository.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task Handle_calls_repository_AddAsync_with_correct_field_valuesAsync()
    {
        ActivityLogEntity? capturedEntry = null;
        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<ActivityLogEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ActivityLogEntity, CancellationToken>((e, _) => capturedEntry = e)
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        var notification = new EntityChangedNotification
        {
            EntityType = "Store",
            EntityId = 42,
            Action = "Created",
            UserName = "user@example.com",
            Timestamp = DefaultAuditDate
        };

        await sut.Handle(notification, CancellationToken.None);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<ActivityLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);

        using (new AssertionScope())
        {
            capturedEntry.Should().NotBeNull();
            capturedEntry!.EntityType.Should().Be("Store");
            capturedEntry.EntityId.Should().Be(42);
            capturedEntry.Action.Should().Be("Created");
            capturedEntry.UserName.Should().Be("user@example.com");
            capturedEntry.Timestamp.Should().Be(DefaultAuditDate);
        }
    }

    [Fact]
    public async Task Handle_truncates_UserName_longer_than_256_charactersAsync()
    {
        ActivityLogEntity? capturedEntry = null;
        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<ActivityLogEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ActivityLogEntity, CancellationToken>((e, _) => capturedEntry = e)
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var longUserName = new string('a', 300);

        var notification = new EntityChangedNotification
        {
            EntityType = "Store",
            EntityId = 1,
            Action = "Created",
            UserName = longUserName,
            Timestamp = DefaultAuditDate
        };

        await sut.Handle(notification, CancellationToken.None);

        capturedEntry.Should().NotBeNull();
        capturedEntry!.UserName.Should().HaveLength(256);
    }

    [Fact]
    public async Task Handle_catches_exception_during_save_and_does_not_rethrowAsync()
    {
        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<ActivityLogEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var sut = CreateSut();

        var notification = new EntityChangedNotification
        {
            EntityType = "Store",
            EntityId = 1,
            Action = "Created",
            UserName = "user@example.com",
            Timestamp = DefaultAuditDate
        };

        var act = async () => await sut.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
