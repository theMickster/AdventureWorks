using System.Text.RegularExpressions;

namespace AdventureWorks.DbReset.Console.Configuration;

/// <summary>
/// Result of a <see cref="ConfigurationValidator.Validate"/> call. <see cref="Ok"/> indicates
/// whether <c>appsettings.json</c> is structurally usable; <see cref="Reason"/> carries a single
/// operator-facing message (validator returns on first failure).
/// </summary>
internal sealed class ConfigurationValidationResult
{
    /// <summary><c>true</c> when configuration is well-formed; <c>false</c> on the first detected problem.</summary>
    public bool Ok { get; init; }

    /// <summary>Operator-facing description of the first failure. Null on success.</summary>
    public string? Reason { get; init; }

    /// <summary>Convenience factory for the success case.</summary>
    public static ConfigurationValidationResult Success() => new() { Ok = true };

    /// <summary>Convenience factory for a failure with <paramref name="reason"/>.</summary>
    public static ConfigurationValidationResult Fail(string reason) => new() { Ok = false, Reason = reason };
}

/// <summary>
/// Validates the structural shape of <see cref="DbResetOptions"/> and the associated
/// <c>ConnectionStrings</c> section before any verb runs. Checks required strings are non-empty,
/// referenced connection-string entries exist, and <see cref="DbResetOptions.TargetNamePattern"/>
/// compiles as a regex.
/// </summary>
/// <remarks>
/// Distinct from <see cref="Safety.DualRoleSafetyValidator"/>: this validator is concerned with
/// "is the config usable at all", while the safety validator is concerned with "is it safe to
/// write to the resolved target". Failures here map to <see cref="DbResetDefaults.ExitConfigInvalid"/>.
/// </remarks>
internal sealed class ConfigurationValidator
{
    /// <summary>
    /// Returns the first configuration problem encountered, or success when none.
    /// </summary>
    /// <param name="options">Bound <c>DbReset</c> options.</param>
    /// <param name="connectionStrings">Snapshot of the <c>ConnectionStrings</c> section.</param>
    /// <returns>A populated <see cref="ConfigurationValidationResult"/>.</returns>
    /// <exception cref="ArgumentNullException">Either argument is <c>null</c>.</exception>
    public ConfigurationValidationResult Validate(
        DbResetOptions options,
        IReadOnlyDictionary<string, string?> connectionStrings)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(connectionStrings);

        if (string.IsNullOrWhiteSpace(options.SnapshotSource))
        {
            return ConfigurationValidationResult.Fail("DbReset:SnapshotSource is required.");
        }

        if (string.IsNullOrWhiteSpace(options.DefaultTarget))
        {
            return ConfigurationValidationResult.Fail("DbReset:DefaultTarget is required.");
        }

        if (string.IsNullOrWhiteSpace(options.BaselinePath))
        {
            return ConfigurationValidationResult.Fail("DbReset:BaselinePath is required.");
        }

        if (string.IsNullOrWhiteSpace(options.TargetNamePattern))
        {
            return ConfigurationValidationResult.Fail("DbReset:TargetNamePattern is required.");
        }

        if (string.IsNullOrWhiteSpace(options.DbUpProjectPath))
        {
            return ConfigurationValidationResult.Fail("DbReset:DbUpProjectPath is required.");
        }

        if (!connectionStrings.TryGetValue(options.SnapshotSource, out var sourceCs)
            || string.IsNullOrWhiteSpace(sourceCs))
        {
            return ConfigurationValidationResult.Fail(
                $"ConnectionStrings:{options.SnapshotSource} is missing (referenced by DbReset:SnapshotSource).");
        }

        if (!connectionStrings.TryGetValue(options.DefaultTarget, out var targetCs)
            || string.IsNullOrWhiteSpace(targetCs))
        {
            return ConfigurationValidationResult.Fail(
                $"ConnectionStrings:{options.DefaultTarget} is missing (referenced by DbReset:DefaultTarget).");
        }

        try
        {
            _ = new Regex(options.TargetNamePattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        }
        catch (ArgumentException ex)
        {
            return ConfigurationValidationResult.Fail(
                $"DbReset:TargetNamePattern is not a valid regex: {ex.Message}");
        }

        return ConfigurationValidationResult.Success();
    }
}
