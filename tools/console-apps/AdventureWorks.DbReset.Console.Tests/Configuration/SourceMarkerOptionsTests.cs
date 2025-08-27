using AdventureWorks.DbReset.Console.Configuration;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Configuration;

public sealed class SourceMarkerOptionsTests
{
    [Fact]
    public void Defaults_PropertyIsDbResetRole()
    {
        var options = new SourceMarkerOptions();

        options.Property.Should().Be("dbreset.role");
    }

    [Fact]
    public void Defaults_ValueIsSource()
    {
        var options = new SourceMarkerOptions();

        options.Value.Should().Be("source");
    }
}
