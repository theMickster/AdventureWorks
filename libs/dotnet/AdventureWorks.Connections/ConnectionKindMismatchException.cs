namespace AdventureWorks.Connections;

public sealed class ConnectionKindMismatchException : InvalidOperationException
{
    public ConnectionKindMismatchException(string name, ConnectionKind expectedKind, ConnectionKind actualKind)
        : base($"Connection '{name}' was expected to be of kind '{expectedKind}' but was configured as '{actualKind}'.")
    {
    }
}
