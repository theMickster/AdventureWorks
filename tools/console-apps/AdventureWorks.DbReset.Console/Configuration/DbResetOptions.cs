namespace AdventureWorks.DbReset.Console.Configuration;

/// <summary>
/// Strongly-typed binding for the <c>DbReset</c> section of <c>appsettings.json</c>.
/// </summary>
/// <remarks>
/// Bound at startup via <c>IOptions&lt;DbResetOptions&gt;</c>. The expected configuration shape:
/// <code>
/// {
///   "ConnectionStrings": {
///     "AdventureWorksDev": "...",
///     "AdventureWorksE2E": "..."
///   },
///   "DbReset": {
///     "SnapshotSource": "AdventureWorksDev",
///     "DefaultTarget":  "AdventureWorksE2E",
///     "BaselinePath":   "tools/console-apps/AdventureWorks.DbReset.Console/baselines/AdventureWorks_baseline.bak",
///     "TargetNamePattern": "^AdventureWorks_(E2E|Test|Load)([A-Za-z0-9_]*)?$",
///     "DbUpProjectPath": "database/dbup/AdventureWorks.DbUp",
///     "SourceMarker": { "Property": "dbreset.role", "Value": "source" }
///   }
/// }
/// </code>
/// Part of the dual-role safety model introduced for ADO Feature #923 / Story #924.
/// </remarks>
public sealed class DbResetOptions
{
    /// <summary>
    /// Configuration section name (<c>"DbReset"</c>) consumed by <c>configuration.GetSection(...)</c>.
    /// </summary>
    public const string SectionName = "DbReset";

    /// <summary>
    /// Name of the <c>ConnectionStrings</c> entry that points at the read-only source database
    /// (the snapshot origin). Never written to by this tool.
    /// </summary>
    public string SnapshotSource { get; set; } = string.Empty;

    /// <summary>
    /// Name of the <c>ConnectionStrings</c> entry used as the destination when no <c>--target</c>
    /// override is supplied on the command line. Must differ from <see cref="SnapshotSource"/>;
    /// see Rule #1 in <see cref="Safety.DualRoleSafetyValidator"/>.
    /// </summary>
    public string DefaultTarget { get; set; } = string.Empty;

    /// <summary>
    /// Repository-relative path to the baseline <c>.bak</c> snapshot used for restore operations.
    /// Resolved against the discovered repo root (see <see cref="Resolution.RepoRootResolver"/>).
    /// </summary>
    public string BaselinePath { get; set; } = string.Empty;

    /// <summary>
    /// Regex matched against the target database NAME (the <c>InitialCatalog</c> of the target
    /// connection string) — not the configuration key. Constrains targets to opt-in naming such
    /// as <c>AdventureWorks_E2E</c> or <c>AdventureWorks_Test*</c>; enforces Rule #4 of the
    /// dual-role safety check.
    /// </summary>
    public string TargetNamePattern { get; set; } = string.Empty;

    /// <summary>
    /// Repository-relative path to the DbUp migration project, used by the <c>migrate</c> verb
    /// to apply ordered SQL scripts after a restore.
    /// </summary>
    public string DbUpProjectPath { get; set; } = string.Empty;

    /// <summary>
    /// Names the SQL extended property that distinguishes a source-role database from a writable
    /// target. Drives Rule #5 of the dual-role safety check.
    /// </summary>
    public SourceMarkerOptions SourceMarker { get; set; } = new();
}
