namespace AdventureWorks.Connections;

public sealed class ConnectionNotFoundException : InvalidOperationException
{
    public ConnectionNotFoundException(string name, IEnumerable<string> configuredNames)
        : base($"Connection '{name}' was not found in the connection catalog. Configured names: {string.Join(", ", configuredNames)}.")
    {
    }
}
