using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Verbs.Handlers;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Verbs.Handlers;

public sealed class VerbResultTests
{
    [Fact]
    public void Ok_PopulatesStdOutAndExitOk()
    {
        var r = VerbResult.Ok("hello");

        r.ExitCode.Should().Be(DbResetDefaults.ExitOk);
        r.StdOut.Should().Be("hello");
        r.StdErr.Should().BeNull();
    }

    [Fact]
    public void Ok_AllowsNullStdOut()
    {
        var r = VerbResult.Ok(null);

        r.ExitCode.Should().Be(DbResetDefaults.ExitOk);
        r.StdOut.Should().BeNull();
        r.StdErr.Should().BeNull();
    }

    [Fact]
    public void Fail_PopulatesStdErrAndExitCode()
    {
        var r = VerbResult.Fail(7, "boom");

        r.ExitCode.Should().Be(7);
        r.StdOut.Should().BeNull();
        r.StdErr.Should().Be("boom");
    }

    [Fact]
    public void Fail_RejectsEmptyMessage()
    {
        var act = () => VerbResult.Fail(7, string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Records_AreStructurallyEqual()
    {
        VerbResult.Ok("x").Should().Be(VerbResult.Ok("x"));
        VerbResult.Fail(2, "e").Should().Be(VerbResult.Fail(2, "e"));
    }

    [Fact]
    public void Records_DifferOnExitCode()
    {
        VerbResult.Fail(1, "e").Should().NotBe(VerbResult.Fail(2, "e"));
    }
}
