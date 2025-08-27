using System.Text.RegularExpressions;
using AdventureWorks.DbReset.Console.Configuration;
using Microsoft.Data.SqlClient;

namespace AdventureWorks.DbReset.Console.Safety;

/// <summary>
/// Enforces the dual-role safety contract that prevents this tool from ever writing to a
/// source-of-truth database. Runs five independent rules against the resolved target before any
/// destructive verb (restore, migrate, snapshot, reset) is allowed to proceed.
/// </summary>
/// <remarks>
/// <para>
/// Implements the safety model defined for ADO Feature #923 / Story #924. The five rules:
/// </para>
/// <list type="number">
///   <item><description><b>Rule #1</b> — <c>SnapshotSource</c> and the resolved target must reference different <c>ConnectionStrings</c> keys.</description></item>
///   <item><description><b>Rule #2</b> — Source and target must not resolve to the same <c>(Server, Database)</c> pair (catches aliased keys pointing at the same physical DB).</description></item>
///   <item><description><b>Rule #3</b> — Both the source and target <c>ConnectionStrings</c> entries must exist and be non-empty.</description></item>
///   <item><description><b>Rule #4</b> — The target's <c>InitialCatalog</c> must match <see cref="DbResetOptions.TargetNamePattern"/> (e.g. opt-in names like <c>AdventureWorks_E2E</c>).</description></item>
///   <item><description><b>Rule #5</b> — The target database must NOT carry the configured <see cref="SourceMarkerOptions"/> extended property; if it does, it has been branded as a source.</description></item>
/// </list>
/// <para>
/// Rules are evaluated in the order above; the first failure short-circuits and is reported via
/// <see cref="SafetyOutcome"/>. Rule keys and reason strings live in <c>DualRoleSafetyMessages</c>.
/// </para>
/// </remarks>
internal sealed class DualRoleSafetyValidator
{
    private readonly ISourceMarkerProbe _markerProbe;

    // Compiled once from the singleton DbResetOptions injected at ctor time.
    // Validate() also receives DbResetOptions for back-compat with tests; in production
    // the runtime singleton's TargetNamePattern is identical to what's passed in.
    private readonly Lazy<Regex> _targetNameRegex;

    /// <summary>
    /// Initializes a validator bound to the runtime <see cref="DbResetOptions"/> singleton.
    /// The configured <see cref="DbResetOptions.TargetNamePattern"/> is compiled lazily on first
    /// <see cref="Validate"/> call and reused for the lifetime of the instance.
    /// </summary>
    /// <param name="markerProbe">Probe used to evaluate Rule #5 against the target database.</param>
    /// <param name="options">Bound <c>DbReset</c> configuration; supplies the regex pattern.</param>
    /// <exception cref="ArgumentNullException">Either argument is <c>null</c>.</exception>
    public DualRoleSafetyValidator(ISourceMarkerProbe markerProbe, DbResetOptions options)
    {
        ArgumentNullException.ThrowIfNull(markerProbe);
        ArgumentNullException.ThrowIfNull(options);
        _markerProbe = markerProbe;
        _targetNameRegex = new Lazy<Regex>(
            () => new Regex(options.TargetNamePattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Runs Rules #1–#5 against the supplied target. Returns on the first failure; only an
    /// all-rules-pass run yields <see cref="SafetyOutcome.Success"/>.
    /// </summary>
    /// <param name="options">Resolved <c>DbReset</c> configuration. Tests pass an instance directly; in production this is the same singleton supplied to the constructor.</param>
    /// <param name="effectiveTargetName">The target <c>ConnectionStrings</c> key to write to — either the CLI <c>--target</c> override or <see cref="DbResetOptions.DefaultTarget"/>.</param>
    /// <param name="connectionStrings">Snapshot of the <c>ConnectionStrings</c> section. Keys are case-sensitive matches against <c>SnapshotSource</c> / <paramref name="effectiveTargetName"/>.</param>
    /// <returns>
    /// <see cref="SafetyOutcome.Success"/> when every rule passes; otherwise
    /// <see cref="SafetyOutcome.Fail"/> populated with the first failed rule key and a human
    /// readable reason. Callers should map a failure to <see cref="DbResetDefaults.ExitSafetyRefused"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
    public SafetyOutcome Validate(
        DbResetOptions options,
        string effectiveTargetName,
        IReadOnlyDictionary<string, string?> connectionStrings)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(effectiveTargetName);
        ArgumentNullException.ThrowIfNull(connectionStrings);

        // Rule #1 — same ConnectionStrings key for source and target.
        if (string.Equals(options.SnapshotSource, effectiveTargetName, StringComparison.OrdinalIgnoreCase))
        {
            return SafetyOutcome.Fail(
                DualRoleSafetyMessages.Rule1_SameKey,
                DualRoleSafetyMessages.Rule1_Reason);
        }

        // Rule #3 — target key missing or empty. Run before Rule #2 so we can parse both strings.
        if (!connectionStrings.TryGetValue(effectiveTargetName, out var targetCs)
            || string.IsNullOrWhiteSpace(targetCs))
        {
            return SafetyOutcome.Fail(
                DualRoleSafetyMessages.Rule3_MissingTargetKey,
                string.Format(DualRoleSafetyMessages.Rule3_ReasonFormat, effectiveTargetName));
        }

        // Source CS must also be resolvable to even consider Rule #2.
        if (!connectionStrings.TryGetValue(options.SnapshotSource, out var sourceCs)
            || string.IsNullOrWhiteSpace(sourceCs))
        {
            return SafetyOutcome.Fail(
                DualRoleSafetyMessages.Rule3_MissingSourceKey,
                string.Format(DualRoleSafetyMessages.Rule3_MissingSourceReasonFormat, options.SnapshotSource));
        }

        var sourceBuilder = new SqlConnectionStringBuilder(sourceCs);
        var targetBuilder = new SqlConnectionStringBuilder(targetCs);

        // Rule #2 — same (Server, Database) pair.
        if (string.Equals(sourceBuilder.DataSource, targetBuilder.DataSource, StringComparison.OrdinalIgnoreCase)
            && string.Equals(sourceBuilder.InitialCatalog, targetBuilder.InitialCatalog, StringComparison.OrdinalIgnoreCase))
        {
            return SafetyOutcome.Fail(
                DualRoleSafetyMessages.Rule2_SameServerDatabase,
                string.Format(
                    DualRoleSafetyMessages.Rule2_ReasonFormat,
                    targetBuilder.DataSource,
                    targetBuilder.InitialCatalog));
        }

        // Rule #4 — target database NAME (not key) must match the configured pattern.
        if (!_targetNameRegex.Value.IsMatch(targetBuilder.InitialCatalog))
        {
            return SafetyOutcome.Fail(
                DualRoleSafetyMessages.Rule4_TargetNamePatternViolation,
                string.Format(
                    DualRoleSafetyMessages.Rule4_ReasonFormat,
                    targetBuilder.InitialCatalog,
                    options.TargetNamePattern));
        }

        // Rule #5 — target carries the source-role extended property.
        if (_markerProbe.HasMarker(targetCs, options.SourceMarker.Property, options.SourceMarker.Value))
        {
            return SafetyOutcome.Fail(
                DualRoleSafetyMessages.Rule5_SourceMarkerPresent,
                string.Format(
                    DualRoleSafetyMessages.Rule5_ReasonFormat,
                    effectiveTargetName,
                    options.SourceMarker.Property,
                    options.SourceMarker.Value));
        }

        return SafetyOutcome.Success();
    }
}
