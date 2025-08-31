using System.IO;
using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Verbs.Handlers;
using FluentAssertions;
using Moq;
using Xunit;
using SysConsole = System.Console;

namespace AdventureWorks.DbReset.Console.Tests.Verbs.Handlers;

public sealed class ResetHandlerTests
{
    private const string TargetName = "AdventureWorks_E2E";

    [Fact]
    public async Task RunAsync_AllStepsSucceed_ReturnsExitOkWithElapsedMessage()
    {
        var verifyBaseline = new Mock<IVerifyBaselineHandler>(MockBehavior.Strict);
        var restore        = new Mock<IRestoreHandler>(MockBehavior.Strict);
        var migrate        = new Mock<IMigrateHandler>(MockBehavior.Strict);

        verifyBaseline
            .Setup(h => h.RunAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VerbResult.Ok("verify ok"));
        restore
            .Setup(h => h.RunAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VerbResult.Ok("restore ok"));
        migrate
            .Setup(h => h.RunAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VerbResult.Ok("migrate ok"));

        var sut = new ResetHandler(verifyBaseline.Object, restore.Object, migrate.Object);

        var originalOut = SysConsole.Out;
        SysConsole.SetOut(new StringWriter());
        VerbResult result;
        try
        {
            result = await sut.RunAsync(TargetName, CancellationToken.None);
        }
        finally
        {
            SysConsole.SetOut(originalOut);
        }

        result.ExitCode.Should().Be(DbResetDefaults.ExitOk);
        result.StdOut.Should().NotBeNull();
        result.StdOut!.Should().Contain(TargetName);
        result.StdOut.Should().Contain("reset complete");
    }

    [Fact]
    public async Task RunAsync_VerifyBaselineFails_AbortsAndReturnsVerifyResult()
    {
        var verifyBaseline = new Mock<IVerifyBaselineHandler>(MockBehavior.Strict);
        var restore        = new Mock<IRestoreHandler>(MockBehavior.Strict);
        var migrate        = new Mock<IMigrateHandler>(MockBehavior.Strict);

        var expected = VerbResult.Fail(DbResetDefaults.ExitVerifyBaselineMissing, "baseline missing");
        verifyBaseline
            .Setup(h => h.RunAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new ResetHandler(verifyBaseline.Object, restore.Object, migrate.Object);

        var originalOut = SysConsole.Out;
        SysConsole.SetOut(new StringWriter());
        VerbResult result;
        try
        {
            result = await sut.RunAsync(TargetName, CancellationToken.None);
        }
        finally
        {
            SysConsole.SetOut(originalOut);
        }

        result.Should().Be(expected);
    }

    [Fact]
    public async Task RunAsync_RestoreFails_AbortsAndReturnsRestoreResult()
    {
        var verifyBaseline = new Mock<IVerifyBaselineHandler>(MockBehavior.Strict);
        var restore        = new Mock<IRestoreHandler>(MockBehavior.Strict);
        var migrate        = new Mock<IMigrateHandler>(MockBehavior.Strict);

        verifyBaseline
            .Setup(h => h.RunAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VerbResult.Ok("verify ok"));
        restore
            .Setup(h => h.RunAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VerbResult.Fail(DbResetDefaults.ExitRestoreTargetUnreachable, "unreachable"));

        var sut = new ResetHandler(verifyBaseline.Object, restore.Object, migrate.Object);

        var originalOut = SysConsole.Out;
        SysConsole.SetOut(new StringWriter());
        VerbResult result;
        try
        {
            result = await sut.RunAsync(TargetName, CancellationToken.None);
        }
        finally
        {
            SysConsole.SetOut(originalOut);
        }

        result.ExitCode.Should().Be(DbResetDefaults.ExitRestoreTargetUnreachable);
    }

    [Fact]
    public async Task RunAsync_MigrateFails_ReturnsMigrateResult()
    {
        var verifyBaseline = new Mock<IVerifyBaselineHandler>(MockBehavior.Strict);
        var restore        = new Mock<IRestoreHandler>(MockBehavior.Strict);
        var migrate        = new Mock<IMigrateHandler>(MockBehavior.Strict);

        verifyBaseline
            .Setup(h => h.RunAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VerbResult.Ok("verify ok"));
        restore
            .Setup(h => h.RunAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VerbResult.Ok("restore ok"));
        migrate
            .Setup(h => h.RunAsync(TargetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VerbResult.Fail(DbResetDefaults.ExitMigrateFailed, "failed"));

        var sut = new ResetHandler(verifyBaseline.Object, restore.Object, migrate.Object);

        var originalOut = SysConsole.Out;
        SysConsole.SetOut(new StringWriter());
        VerbResult result;
        try
        {
            result = await sut.RunAsync(TargetName, CancellationToken.None);
        }
        finally
        {
            SysConsole.SetOut(originalOut);
        }

        result.ExitCode.Should().Be(DbResetDefaults.ExitMigrateFailed);
    }

    [Fact]
    public async Task RunAsync_VerifyBaselineThrowsOperationCanceledException_Rethrows()
    {
        var verifyBaseline = new Mock<IVerifyBaselineHandler>(MockBehavior.Strict);
        var restore        = new Mock<IRestoreHandler>(MockBehavior.Strict);
        var migrate        = new Mock<IMigrateHandler>(MockBehavior.Strict);

        verifyBaseline
            .Setup(h => h.RunAsync(TargetName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var sut = new ResetHandler(verifyBaseline.Object, restore.Object, migrate.Object);

        var originalOut = SysConsole.Out;
        SysConsole.SetOut(new StringWriter());
        try
        {
            var act = async () => await sut.RunAsync(TargetName, CancellationToken.None);
            await act.Should().ThrowAsync<OperationCanceledException>();
        }
        finally
        {
            SysConsole.SetOut(originalOut);
        }
    }
}
