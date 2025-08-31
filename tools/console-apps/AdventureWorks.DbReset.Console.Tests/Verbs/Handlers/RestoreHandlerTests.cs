using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Snapshot;
using AdventureWorks.DbReset.Console.Verbs.Handlers;
using FluentAssertions;
using Moq;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Verbs.Handlers;

public sealed class RestoreHandlerTests
{
    private const string TargetName = "AdventureWorks_E2E";
    private const string BaselinePath = "/baselines/AdventureWorks.bak";

    private static DbResetOptions BuildOptions() => new()
    {
        SnapshotSource = "AdventureWorksDev",
        DefaultTarget = TargetName,
        BaselinePath = BaselinePath,
        TargetNamePattern = "^AdventureWorks_(E2E|Test)$",
        DbUpProjectPath = "database/dbup/AdventureWorks.DbUp",
        SourceMarker = new SourceMarkerOptions { Property = "dbreset.role", Value = "source" },
    };

    [Fact]
    public async Task RunAsync_BaselinePresent_RestoreSucceeds_ReturnsExitOk()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.BaselineExistsAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BaselineStatus.Present(BaselinePath, 1024));
        provider
            .Setup(p => p.RestoreSnapshotAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TimeSpan.FromSeconds(2.5));

        var sut = new RestoreHandler(provider.Object, BuildOptions());

        var result = await sut.RunAsync(TargetName, CancellationToken.None);

        result.ExitCode.Should().Be(DbResetDefaults.ExitOk);
        result.StdOut.Should().NotBeNull();
        result.StdOut!.Should().Contain(TargetName);
        result.StdOut.Should().Contain("2.5s");
        result.StdErr.Should().BeNull();
    }

    [Fact]
    public async Task RunAsync_BaselineMissing_ReturnsExitVerifyBaselineMissing()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.BaselineExistsAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BaselineStatus.Missing(BaselinePath));

        var sut = new RestoreHandler(provider.Object, BuildOptions());

        var result = await sut.RunAsync(TargetName, CancellationToken.None);

        result.ExitCode.Should().Be(DbResetDefaults.ExitVerifyBaselineMissing);
        result.StdErr.Should().NotBeNull();
        result.StdErr!.Should().Contain(BaselinePath);
        result.StdErr.Should().Contain("snapshot");
    }

    [Fact]
    public async Task RunAsync_RestoreThrowsSqlException4060_ReturnsExitRestoreTargetUnreachable()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.BaselineExistsAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BaselineStatus.Present(BaselinePath, 1024));
        provider
            .Setup(p => p.RestoreSnapshotAsync(TargetName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(SqlExceptionFactory.Create(4060, "Cannot open database requested by the login."));

        var sut = new RestoreHandler(provider.Object, BuildOptions());

        var result = await sut.RunAsync(TargetName, CancellationToken.None);

        result.ExitCode.Should().Be(DbResetDefaults.ExitRestoreTargetUnreachable);
        result.StdErr.Should().NotBeNull();
        result.StdErr!.Should().Contain(TargetName);
    }

    [Fact]
    public async Task RunAsync_RestoreThrowsSqlExceptionOsError5_ReturnsExitRestorePermissionDenied()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.BaselineExistsAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BaselineStatus.Present(BaselinePath, 1024));
        provider
            .Setup(p => p.RestoreSnapshotAsync(TargetName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(SqlExceptionFactory.Create(
                3201,
                "Cannot open backup device. Operating system error 5(Access is denied.)."));

        var sut = new RestoreHandler(provider.Object, BuildOptions());

        var result = await sut.RunAsync(TargetName, CancellationToken.None);

        result.ExitCode.Should().Be(DbResetDefaults.ExitRestorePermissionDenied);
        result.StdErr.Should().NotBeNull();
        result.StdErr!.Should().Contain(BaselinePath);
    }

    [Fact]
    public async Task RunAsync_RestoreThrowsUnrecognizedSqlException_Rethrows()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.BaselineExistsAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BaselineStatus.Present(BaselinePath, 1024));
        provider
            .Setup(p => p.RestoreSnapshotAsync(TargetName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(SqlExceptionFactory.Create(9999, "unrecognized failure"));

        var sut = new RestoreHandler(provider.Object, BuildOptions());

        var act = async () => await sut.RunAsync(TargetName, CancellationToken.None);

        await act.Should().ThrowAsync<Microsoft.Data.SqlClient.SqlException>();
    }

    [Fact]
    public async Task RunAsync_RestoreThrowsOperationCanceledException_Rethrows()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.BaselineExistsAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BaselineStatus.Present(BaselinePath, 1024));
        provider
            .Setup(p => p.RestoreSnapshotAsync(TargetName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var sut = new RestoreHandler(provider.Object, BuildOptions());

        var act = async () => await sut.RunAsync(TargetName, CancellationToken.None);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RunAsync_RestoreThrowsInvalidOperationException_Rethrows()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.BaselineExistsAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BaselineStatus.Present(BaselinePath, 1024));
        provider
            .Setup(p => p.RestoreSnapshotAsync(TargetName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("config bug"));

        var sut = new RestoreHandler(provider.Object, BuildOptions());

        var act = async () => await sut.RunAsync(TargetName, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("config bug");
    }
}
