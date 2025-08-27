namespace AdventureWorks.DbReset.Console.Safety;

/// <summary>
/// Result of a <see cref="DualRoleSafetyValidator.Validate"/> call. On failure, carries the
/// failing rule key (suitable for log aggregation / metrics) and an operator-facing reason.
/// </summary>
/// <remarks>
/// Shape parallels <c>ConfigurationValidationResult</c>. The two are deliberately not unified
/// because they serve different layers (config vs. safety) and consolidating into a generic
/// <c>Result&lt;T&gt;</c> would obscure that. If a third validator type lands, reconsider.
/// </remarks>
internal sealed class SafetyOutcome
{
    /// <summary>
    /// <c>true</c> when every rule passed; <c>false</c> when one rule failed.
    /// </summary>
    public bool Ok { get; init; }

    /// <summary>
    /// Stable identifier of the rule that failed (e.g. <c>Rule1_SameConnectionStringKey</c>).
    /// Null on success. See <c>DualRoleSafetyMessages</c> for the full set.
    /// </summary>
    public string? FailedRule { get; init; }

    /// <summary>
    /// Human-readable explanation of the failure, suitable for stderr / log output. Null on success.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Convenience factory for the all-rules-passed case.
    /// </summary>
    public static SafetyOutcome Success() => new() { Ok = true };

    /// <summary>
    /// Convenience factory for a rule failure.
    /// </summary>
    /// <param name="rule">Stable rule identifier from <c>DualRoleSafetyMessages</c>.</param>
    /// <param name="reason">Operator-facing reason string.</param>
    public static SafetyOutcome Fail(string rule, string reason) => new()
    {
        Ok = false,
        FailedRule = rule,
        Reason = reason,
    };
}
