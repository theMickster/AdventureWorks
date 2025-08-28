using System.Data;
using System.Globalization;

namespace AdventureWorks.DbReset.Console.Snapshot;

/// <summary>
/// Projects an open <see cref="IDataReader"/> (the result of <c>RESTORE FILELISTONLY</c>) into
/// <see cref="BackupFileMetadata"/> rows. Synchronous on purpose — <c>IDataReader.Read</c> is
/// sync; <c>ISqlScriptExecutor</c> owns the surrounding async I/O.
/// </summary>
internal static class BackupFileListReader
{
    /// <summary>
    /// Reads every row in <paramref name="reader"/>. Columns are looked up by name (Risk #4).
    /// <c>Type</c> is <c>CHAR(1)</c> so its <see cref="string"/> form is dereferenced at <c>[0]</c>
    /// (Risk #5). The caller owns the reader's lifecycle.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">A required column is absent or has an invalid value.</exception>
    public static IReadOnlyList<BackupFileMetadata> Read(IDataReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var rows = new List<BackupFileMetadata>();
        while (reader.Read())
        {
            string logicalName;
            string physicalName;
            string typeRaw;
            long size;
            try
            {
                logicalName = (string)reader["LogicalName"];
                physicalName = (string)reader["PhysicalName"];
                typeRaw = (string)reader["Type"];
                size = Convert.ToInt64(reader["Size"], CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (ex is IndexOutOfRangeException
                or ArgumentException
                or OverflowException
                or FormatException)
            {
                // SqlDataReader throws IndexOutOfRangeException for unknown column names;
                // DataTableReader (used in unit tests) throws ArgumentException.
                // Numeric conversion of a malformed Size value surfaces as OverflowException
                // or FormatException. Treat all four as a single actionable error.
                throw new InvalidOperationException(
                    "RESTORE FILELISTONLY result has a missing or invalid required column "
                    + "(LogicalName, PhysicalName, Type, Size).",
                    ex);
            }

            if (typeRaw.Length == 0)
            {
                throw new InvalidOperationException(
                    $"RESTORE FILELISTONLY row '{logicalName}' has empty Type column.");
            }

            rows.Add(new BackupFileMetadata(logicalName, physicalName, typeRaw[0], size));
        }

        return rows;
    }
}
