namespace AdventureWorks.Connections;

public interface IConnectionCatalog
{
    ResolvedConnection Get(string name);

    ResolvedConnection Get(string name, ConnectionKind expectedKind);

    bool TryGet(string name, out ResolvedConnection? connection);

    IReadOnlyList<ResolvedConnection> GetByKind(ConnectionKind kind);
}
