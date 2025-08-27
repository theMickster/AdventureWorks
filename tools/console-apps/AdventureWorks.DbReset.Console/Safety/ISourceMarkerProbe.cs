namespace AdventureWorks.DbReset.Console.Safety;

/// <summary>
/// Inspects a target database for the source-role extended property. Drives Rule #5 of
/// <see cref="DualRoleSafetyValidator"/>: a target that has been branded as a source must never
/// be written to.
/// </summary>
/// <remarks>
/// Story #924 ships a conservative stub (<c>SqlSourceMarkerProbe</c>) that returns <c>false</c>
/// for every input — Rule #5 cannot trip until the SQL implementation lands in Story #925, which
/// will execute against <c>sys.extended_properties</c>. The interface is split out so tests can
/// drive Rule #5 paths deterministically without a live database.
/// </remarks>
public interface ISourceMarkerProbe
{
    /// <summary>
    /// Returns <c>true</c> when the database referenced by <paramref name="connectionString"/>
    /// has an extended property named <paramref name="property"/> whose value matches
    /// <paramref name="value"/>. Implementations must not mutate state.
    /// </summary>
    /// <param name="connectionString">Connection string for the target database.</param>
    /// <param name="property">Extended-property name (e.g. <c>dbreset.role</c>).</param>
    /// <param name="value">Extended-property value indicating source role (e.g. <c>source</c>).</param>
    bool HasMarker(string connectionString, string property, string value);
}
