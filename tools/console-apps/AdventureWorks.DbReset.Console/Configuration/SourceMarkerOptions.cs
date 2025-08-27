namespace AdventureWorks.DbReset.Console.Configuration;

/// <summary>
/// Identifies the SQL extended property that brands a database as a read-only source (snapshot
/// origin). When the tool inspects a candidate target and finds this property set to the configured
/// <see cref="Value"/>, it refuses to write — Rule #5 of the dual-role safety check in
/// <see cref="Safety.DualRoleSafetyValidator"/>.
/// </summary>
/// <remarks>
/// Defaults match the <c>appsettings.json</c> shipped with the tool (<c>dbreset.role</c> /
/// <c>source</c>). Operators brand a source database by adding the extended property out-of-band:
/// <code>EXEC sp_addextendedproperty @name = N'dbreset.role', @value = N'source';</code>
/// </remarks>
public sealed class SourceMarkerOptions
{
    /// <summary>
    /// Extended-property name probed on the target database. Default: <c>dbreset.role</c>.
    /// </summary>
    public string Property { get; set; } = "dbreset.role";

    /// <summary>
    /// Extended-property value that, when present on the target, indicates a source-role database
    /// and trips Rule #5. Default: <c>source</c>.
    /// </summary>
    public string Value { get; set; } = "source";
}
