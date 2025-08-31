using AdventureWorks.DbReset.Console.Migration;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Migration;

public sealed class DbUpProcessRunnerTests
{
    private static readonly DbUpProcessRunner Sut = new();

    [Fact]
    public async Task RunAsync_BlankAbsoluteProjectPath_ThrowsArgumentException()
    {
        var act = async () => await Sut.RunAsync("   ", "Server=localhost;", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RunAsync_BlankConnectionString_ThrowsArgumentException()
    {
        var act = async () => await Sut.RunAsync("/some/path", "   ", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
