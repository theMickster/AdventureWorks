namespace AdventureWorks.DbReset.Console.Snapshot;

/// <summary>
/// Mutually-exclusive enumeration of corrective actions taken by
/// <see cref="IDatabaseSnapshotProvider.SelfHealStuckTargetAsync"/>. Not <c>[Flags]</c> —
/// <see cref="Both"/> is its own value.
/// </summary>
public enum RecoveryAction
{
    /// <summary>No corrective action was needed.</summary>
    None,

    /// <summary>The target was in <c>RESTORING</c> state and a <c>RESTORE … WITH RECOVERY</c> brought it online.</summary>
    RecoveredFromRestoring,

    /// <summary>The target was in <c>SINGLE_USER</c> mode and a <c>SET MULTI_USER</c> restored multi-user access.</summary>
    RestoredMultiUser,

    /// <summary>Both <see cref="RecoveredFromRestoring"/> and <see cref="RestoredMultiUser"/> applied.</summary>
    Both,
}

/// <summary>
/// Result of <see cref="IDatabaseSnapshotProvider.SelfHealStuckTargetAsync"/>. <see cref="StateChanged"/>
/// is <c>false</c> if and only if <see cref="Action"/> is <see cref="RecoveryAction.None"/>.
/// </summary>
public sealed record RecoveryOutcome(RecoveryAction Action, bool StateChanged, string? Detail = null)
{
    /// <summary>Constructs a no-op outcome for the case where no corrective action was needed.</summary>
    public static RecoveryOutcome NoOp() => new(RecoveryAction.None, false);
}
