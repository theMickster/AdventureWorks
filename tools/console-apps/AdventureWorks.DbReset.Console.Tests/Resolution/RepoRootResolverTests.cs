using AdventureWorks.DbReset.Console.Resolution;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Resolution;

public sealed class RepoRootResolverTests : IDisposable
{
    private readonly string _tempRoot;

    public RepoRootResolverTests()
    {
        _tempRoot = new DirectoryInfo(
            Path.Combine(Path.GetTempPath(), "dbreset-tests-" + Guid.NewGuid()))
            .FullName;
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    [Fact]
    public void Resolve_FromNestedDirectoryUnderRepo_ReturnsRepoRootContainingDotGit()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, ".git"));
        var nested = Directory.CreateDirectory(
            Path.Combine(_tempRoot, "level1", "level2", "level3")).FullName;

        var result = new RepoRootResolver().Resolve(nested);

        result.Should().NotBeNull();
        new DirectoryInfo(result!).FullName.Should().Be(_tempRoot);
    }

    [Fact]
    public void Resolve_FromNestedDirectoryWithSolutionMarker_ReturnsRepoRoot()
    {
        File.WriteAllText(Path.Combine(_tempRoot, "AdventureWorks.sln"), string.Empty);
        var nested = Directory.CreateDirectory(
            Path.Combine(_tempRoot, "level1", "level2", "level3")).FullName;

        var result = new RepoRootResolver().Resolve(nested);

        result.Should().NotBeNull();
        new DirectoryInfo(result!).FullName.Should().Be(_tempRoot);
    }

    [Fact]
    public void Resolve_WhenNoMarkerExistsAnywhereUpChain_ReturnsNull()
    {
        // Use a path under an isolated temp tree we control. We create a sibling
        // "no-markers" directory under _tempRoot and pass that — neither it nor any
        // of its parents up to / contains a .git directory or AdventureWorks.sln,
        // because Path.GetTempPath()'s ancestors do not.
        var isolated = Directory.CreateDirectory(
            Path.Combine(_tempRoot, "no-markers", "deep")).FullName;

        var result = new RepoRootResolver().Resolve(isolated);

        result.Should().BeNull();
    }

    [Fact]
    public void Resolve_WhenStartDirectoryIsNull_Throws()
    {
        var act = () => new RepoRootResolver().Resolve(startDirectory: null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("startDirectory");
    }

    [Fact]
    public void Resolve_PrefersGitDirectoryOverSolutionFileWhenBothExistAtSameLevel()
    {
        // Both markers at the same level — Directory.Exists(.git) is checked first,
        // so the resolver returns at the level where .git lives.
        Directory.CreateDirectory(Path.Combine(_tempRoot, ".git"));
        File.WriteAllText(Path.Combine(_tempRoot, "AdventureWorks.sln"), string.Empty);
        var nested = Directory.CreateDirectory(Path.Combine(_tempRoot, "level1")).FullName;

        var result = new RepoRootResolver().Resolve(nested);

        result.Should().NotBeNull();
        new DirectoryInfo(result!).FullName.Should().Be(_tempRoot);
    }

    [Fact]
    public void Resolve_FromStartDirectoryThatIsItselfTheRoot_ReturnsRoot()
    {
        // Walk-up should consider the starting directory itself, not just its parents.
        Directory.CreateDirectory(Path.Combine(_tempRoot, ".git"));

        var result = new RepoRootResolver().Resolve(_tempRoot);

        result.Should().NotBeNull();
        new DirectoryInfo(result!).FullName.Should().Be(_tempRoot);
    }
}
