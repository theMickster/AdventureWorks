using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Safety;
using FluentAssertions;
using Moq;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Safety;

public sealed class DualRoleSafetyValidatorTests
{
    private const string PermissivePattern = ".*";
    private const string TestTargetPattern = "^AdventureWorks_(E2E|Test)$";

    private static DbResetOptions BuildOptions(
        string snapshotSource = "AdventureWorksDev",
        string defaultTarget = "AdventureWorksE2E",
        string targetNamePattern = PermissivePattern)
    {
        return new DbResetOptions
        {
            SnapshotSource = snapshotSource,
            DefaultTarget = defaultTarget,
            BaselinePath = "baselines/AdventureWorks_baseline.bak",
            TargetNamePattern = targetNamePattern,
            DbUpProjectPath = "database/dbup/AdventureWorks.DbUp",
            SourceMarker = new SourceMarkerOptions
            {
                Property = "dbreset.role",
                Value = "source",
            },
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
    public void Validate_WhenSourceAndTargetUseSameKey_ReturnsRule1Failure()
    {
        var options = BuildOptions(snapshotSource: "X", defaultTarget: "X");
        var connectionStrings = new Dictionary<string, string?>
        {
            ["X"] = "Server=localhost;Database=AdventureWorks_E2E;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options).Validate(options, "X", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule1_SameKey);
    }

    [Fact]
    public void Validate_WhenSourceAndTargetResolveToSameServerAndDatabase_ReturnsRule2Failure()
    {
        var options = BuildOptions(snapshotSource: "A", defaultTarget: "B");
        var connectionStrings = new Dictionary<string, string?>
        {
            ["A"] = "Server=localhost;Database=AW;Trusted_Connection=True;TrustServerCertificate=True;",
            ["B"] = "Server=localhost;Database=AW;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options).Validate(options, "B", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule2_SameServerDatabase);
    }

    [Fact]
    public void Validate_WhenTargetKeyMissingFromConnectionStrings_ReturnsRule3Failure()
    {
        var options = BuildOptions(snapshotSource: "AdventureWorksDev", defaultTarget: "NotInDict");
        var connectionStrings = new Dictionary<string, string?>
        {
            ["AdventureWorksDev"] = "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options).Validate(options, "NotInDict", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule3_MissingTargetKey);
    }

    [Fact]
    public void Validate_WhenSourceKeyMissingFromConnectionStrings_ReturnsRule3SourceFailure()
    {
        // Target is present, source key is not. Must surface the distinct source-missing rule label.
        var options = BuildOptions(snapshotSource: "MissingSource", defaultTarget: "AdventureWorksE2E");
        var connectionStrings = new Dictionary<string, string?>
        {
            ["AdventureWorksE2E"] = "Server=otherhost;Database=AdventureWorks_E2E;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options).Validate(options, "AdventureWorksE2E", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be("Rule3_SourceKeyMissing");
        outcome.Reason.Should().Contain("MissingSource");
    }

    [Fact]
    public void Validate_WhenTargetDatabaseNameDoesNotMatchPattern_ReturnsRule4Failure()
    {
        var options = BuildOptions(
            snapshotSource: "AdventureWorksDev",
            defaultTarget: "AdventureWorksProd",
            targetNamePattern: TestTargetPattern);
        var connectionStrings = new Dictionary<string, string?>
        {
            ["AdventureWorksDev"] = "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;",
            ["AdventureWorksProd"] = "Server=otherhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options).Validate(options, "AdventureWorksProd", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule4_TargetNamePatternViolation);
    }

    [Fact]
    public void Validate_WhenSourceMarkerProbeReportsTrueOnTarget_ReturnsRule5Failure()
    {
        var options = BuildOptions(
            snapshotSource: "AdventureWorksDev",
            defaultTarget: "AdventureWorksE2E",
            targetNamePattern: TestTargetPattern);
        var connectionStrings = new Dictionary<string, string?>
        {
            ["AdventureWorksDev"] = "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;",
            ["AdventureWorksE2E"] = "Server=otherhost;Database=AdventureWorks_E2E;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options, markerReturns: true)
            .Validate(options, "AdventureWorksE2E", connectionStrings);

        outcome.Ok.Should().BeFalse();
        outcome.FailedRule.Should().Be(DualRoleSafetyMessages.Rule5_SourceMarkerPresent);
    }

    [Fact]
    public void Validate_WhenAllRulesPass_ReturnsSuccess()
    {
        var options = BuildOptions(
            snapshotSource: "AdventureWorksDev",
            defaultTarget: "AdventureWorksE2E",
            targetNamePattern: TestTargetPattern);
        var connectionStrings = new Dictionary<string, string?>
        {
            ["AdventureWorksDev"] = "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;",
            ["AdventureWorksE2E"] = "Server=otherhost;Database=AdventureWorks_E2E;Trusted_Connection=True;TrustServerCertificate=True;",
        };

        var outcome = BuildValidator(options).Validate(options, "AdventureWorksE2E", connectionStrings);

        outcome.Ok.Should().BeTrue();
        outcome.FailedRule.Should().BeNull();
    }
}
