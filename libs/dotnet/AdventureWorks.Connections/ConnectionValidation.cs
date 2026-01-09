namespace AdventureWorks.Connections;

public static class ConnectionValidation
{
    public static void EnsureNonEmpty(ResolvedConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (string.IsNullOrWhiteSpace(connection.Value))
        {
            throw new InvalidOperationException(
                $"Connection '{connection.Name}' ({connection.Kind}) is configured but its value is empty.");
        }
    }
}
