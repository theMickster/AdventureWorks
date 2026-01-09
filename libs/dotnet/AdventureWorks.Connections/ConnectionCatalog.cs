namespace AdventureWorks.Connections;

internal sealed class ConnectionCatalog : IConnectionCatalog
{
    private readonly Dictionary<string, ResolvedConnection> _connections;

    internal ConnectionCatalog(IReadOnlyDictionary<string, ResolvedConnection> connections)
    {
        ArgumentNullException.ThrowIfNull(connections);
        _connections = new Dictionary<string, ResolvedConnection>(connections, StringComparer.Ordinal);
    }

    public ResolvedConnection Get(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (!_connections.TryGetValue(name, out var connection))
        {
            throw new ConnectionNotFoundException(name, _connections.Keys);
        }

        return connection;
    }

    public ResolvedConnection Get(string name, ConnectionKind expectedKind)
    {
        var connection = Get(name);

        if (connection.Kind != expectedKind)
        {
            throw new ConnectionKindMismatchException(name, expectedKind, connection.Kind);
        }

        return connection;
    }

    public bool TryGet(string name, out ResolvedConnection? connection)
    {
        ArgumentNullException.ThrowIfNull(name);
        return _connections.TryGetValue(name, out connection);
    }

    public IReadOnlyList<ResolvedConnection> GetByKind(ConnectionKind kind) =>
        _connections.Values.Where(c => c.Kind == kind).ToList();
}
