using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Migration;
using AdventureWorks.DbReset.Console.Verbs.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Verbs.Handlers;

public sealed class MigrateHandlerTests
{
    private const string TargetName = "AdventureWorks_E2E";
    private const string ConnectionString = "Server=localhost;Database=AdventureWorks_E2E;";
    private const string RepoRoot = "/repo";

    private static DbResetOptions BuildOptions(string dbUpProjectPath = "database/dbup/AdventureWorks.DbUp") => new()
    {
        DbUpProjectPath   = dbUpProjectPath,
        DefaultTarget     = TargetName,
        TargetNamePattern = "^AdventureWorks_(E2E|Test|Load)([A-Za-z0-9_]*)?$",
        BaselinePath      = "/baselines/AdventureWorks_baseline.bak",
        SnapshotSource    = "AdventureWorksDev",
    };

    private static IConfiguration BuildConfig(string targetName, string cs) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{targetName}"] = cs
            })
            .Build();

    [Fact]
    public async Task RunAsync_RunnerReturnsZero_ReturnsExitOkWithSuccessMessage()
    {
        var runner = new Mock<IDbUpProcessRunner>(MockBehavior.Strict);
        runner
            .Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var sut = new MigrateHandler(runner.Object, BuildConfig(TargetName, ConnectionString), BuildOptions(), RepoRoot);

        var result = await sut.RunAsync(TargetName, CancellationToken.None);

        result.ExitCode.Should().Be(DbResetDefaults.ExitOk);
        result.StdOut.Should().NotBeNull();
        result.StdOut!.Should().Contain(TargetName);
        result.StdErr.Should().BeNull();
    }

    [Fact]
    public async Task RunAsync_RunnerReturnsMinusOne_ReturnsMigrateFailedExitCode()
    {
        var runner = new Mock<IDbUpProcessRunner>(MockBehavior.Strict);
        runner
            .Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(-1);

        var sut = new MigrateHandler(runner.Object, BuildConfig(TargetName, ConnectionString), BuildOptions(), RepoRoot);

        var result = await sut.RunAsync(TargetName, CancellationToken.None);

        result.ExitCode.Should().Be(DbResetDefaults.ExitMigrateFailed);
        result.StdErr.Should().NotBeNull();
        result.StdErr!.Should().Contain(TargetName);
        result.StdOut.Should().BeNull();
    }

    [Fact]
    public async Task RunAsync_RunnerReturnsNonZero_ReturnsMigrateFailedExitCode()
    {
        var runner = new Mock<IDbUpProcessRunner>(MockBehavior.Strict);
        runner
            .Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var sut = new MigrateHandler(runner.Object, BuildConfig(TargetName, ConnectionString), BuildOptions(), RepoRoot);

        var result = await sut.RunAsync(TargetName, CancellationToken.None);

        result.ExitCode.Should().Be(DbResetDefaults.ExitMigrateFailed);
    }

    [Fact]
    public async Task RunAsync_RunnerThrowsOperationCanceledException_Rethrows()
    {
        var runner = new Mock<IDbUpProcessRunner>(MockBehavior.Strict);
        runner
            .Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var sut = new MigrateHandler(runner.Object, BuildConfig(TargetName, ConnectionString), BuildOptions(), RepoRoot);

        var act = async () => await sut.RunAsync(TargetName, CancellationToken.None);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RunAsync_BlankTargetName_ThrowsArgumentException()
    {
        var runner = new Mock<IDbUpProcessRunner>(MockBehavior.Strict);
        var sut = new MigrateHandler(runner.Object, BuildConfig(TargetName, ConnectionString), BuildOptions(), RepoRoot);

        var act = async () => await sut.RunAsync("   ", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RunAsync_MissingConnectionString_ThrowsInvalidOperationException()
    {
        var runner = new Mock<IDbUpProcessRunner>(MockBehavior.Strict);
        var emptyConfig = new ConfigurationBuilder().Build();
        var sut = new MigrateHandler(runner.Object, emptyConfig, BuildOptions(), RepoRoot);

        var act = async () => await sut.RunAsync(TargetName, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RunAsync_DbUpProjectPathEscapesRepoRoot_ThrowsInvalidOperationExceptionWithoutCallingRunner()
    {
        var runner = new Mock<IDbUpProcessRunner>(MockBehavior.Strict);
        var sut = new MigrateHandler(runner.Object, BuildConfig(TargetName, ConnectionString), BuildOptions("../../outside"), RepoRoot);

        var act = async () => await sut.RunAsync(TargetName, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        // MockBehavior.Strict enforces that RunAsync was never called on the runner.
    }
}
