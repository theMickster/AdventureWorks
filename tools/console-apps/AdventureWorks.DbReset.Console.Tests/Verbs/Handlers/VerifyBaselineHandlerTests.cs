using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Snapshot;
using AdventureWorks.DbReset.Console.Verbs.Handlers;
using FluentAssertions;
using Moq;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Verbs.Handlers;

public sealed class VerifyBaselineHandlerTests
{
    private const string TargetName = "AdventureWorks_E2E";

    // Path designed never to exist on the test host so File.Exists returns false and we exercise
    // the FILELISTONLY-only fallback message.
    private const string UnreachablePath = "/var/empty/does-not-exist/AdventureWorks_baseline.bak";

    private static DbResetOptions BuildOptions(string baselinePath = UnreachablePath) => new()
    {
        SnapshotSource = "AdventureWorksDev",
        DefaultTarget = TargetName,
        BaselinePath = baselinePath,
        TargetNamePattern = "^AdventureWorks_(E2E|Test)$",
        DbUpProjectPath = "database/dbup/AdventureWorks.DbUp",
        SourceMarker = new SourceMarkerOptions { Property = "dbreset.role", Value = "source" },
    };

    [Fact]
    public async Task RunAsync_BaselinePresent_ReturnsOkWithLogicalSizeWhenFileNotVisible()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.BaselineExistsAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BaselineStatus.Present(UnreachablePath, 1_572_864L));

        var sut = new VerifyBaselineHandler(provider.Object, BuildOptions());

        var result = await sut.RunAsync(TargetName, CancellationToken.None);

        result.ExitCode.Should().Be(DbResetDefaults.ExitOk);
        result.StdOut.Should().NotBeNull();
        result.StdOut!.Should().Contain(UnreachablePath);
        result.StdOut.Should().Contain("1.5 MiB");
        result.StdOut.Should().Contain("logical");
        result.StdErr.Should().BeNull();
    }

    [Fact]
    public async Task RunAsync_BaselineMissing_ReturnsFailWithSnapshotHint()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.BaselineExistsAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BaselineStatus.Missing(UnreachablePath));

        var sut = new VerifyBaselineHandler(provider.Object, BuildOptions());

        var result = await sut.RunAsync(TargetName, CancellationToken.None);

        result.ExitCode.Should().Be(DbResetDefaults.ExitVerifyBaselineMissing);
        result.StdErr.Should().NotBeNull();
        result.StdErr!.Should().Contain(UnreachablePath);
        result.StdErr.Should().Contain("snapshot"); // names the verb literally
        result.StdErr.Should().NotContain("\n");    // single line
        result.StdOut.Should().BeNull();
    }

    [Fact]
    public async Task RunAsync_CancelledToken_PropagatesFromProvider()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        provider
            .Setup(p => p.BaselineExistsAsync(TargetName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("user cancelled"));

        var sut = new VerifyBaselineHandler(provider.Object, BuildOptions());

        var act = async () => await sut.RunAsync(TargetName, CancellationToken.None);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RunAsync_BlankTargetName_Throws()
    {
        var provider = new Mock<IDatabaseSnapshotProvider>(MockBehavior.Strict);
        var sut = new VerifyBaselineHandler(provider.Object, BuildOptions());

        var act = async () => await sut.RunAsync("   ", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
