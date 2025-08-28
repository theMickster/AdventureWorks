using AdventureWorks.DbReset.Console.Verbs.Handlers;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Verbs.Handlers;

public sealed class ByteSizeFormatterTests
{
    [Theory]
    [InlineData(0L, "0 B")]
    [InlineData(1L, "1 B")]
    [InlineData(1023L, "1023 B")]
    [InlineData(1024L, "1.0 KiB")]
    [InlineData(1_048_575L, "1024.0 KiB")]
    [InlineData(1_048_576L, "1.0 MiB")]
    [InlineData(1_572_864L, "1.5 MiB")]
    [InlineData(1_073_741_823L, "1024.0 MiB")]
    [InlineData(1_073_741_824L, "1.0 GiB")]
    [InlineData(5_500_000_000L, "5.1 GiB")]
    [InlineData(1_099_511_627_776L, "1.0 TiB")]
    public void Format_ProducesExpectedString(long bytes, string expected)
    {
        ByteSizeFormatter.Format(bytes).Should().Be(expected);
    }

    [Fact]
    public void Format_NegativeBytes_Throws()
    {
        var act = () => ByteSizeFormatter.Format(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
