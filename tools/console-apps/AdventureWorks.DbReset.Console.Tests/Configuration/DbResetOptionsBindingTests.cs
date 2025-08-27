using AdventureWorks.DbReset.Console.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AdventureWorks.DbReset.Console.Tests.Configuration;

public sealed class DbResetOptionsBindingTests
{
    [Fact]
    public void SectionName_IsDbReset()
    {
        DbResetOptions.SectionName.Should().Be("DbReset");
    }

    [Fact]
    public void Bind_FromDbResetSection_PopulatesAllScalarFields()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DbReset:SnapshotSource"] = "AdventureWorksDev",
                ["DbReset:DefaultTarget"] = "AdventureWorksE2E",
                ["DbReset:BaselinePath"] = "baselines/baseline.bak",
                ["DbReset:TargetNamePattern"] = "^AdventureWorks_(E2E|Test)$",
                ["DbReset:DbUpProjectPath"] = "database/dbup/AdventureWorks.DbUp",
            })
            .Build();

        var options = new DbResetOptions();
        configuration.GetSection(DbResetOptions.SectionName).Bind(options);

        options.SnapshotSource.Should().Be("AdventureWorksDev");
        options.DefaultTarget.Should().Be("AdventureWorksE2E");
        options.BaselinePath.Should().Be("baselines/baseline.bak");
        options.TargetNamePattern.Should().Be("^AdventureWorks_(E2E|Test)$");
        options.DbUpProjectPath.Should().Be("database/dbup/AdventureWorks.DbUp");
    }

    [Fact]
    public void Bind_FromDbResetSection_PopulatesNestedSourceMarker()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DbReset:SourceMarker:Property"] = "custom.role",
                ["DbReset:SourceMarker:Value"] = "primary-source",
            })
            .Build();

        var options = new DbResetOptions();
        configuration.GetSection(DbResetOptions.SectionName).Bind(options);

        options.SourceMarker.Should().NotBeNull();
        options.SourceMarker.Property.Should().Be("custom.role");
        options.SourceMarker.Value.Should().Be("primary-source");
    }

    [Fact]
    public void Bind_WhenSourceMarkerSectionOmitted_PreservesDefaults()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DbReset:SnapshotSource"] = "AdventureWorksDev",
            })
            .Build();

        var options = new DbResetOptions();
        configuration.GetSection(DbResetOptions.SectionName).Bind(options);

        options.SourceMarker.Property.Should().Be("dbreset.role");
        options.SourceMarker.Value.Should().Be("source");
    }

    [Fact]
    public void Bind_FromWrongSectionName_LeavesScalarFieldsAsDefaults()
    {
        // Confirms the binder is keyed off "DbReset", not e.g. "DbResetOptions".
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DbResetOptions:SnapshotSource"] = "ShouldNotBind",
            })
            .Build();

        var options = new DbResetOptions();
        configuration.GetSection(DbResetOptions.SectionName).Bind(options);

        options.SnapshotSource.Should().Be(string.Empty);
    }
}
