using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Snapshot;
using AdventureWorks.DbReset.Console.Verbs.Handlers;
using FluentAssertions;
using Moq;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Verbs.Handlers;

public sealed class SnapshotHandlerTests
{
    private const string SourceKey = "AdventureWorksDev";
    private const string UnreachablePath = "/var/empty/does-not-exist/AdventureWorks_baseline.bak";

    private static DbResetOptions BuildOptions() => new()
    {
        SnapshotSource = SourceKey,
        DefaultTarget = "AdventureWorks_E2E",
        BaselinePath = UnreachablePath,
        TargetNamePattern = "^AdventureWorks_(E2E|Test)$",
        DbUpProjectPath = "database/dbup/AdventureWorks.DbUp",
        SourceMarker = new SourceMarkerOptions { Property = "dbreset.role", Value = "source" },
    };

    [Fact]
    public async Task RunAsync_HappyPath_ReturnsOkWithPathAndElapsed()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.CreateSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(TimeSpan.FromSeconds(2.5));

        var sut = new SnapshotHandler(provider.Object, BuildOptions());

        var result = await sut.RunAsync(CancellationToken.None);

        result.ExitCode.Should().Be(DbResetDefaults.ExitOk);
        result.StdOut.Should().NotBeNull();
        result.StdOut!.Should().Contain(UnreachablePath);
        result.StdOut.Should().Contain("2.5s");
        result.StdOut.Should().Contain("unknown"); // .bak not visible from this process
        result.StdErr.Should().BeNull();
    }

    [Fact]
    public async Task RunAsync_SqlException4060_MapsToUnreachableExitCode()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.CreateSnapshotAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(SqlExceptionFactory.Create(4060, "Cannot open database 'AW' requested by the login."));

        var sut = new SnapshotHandler(provider.Object, BuildOptions());

        var result = await sut.RunAsync(CancellationToken.None);

        result.ExitCode.Should().Be(DbResetDefaults.ExitSnapshotSourceUnreachable);
        result.StdErr.Should().NotBeNull();
        result.StdErr!.Should().Contain(SourceKey);
    }

    [Fact]
    public async Task RunAsync_OperatingSystemError5_MapsToPermissionDeniedExitCode()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.CreateSnapshotAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(SqlExceptionFactory.Create(
                3201,
                "Cannot open backup device. Operating system error 5(Access is denied.)."));

        var sut = new SnapshotHandler(provider.Object, BuildOptions());

        var result = await sut.RunAsync(CancellationToken.None);

        result.ExitCode.Should().Be(DbResetDefaults.ExitSnapshotPermissionDenied);
        result.StdErr.Should().NotBeNull();
        result.StdErr!.Should().Contain(UnreachablePath);
    }

    [Fact]
    public async Task RunAsync_UnrelatedException_Rethrows()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.CreateSnapshotAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("config bug"));

        var sut = new SnapshotHandler(provider.Object, BuildOptions());

        var act = async () => await sut.RunAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("config bug");
    }

    [Fact]
    public async Task RunAsync_UnrecognizedSqlException_Rethrows()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.CreateSnapshotAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(SqlExceptionFactory.Create(1234, "totally different"));

        var sut = new SnapshotHandler(provider.Object, BuildOptions());

        var act = async () => await sut.RunAsync(CancellationToken.None);

        await act.Should().ThrowAsync<Microsoft.Data.SqlClient.SqlException>();
    }
}
