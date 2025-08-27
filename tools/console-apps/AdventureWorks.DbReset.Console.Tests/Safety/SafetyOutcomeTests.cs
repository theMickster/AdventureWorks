using AdventureWorks.DbReset.Console.Safety;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Safety;

public sealed class SafetyOutcomeTests
{
    [Fact]
    public void Success_HasOkTrueAndNullFailedRuleAndReason()
    {
        var outcome = SafetyOutcome.Success();

        outcome.Ok.Should().BeTrue();
        outcome.FailedRule.Should().BeNull();
        outcome.Reason.Should().BeNull();
    }

    [Fact]
    public void Fail_PropagatesRuleAndReason()
    {
        var outcome = SafetyOutcome.Fail("Rule_X", "because reasons");

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be("Rule_X");
        outcome.Reason.Should().Be("because reasons");
    }

    [Fact]
    public void Fail_WithEmptyStrings_StillReturnsNotOk()
    {
        // The factory does not validate inputs; it's a pure value carrier.
        var outcome = SafetyOutcome.Fail(string.Empty, string.Empty);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(string.Empty);
        outcome.Reason.Should().Be(string.Empty);
    }
}
