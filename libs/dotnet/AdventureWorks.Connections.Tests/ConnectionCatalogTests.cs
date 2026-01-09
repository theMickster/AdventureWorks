using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AdventureWorks.Connections.Tests;

public sealed class ConnectionCatalogTests
{
    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] connectionStrings)
    {
        var data = connectionStrings.ToDictionary(
            x => $"ConnectionStrings:{x.Key}",
            x => (string?)x.Value);

        return new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
    }

    [Fact]
    public void Get_ResolvesConfiguredConnection()
    {
        var configuration = BuildConfiguration((ConnectionNames.AdventureWorks, "Server=.;Database=AdventureWorks;"));

        var catalog = new ConnectionCatalogBuilder()
            .Add(ConnectionNames.AdventureWorks, ConnectionKind.Sql)
            .Build(configuration);

        var connection = catalog.Get(ConnectionNames.AdventureWorks);

        connection.Name.Should().Be(ConnectionNames.AdventureWorks);
        connection.Kind.Should().Be(ConnectionKind.Sql);
        connection.Value.Should().Be("Server=.;Database=AdventureWorks;");
    }

    [Fact]
    public void Get_ThrowsConnectionNotFoundException_WhenNameIsMissing()
    {
        var configuration = BuildConfiguration();

        var catalog = new ConnectionCatalogBuilder()
            .Add(ConnectionNames.AdventureWorks, ConnectionKind.Sql)
            .Build(configuration);

        var act = () => catalog.Get(ConnectionNames.AdventureWorks);

        act.Should().Throw<ConnectionNotFoundException>();
    }

    [Fact]
    public void Get_WithExpectedKind_ThrowsConnectionKindMismatchException_OnMismatch()
    {
        var configuration = BuildConfiguration((ConnectionNames.ServiceBus, "Endpoint=sb://example/;"));

        var catalog = new ConnectionCatalogBuilder()
            .Add(ConnectionNames.ServiceBus, ConnectionKind.ServiceBus)
            .Build(configuration);

        var act = () => catalog.Get(ConnectionNames.ServiceBus, ConnectionKind.Sql);

        act.Should().Throw<ConnectionKindMismatchException>();
    }

    [Fact]
    public void TryGet_ReturnsFalse_WhenNameIsNotConfigured()
    {
        var configuration = BuildConfiguration();

        var catalog = new ConnectionCatalogBuilder()
            .Add(ConnectionNames.AdventureWorks, ConnectionKind.Sql)
            .Build(configuration);

        var found = catalog.TryGet(ConnectionNames.AdventureWorks, out var connection);

        found.Should().BeFalse();
        connection.Should().BeNull();
    }

    [Fact]
    public void GetByKind_ReturnsOnlyConnectionsOfThatKind()
    {
        var configuration = BuildConfiguration(
            (ConnectionNames.AdventureWorks, "Server=.;Database=AdventureWorks;"),
            (ConnectionNames.ServiceBus, "Endpoint=sb://example/;"));

        var catalog = new ConnectionCatalogBuilder()
            .Add(ConnectionNames.AdventureWorks, ConnectionKind.Sql)
            .Add(ConnectionNames.ServiceBus, ConnectionKind.ServiceBus)
            .Build(configuration);

        var sqlConnections = catalog.GetByKind(ConnectionKind.Sql);

        sqlConnections.Should().ContainSingle(c => c.Name == ConnectionNames.AdventureWorks);
    }

    [Fact]
    public void EnsureNonEmpty_Throws_WhenValueIsBlank()
    {
        var connection = new ResolvedConnection(ConnectionNames.AdventureWorks, ConnectionKind.Sql, "   ");

        var act = () => ConnectionValidation.EnsureNonEmpty(connection);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EnsureNonEmpty_DoesNotThrow_WhenValueIsPresent()
    {
        var connection = new ResolvedConnection(ConnectionNames.AdventureWorks, ConnectionKind.Sql, "Server=.;");

        var act = () => ConnectionValidation.EnsureNonEmpty(connection);

        act.Should().NotThrow();
    }
}
