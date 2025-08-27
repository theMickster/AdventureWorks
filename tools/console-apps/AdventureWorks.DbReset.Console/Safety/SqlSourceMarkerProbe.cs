namespace AdventureWorks.DbReset.Console.Safety;

// Stub for ADO Story #924; the real SQL-backed implementation ships in Story #925.
public sealed class SqlSourceMarkerProbe : ISourceMarkerProbe
{
    public bool HasMarker(string connectionString, string property, string value)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(property);
        ArgumentNullException.ThrowIfNull(value);

        // TODO (Story #925): execute against the target DB:
        //   SELECT value FROM sys.extended_properties
        //   WHERE class = 0 AND name = @property AND CONVERT(nvarchar(4000), value) = @value
        // For Story #924 we conservatively return false (no marker => Rule #5 cannot trip).
        return false;
    }
}
