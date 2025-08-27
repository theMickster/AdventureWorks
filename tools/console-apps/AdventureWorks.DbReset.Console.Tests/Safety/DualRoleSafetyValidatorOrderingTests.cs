using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Safety;
using FluentAssertions;
using Moq;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Safety;

/// <summary>
/// Tests covering rule-ordering invariants, case-insensitive matching,
/// and null-argument guards on <see cref="DualRoleSafetyValidator"/>.
/// </summary>
public sealed class DualRoleSafetyValidatorOrderingTests
{
    private const string TestTargetPattern = "^AdventureWorks_(E2E|Test)$";

    private static DbResetOptions BuildOptions(
        string snapshotSource = "AdventureWorksDev",
        string defaultTarget = "AdventureWorksE2E",
        string targetNamePattern = TestTargetPattern)
    {
        return new DbResetOptions
        {
            SnapshotSource = snapshotSource,
            DefaultTarget = defaultTarget,
            BaselinePath = "baselines/baseline.bak",
            TargetNamePattern = targetNamePattern,
            DbUpProjectPath = "database/dbup/AdventureWorks.DbUp",
            SourceMarker = new SourceMarkerOptions(),
        };
    }

    private static DualRoleSafetyValidator BuildValidator(DbResetOptions options, bool markerReturns = false)
    {
        var probe = new Mock<ISourceMarkerProbe>();
        probe.Setup(p => p.HasMarker(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(markerReturns);
        return new DualRoleSafetyValidator(probe.Object, options);
    }

    [Fact]
    public void Validate_WhenRule1AndRule4WouldBothFail_ReportsRule1()
    {
        // Same key (Rule #1) AND target db name doesn't match pattern (Rule #4).
        // Rule #1 must win because it is evaluated first.
        var options = BuildOptions(snapshotSource: "Same", defaultTarget: "Same");
        var connectionStrings = new Dictionary<string, string?>
        {
            ["Same"] = "Server=localhost;Database=ProductionDb;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options).Validate(options, "Same", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule1_SameKey);
    }

    [Fact]
    public void Validate_WhenRule1AndRule5WouldBothFail_ReportsRule1()
    {
        // Same key (Rule #1) AND marker probe would say true (Rule #5).
        // Rule #1 must win.
        var options = BuildOptions(snapshotSource: "X", defaultTarget: "X");
        var connectionStrings = new Dictionary<string, string?>
        {
            ["X"] = "Server=localhost;Database=AdventureWorks_E2E;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options, markerReturns: true).Validate(options, "X", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule1_SameKey);
    }

    [Fact]
    public void Validate_WhenRule3AndRule4WouldBothFail_ReportsRule3()
    {
        // Target key missing (Rule #3) AND if it WERE present, name would not match pattern.
        // The implementation can't even check Rule #4 without a CS to parse, so Rule #3 must win.
        var options = BuildOptions(
            snapshotSource: "AdventureWorksDev",
            defaultTarget: "Missing",
            targetNamePattern: TestTargetPattern);
        var connectionStrings = new Dictionary<string, string?>
        {
            ["AdventureWorksDev"] = "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options).Validate(options, "Missing", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule3_MissingTargetKey);
    }

    [Fact]
    public void Validate_WhenRule2AndRule4WouldBothFail_ReportsRule2()
    {
        // Both keys parse to the same (Server, Database) pair (Rule #2) AND that database name
        // does not match the pattern (Rule #4). Rule #2 must win because it is evaluated first.
        var options = BuildOptions(
            snapshotSource: "Source",
            defaultTarget: "Target",
            targetNamePattern: TestTargetPattern);
        var connectionStrings = new Dictionary<string, string?>
        {
            ["Source"] = "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;",
            ["Target"] = "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options).Validate(options, "Target", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule2_SameServerDatabase);
    }

    [Fact]
    public void Validate_WhenRule4AndRule5WouldBothFail_ReportsRule4()
    {
        // Pattern violation (Rule #4) AND marker probe says true (Rule #5).
        // Rule #4 must win.
        var options = BuildOptions(
            snapshotSource: "Source",
            defaultTarget: "Target",
            targetNamePattern: TestTargetPattern);
        var connectionStrings = new Dictionary<string, string?>
        {
            ["Source"] = "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;",
            ["Target"] = "Server=otherhost;Database=NotMatching;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options, markerReturns: true).Validate(options, "Target", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule4_TargetNamePatternViolation);
    }

    [Fact]
    public void Validate_Rule1IsCaseInsensitive_FiresOnDifferentCasingOfSameKey()
    {
        // The Program.cs builds the dictionary with OrdinalIgnoreCase. Rule #1 itself uses
        // OrdinalIgnoreCase. When SnapshotSource is "AdventureWorksDev" and the resolved
        // target name is "adventureworksdev", they reference the same logical entry and
        // Rule #1 must fire.
        var options = BuildOptions(snapshotSource: "AdventureWorksDev", defaultTarget: "adventureworksdev");
        var connectionStrings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["AdventureWorksDev"] = "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options).Validate(options, "adventureworksdev", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule1_SameKey);
    }

    [Fact]
    public void Validate_Rule2IsCaseInsensitive_FiresOnDifferentCasingOfServerAndDatabase()
    {
        // Two different keys, but the connection strings differ only in Server/Database casing.
        // Rule #2 uses OrdinalIgnoreCase on DataSource and InitialCatalog, so this must fail.
        var options = BuildOptions(snapshotSource: "Source", defaultTarget: "Target");
        var connectionStrings = new Dictionary<string, string?>
        {
            ["Source"] = "Server=LocalHost;Database=adventureworks;Trusted_Connection=True;TrustServerCertificate=True;",
            ["Target"] = "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options).Validate(options, "Target", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule2_SameServerDatabase);
    }

    [Fact]
    public void Validate_WhenOptionsIsNull_Throws()
    {
        var act = () => BuildValidator(BuildOptions()).Validate(
            options: null!,
            effectiveTargetName: "AdventureWorksE2E",
            connectionStrings: new Dictionary<string, string?>());

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Validate_WhenEffectiveTargetNameIsNull_Throws()
    {
        var act = () => BuildValidator(BuildOptions()).Validate(
            BuildOptions(),
            effectiveTargetName: null!,
            connectionStrings: new Dictionary<string, string?>());

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("effectiveTargetName");
    }

    [Fact]
    public void Validate_WhenConnectionStringsIsNull_Throws()
    {
        var act = () => BuildValidator(BuildOptions()).Validate(
            BuildOptions(),
            effectiveTargetName: "AdventureWorksE2E",
            connectionStrings: null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("connectionStrings");
    }

    [Fact]
    public void Constructor_WhenProbeIsNull_Throws()
    {
        var act = () => new DualRoleSafetyValidator(markerProbe: null!, options: BuildOptions());

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("markerProbe");
    }

    [Fact]
    public void Constructor_WhenOptionsIsNull_Throws()
    {
        var probe = new Mock<ISourceMarkerProbe>();
        var act = () => new DualRoleSafetyValidator(probe.Object, options: null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Validate_Rule4MatchesAgainstDatabaseNameNotKey()
    {
        // The key is "Production_E2E" which would match the pattern, but the actual
        // Database= value is "Production_Live" which does NOT match. Rule #4 must
        // evaluate against the database name (InitialCatalog), not the dictionary key.
        var options = BuildOptions(
            snapshotSource: "Source",
            defaultTarget: "Production_E2E",
            targetNamePattern: TestTargetPattern);
        var connectionStrings = new Dictionary<string, string?>
        {
            ["Source"] = "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;",
            ["Production_E2E"] = "Server=otherhost;Database=Production_Live;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options).Validate(options, "Production_E2E", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule4_TargetNamePatternViolation);
        outcome.Reason.Should().Contain("Production_Live");
    }

    [Fact]
    public void Validate_Rule5_ProbeReceivesTargetConnectionStringAndConfiguredMarker()
    {
        // Verifies the probe is invoked with the target connection string and the
        // SourceMarker.Property/Value from options — not the source CS or hardcoded values.
        var options = BuildOptions(
            snapshotSource: "AdventureWorksDev",
            defaultTarget: "AdventureWorksE2E",
            targetNamePattern: TestTargetPattern);
        options.SourceMarker = new SourceMarkerOptions
        {
            Property = "custom.prop",
            Value = "custom.value",
        };
        const string targetCs = "Server=otherhost;Database=AdventureWorks_E2E;Trusted_Connection=True;TrustServerCertificate=True;";
        var connectionStrings = new Dictionary<string, string?>
        {
            ["AdventureWorksDev"] = "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;",
            ["AdventureWorksE2E"] = targetCs,
        };

        var probe = new Mock<ISourceMarkerProbe>();
        probe.Setup(p => p.HasMarker(targetCs, "custom.prop", "custom.value")).Returns(true);
        var validator = new DualRoleSafetyValidator(probe.Object, options);

        var outcome = validator.Validate(options, "AdventureWorksE2E", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule5_SourceMarkerPresent);
        probe.Verify(p => p.HasMarker(targetCs, "custom.prop", "custom.value"), Times.Once);
    }
}
