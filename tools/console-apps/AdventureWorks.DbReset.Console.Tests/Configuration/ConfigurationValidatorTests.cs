using AdventureWorks.DbReset.Console.Configuration;
using FluentAssertions;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Configuration;

public sealed class ConfigurationValidatorTests
{
    private const string ValidPattern = "^AdventureWorks_(E2E|Test)$";
    private const string ValidSourceCs = "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;";
    private const string ValidTargetCs = "Server=localhost;Database=AdventureWorks_E2E;Trusted_Connection=True;TrustServerCertificate=True;";

    private static DbResetOptions BuildValidOptions() => new()
    {
        SnapshotSource = "Source",
        DefaultTarget = "Target",
        BaselinePath = "baselines/baseline.bak",
        TargetNamePattern = ValidPattern,
        DbUpProjectPath = "database/dbup/AdventureWorks.DbUp",
        SourceMarker = new SourceMarkerOptions(),
    };

    private static IReadOnlyDictionary<string, string?> BuildValidConnectionStrings() =>
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Source"] = ValidSourceCs,
            ["Target"] = ValidTargetCs,
        };

    [Fact]
    public void Validate_WhenAllValid_ReturnsSuccess()
    {
        var result = new ConfigurationValidator().Validate(BuildValidOptions(), BuildValidConnectionStrings());

        result.Ok.Should().BeTrue();
        result.Reason.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenSnapshotSourceIsNullOrWhitespace_Fails(string? snapshotSource)
    {
        var options = BuildValidOptions();
        options.SnapshotSource = snapshotSource!;

        var result = new ConfigurationValidator().Validate(options, BuildValidConnectionStrings());

        result.Ok.Should().BeFalse();
        result.Reason.Should().Contain("DbReset:SnapshotSource");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenDefaultTargetIsNullOrWhitespace_Fails(string? defaultTarget)
    {
        var options = BuildValidOptions();
        options.DefaultTarget = defaultTarget!;

        var result = new ConfigurationValidator().Validate(options, BuildValidConnectionStrings());

        result.Ok.Should().BeFalse();
        result.Reason.Should().Contain("DbReset:DefaultTarget");
    }

    [Fact]
    public void Validate_WhenBaselinePathIsEmpty_Fails()
    {
        var options = BuildValidOptions();
        options.BaselinePath = string.Empty;

        var result = new ConfigurationValidator().Validate(options, BuildValidConnectionStrings());

        result.Ok.Should().BeFalse();
        result.Reason.Should().Contain("DbReset:BaselinePath");
    }

    [Fact]
    public void Validate_WhenTargetNamePatternIsEmpty_Fails()
    {
        var options = BuildValidOptions();
        options.TargetNamePattern = string.Empty;

        var result = new ConfigurationValidator().Validate(options, BuildValidConnectionStrings());

        result.Ok.Should().BeFalse();
        result.Reason.Should().Contain("DbReset:TargetNamePattern");
    }

    [Fact]
    public void Validate_WhenDbUpProjectPathIsEmpty_Fails()
    {
        var options = BuildValidOptions();
        options.DbUpProjectPath = string.Empty;

        var result = new ConfigurationValidator().Validate(options, BuildValidConnectionStrings());

        result.Ok.Should().BeFalse();
        result.Reason.Should().Contain("DbReset:DbUpProjectPath");
    }

    [Fact]
    public void Validate_WhenSourceConnectionStringKeyMissing_Fails()
    {
        var options = BuildValidOptions();
        var connectionStrings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Target"] = ValidTargetCs,
        };

        var result = new ConfigurationValidator().Validate(options, connectionStrings);

        result.Ok.Should().BeFalse();
        result.Reason.Should().Contain("ConnectionStrings:Source");
        result.Reason.Should().Contain("DbReset:SnapshotSource");
    }

    [Fact]
    public void Validate_WhenSourceConnectionStringValueIsWhitespace_Fails()
    {
        var options = BuildValidOptions();
        var connectionStrings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Source"] = "   ",
            ["Target"] = ValidTargetCs,
        };

        var result = new ConfigurationValidator().Validate(options, connectionStrings);

        result.Ok.Should().BeFalse();
        result.Reason.Should().Contain("ConnectionStrings:Source");
    }

    [Fact]
    public void Validate_WhenTargetConnectionStringKeyMissing_Fails()
    {
        var options = BuildValidOptions();
        var connectionStrings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Source"] = ValidSourceCs,
        };

        var result = new ConfigurationValidator().Validate(options, connectionStrings);

        result.Ok.Should().BeFalse();
        result.Reason.Should().Contain("ConnectionStrings:Target");
        result.Reason.Should().Contain("DbReset:DefaultTarget");
    }

    [Fact]
    public void Validate_WhenTargetConnectionStringValueIsWhitespace_Fails()
    {
        var options = BuildValidOptions();
        var connectionStrings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Source"] = ValidSourceCs,
            ["Target"] = "",
        };

        var result = new ConfigurationValidator().Validate(options, connectionStrings);

        result.Ok.Should().BeFalse();
        result.Reason.Should().Contain("ConnectionStrings:Target");
    }

    [Fact]
    public void Validate_WhenTargetNamePatternIsInvalidRegex_Fails()
    {
        var options = BuildValidOptions();
        // Unbalanced parenthesis is a hard regex compile error.
        options.TargetNamePattern = "^AdventureWorks_(E2E";

        var result = new ConfigurationValidator().Validate(options, BuildValidConnectionStrings());

        result.Ok.Should().BeFalse();
        result.Reason.Should().Contain("DbReset:TargetNamePattern");
        result.Reason.Should().Contain("not a valid regex");
    }

    [Fact]
    public void Validate_PrefersStructuralFailureOverConnectionStringFailure()
    {
        // Both SnapshotSource is empty AND ConnectionStrings is empty.
        // Per validator order, structural fields are checked first.
        var options = BuildValidOptions();
        options.SnapshotSource = string.Empty;
        var connectionStrings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        var result = new ConfigurationValidator().Validate(options, connectionStrings);

        result.Ok.Should().BeFalse();
        result.Reason.Should().Contain("DbReset:SnapshotSource");
    }

    [Fact]
    public void Validate_WhenOptionsIsNull_Throws()
    {
        var act = () => new ConfigurationValidator().Validate(null!, BuildValidConnectionStrings());

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Validate_WhenConnectionStringsIsNull_Throws()
    {
        var act = () => new ConfigurationValidator().Validate(BuildValidOptions(), null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("connectionStrings");
    }
}
