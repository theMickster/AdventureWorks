using AdventureWorks.DbReset.Console.Snapshot;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Snapshot;

public sealed class RecoveryOutcomeTests
{
    [Fact]
    public void NoOp_ReturnsNoneActionWithNoStateChange()
    {
        var outcome = RecoveryOutcome.NoOp();

        outcome.Action.Should().Be(RecoveryAction.None);
        outcome.StateChanged.Should().BeFalse();
        outcome.Detail.Should().BeNull();
    }

    [Theory]
    [InlineData(RecoveryAction.None)]
    [InlineData(RecoveryAction.RecoveredFromRestoring)]
    [InlineData(RecoveryAction.RestoredMultiUser)]
    [InlineData(RecoveryAction.Both)]
    public void RecoveryAction_AllFourValuesExist(RecoveryAction action)
    {
        Enum.IsDefined(typeof(RecoveryAction), action).Should().BeTrue();
    }

    [Fact]
    public void Records_AreStructurallyEqual()
    {
        var a = new RecoveryOutcome(RecoveryAction.RecoveredFromRestoring, true, "x");
        var b = new RecoveryOutcome(RecoveryAction.RecoveredFromRestoring, true, "x");

        a.Should().Be(b);
    }

    [Fact]
    public void Records_DifferByDetail()
    {
        var a = new RecoveryOutcome(RecoveryAction.None, false, "x");
        var b = new RecoveryOutcome(RecoveryAction.None, false, "y");

        a.Should().NotBe(b);
    }
}
