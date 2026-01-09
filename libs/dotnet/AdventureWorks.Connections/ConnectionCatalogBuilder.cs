using Microsoft.Extensions.Configuration;

namespace AdventureWorks.Connections;

public sealed class ConnectionCatalogBuilder
{
    private readonly List<ConnectionDescriptor> _descriptors = [];

    public ConnectionCatalogBuilder Add(string name, ConnectionKind kind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _descriptors.Add(new ConnectionDescriptor(name, kind));
        return this;
    }

    public IConnectionCatalog Build(IConfiguration configuration, string sectionName = "ConnectionStrings")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        var section = configuration.GetSection(sectionName);

        var connections = new Dictionary<string, ResolvedConnection>(StringComparer.Ordinal);

        foreach (var descriptor in _descriptors)
        {
            var value = section[descriptor.Name];

            if (value is not null)
            {
                connections[descriptor.Name] = new ResolvedConnection(descriptor.Name, descriptor.Kind, value);
            }
        }

        return new ConnectionCatalog(connections);
    }
}
