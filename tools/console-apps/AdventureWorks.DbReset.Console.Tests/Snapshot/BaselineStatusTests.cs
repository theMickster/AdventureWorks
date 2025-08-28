using AdventureWorks.DbReset.Console.Snapshot;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Snapshot;

public sealed class BaselineStatusTests
{
    [Fact]
    public void Missing_ReturnsRecordWithFalseExistsAndNullSize()
    {
        var status = BaselineStatus.Missing("/baselines/aw.bak");

        status.Exists.Should().BeFalse();
        status.SizeBytes.Should().BeNull();
        status.Path.Should().Be("/baselines/aw.bak");
    }

    [Fact]
    public void Present_ReturnsRecordWithTrueExistsAndPopulatedSize()
    {
        var status = BaselineStatus.Present("/baselines/aw.bak", 1234L);

        status.Exists.Should().BeTrue();
        status.SizeBytes.Should().Be(1234L);
        status.Path.Should().Be("/baselines/aw.bak");
    }

    [Fact]
    public void Records_AreStructurallyEqual()
    {
        BaselineStatus.Missing("a").Should().Be(BaselineStatus.Missing("a"));
        BaselineStatus.Present("a", 1).Should().Be(BaselineStatus.Present("a", 1));
    }

    [Fact]
    public void Records_DifferByPath()
    {
        BaselineStatus.Missing("a").Should().NotBe(BaselineStatus.Missing("b"));
    }

    [Fact]
    public void Records_DifferBySize()
    {
        BaselineStatus.Present("a", 1).Should().NotBe(BaselineStatus.Present("a", 2));
    }
}
