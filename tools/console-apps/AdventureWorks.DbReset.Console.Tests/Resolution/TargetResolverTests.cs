using AdventureWorks.DbReset.Console.Resolution;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Resolution;

public sealed class TargetResolverTests
{
    [Fact]
    public void Resolve_WhenCliTargetIsNull_ReturnsDefaultTarget()
    {
        var result = new TargetResolver().Resolve(cliTarget: null, defaultTarget: "AdventureWorksE2E");

        result.Should().Be("AdventureWorksE2E");
    }

    [Fact]
    public void Resolve_WhenCliTargetIsEmpty_ReturnsDefaultTarget()
    {
        var result = new TargetResolver().Resolve(cliTarget: string.Empty, defaultTarget: "AdventureWorksE2E");

        result.Should().Be("AdventureWorksE2E");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Resolve_WhenCliTargetIsWhitespace_ReturnsDefaultTarget(string cliTarget)
    {
        var result = new TargetResolver().Resolve(cliTarget, defaultTarget: "AdventureWorksE2E");

        result.Should().Be("AdventureWorksE2E");
    }

    [Fact]
    public void Resolve_WhenCliTargetProvided_PreferredOverDefault()
    {
        var result = new TargetResolver().Resolve(cliTarget: "AdventureWorksLoad", defaultTarget: "AdventureWorksE2E");

        result.Should().Be("AdventureWorksLoad");
    }

    [Fact]
    public void Resolve_DoesNotTrimCliTargetValue()
    {
        // The validator (not the resolver) is responsible for trimming/pattern matching.
        // The resolver should pass the value through verbatim once it has decided to use it.
        var result = new TargetResolver().Resolve(cliTarget: " AdventureWorksE2E ", defaultTarget: "AdventureWorksLoad");

        result.Should().Be(" AdventureWorksE2E ");
    }

    [Fact]
    public void Resolve_WhenDefaultTargetIsNull_Throws()
    {
        var act = () => new TargetResolver().Resolve(cliTarget: null, defaultTarget: null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("defaultTarget");
    }
}
