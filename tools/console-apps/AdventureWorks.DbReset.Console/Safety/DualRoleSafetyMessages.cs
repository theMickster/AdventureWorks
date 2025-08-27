namespace AdventureWorks.DbReset.Console.Safety;

/// <summary>
/// Stable rule keys and reason-format strings used by <see cref="DualRoleSafetyValidator"/>.
/// Keys are emitted as-is in <see cref="SafetyOutcome.FailedRule"/> so log aggregation can pivot
/// on them; reason formats are operator-facing and may be tweaked without breaking consumers.
/// </summary>
internal static class DualRoleSafetyMessages
{
    public const string Rule1_SameKey = "Rule1_SameConnectionStringKey";

    public const string Rule2_SameServerDatabase = "Rule2_SameServerDatabasePair";

    public const string Rule3_MissingTargetKey = "Rule3_TargetKeyMissing";

    public const string Rule3_MissingSourceKey = "Rule3_SourceKeyMissing";

    public const string Rule4_TargetNamePatternViolation = "Rule4_TargetNamePatternViolation";

    public const string Rule5_SourceMarkerPresent = "Rule5_SourceMarkerOnTarget";

    public const string Rule1_Reason = "SnapshotSource and the resolved target reference the same ConnectionStrings entry.";

    public const string Rule2_ReasonFormat = "SnapshotSource and target resolve to the same (Server, Database) pair: {0} / {1}.";

    public const string Rule3_ReasonFormat = "Target ConnectionStrings entry '{0}' is missing.";

    public const string Rule3_MissingSourceReasonFormat = "SnapshotSource ConnectionStrings entry '{0}' is missing.";

    public const string Rule4_ReasonFormat = "Target database name '{0}' does not match TargetNamePattern '{1}'.";

    public const string Rule5_ReasonFormat = "Target '{0}' carries the source marker ({1}={2}); refusing to write to a source database.";
}
