# Code Review: Local Changes — 2026-05-09

**Date:** 2026-05-09 | **Reviewed by:** Claude Code (claude-opus-4-7)

**Scope:** Story #927 — `restore` verb implementation for `AdventureWorks.DbReset.Console`

## Summary

| Severity      | Count |
| ------------- | ----- |
| 🛑 Blocker    | 0     |
| ⚠️ Important  | 0     |
| ♻️ Refactor   | 2     |
| 💡 Suggestion | 2     |

Clean implementation. The restore verb follows established handler and error-mapper conventions, the safety model is correct, and all 217 tests pass. Two refactors address duplication that will compound with Stories #928 (migrate/reset verbs); two suggestions are style-level fixes.

---

## Findings

### ♻️ Refactor

#### SQL error constants, IsUnreachable logic, and FormatElapsed are duplicated across mapper and handler pairs

`tools/console-apps/AdventureWorks.DbReset.Console/Verbs/Handlers/RestoreErrorMapper.cs:25-32`

<details><summary>Details</summary>

`RestoreErrorMapper` copies verbatim from `SnapshotErrorMapper`:
- Six `private const int` SQL error number constants (lines 25–30, identical values and inline comments)
- `OsErrorFiveMarker` constant (line 32)
- `IsTargetUnreachable` method body (lines 74–93) — identical logic to `IsSourceUnreachable`, only the method name differs

Additionally, `RestoreHandler.FormatElapsed` (lines 68–73) is behaviorally identical to `SnapshotHandler.FormatElapsed` (lines 71–83).

With migrate and reset verbs landing in Story #928, there will be a third and fourth copy. A future change to the recognized error numbers (e.g. adding error 233 for named-pipe failures) must be made in every copy or a silent divergence results.

**Suggested fix:** Extract a shared `internal static class SqlConnectivityClassifier` (under `Verbs/Handlers/Internal/` or `Snapshot/Internal/`) exposing `IsConnectivityFailure(SqlException)`, `ContainsOsError5(SqlException)`, and the shared constant block. Extract `FormatElapsed` to a shared `internal static class ElapsedFormatter`. Each per-verb mapper keeps only its verb-specific message strings and exit codes.
</details>

---

#### SnapshotErrorMapper.SummarizeReason echoes raw SqlException message for Number==0; RestoreErrorMapper was fixed but fix was not backported

`tools/console-apps/AdventureWorks.DbReset.Console/Verbs/Handlers/SnapshotErrorMapper.cs:103-111`

<details><summary>Details</summary>

`SnapshotErrorMapper.SummarizeReason` (lines 103–111):
```csharp
var firstLine = ex.Message.Split('\n', 2, StringSplitOptions.None)[0].Trim();
return string.IsNullOrEmpty(firstLine) ? "connection failed" : firstLine;
```

`RestoreErrorMapper.SummarizeReason` (lines 95–105) has the fix:
```csharp
// Number==0 is a SqlClient transport-layer failure. The driver message can contain
// the server hostname and port, so we substitute a fixed safe string rather than
// echoing it to operator-facing output.
return "transport-level connection failure";
```

The `RestoreErrorMapper` comment explicitly calls out the risk. That same risk applies to snapshot operations: for a Number==0 transport failure, `SnapshotErrorMapper` will echo the first line of the SqlClient driver message — which can contain the server hostname and port — to operator-facing stderr.

**Suggested fix:** Apply the same `return "transport-level connection failure"` pattern to `SnapshotErrorMapper.SummarizeReason`. This resolves automatically if Finding 1 is addressed via extraction, since the shared classifier would use the safe string.
</details>

---

### 💡 Suggestions

#### HealRestoringStateAsync return type exposes raw sys.databases state integer; call site branches on opaque `>= 0` sentinel

`tools/console-apps/AdventureWorks.DbReset.Console/Snapshot/LocalSqlServerSnapshotProvider.cs:270-272`

<details><summary>Details</summary>

```csharp
var dbState = await HealRestoringStateAsync(connection, targetDbQuoted, targetDbName, ct);
if (dbState >= 0)
    await LockSingleUserAsync(connection, targetDbQuoted, ct);
```

`HealRestoringStateAsync` returns `Task<int>` where the return value is either the `sys.databases.state` integer (0–6) or `-1` as a sentinel meaning "database not found in sys.databases." The caller branches on `dbState >= 0` — a reader must navigate into `ReadDatabaseStateAsync` to discover the sentinel. The method name (`HealRestoringState`) implies a side-effecting heal, not an existence check, which adds to the surprise.

**Suggested fix:** Return `bool dbExists` (the only information the call site consumes) and rename the method to `HealRestoringStateIfExistsAsync` or similar. Alternatively, add a one-line comment: `// -1 sentinel: DB does not exist; skip SINGLE_USER lock`.
</details>

---

#### RestoreErrorMapper.TryMap uses string interpolation in the OS-error-5 branch; SnapshotErrorMapper uses string.Format consistently

`tools/console-apps/AdventureWorks.DbReset.Console/Verbs/Handlers/RestoreErrorMapper.cs:53`

<details><summary>Details</summary>

```csharp
// OS-error-5 branch — uses $"..."
var msg = $"SQL Server cannot read baseline file at '{baselinePath}' (OS error 5). ...";

// Unreachable branch — uses string.Format(CultureInfo.InvariantCulture, ...)
var msg = string.Format(CultureInfo.InvariantCulture, "Cannot reach restore target '{0}': ...", targetName, reason);
```

`SnapshotErrorMapper` uses `string.Format(CultureInfo.InvariantCulture, ...)` in both branches. The interpolated string is functionally correct here (no culture-sensitive values), but the inconsistency breaks the explicit `CultureInfo.InvariantCulture` discipline applied everywhere else in these two files.

**Suggested fix:** Replace the `$"..."` with `string.Format(CultureInfo.InvariantCulture, "SQL Server cannot read baseline file at '{0}' (OS error 5). ...", baselinePath)`.
</details>

---

## Reviewed and Dismissed

<details><summary>🔍 3 initial findings dismissed after validation</summary>

#### 🛑 Blocker: SqlSourceMarkerProbe.cs catches error 4060, potentially bypassing Rule #5 when DB exists but login is CONNECT-denied
`tools/console-apps/AdventureWorks.DbReset.Console/Safety/SqlSourceMarkerProbe.cs:68`
**Original confidence:** 82/100
**Dismissed because:** `RESTORE DATABASE` requires `sysadmin` or `CONTROL SERVER`, and `sysadmin` membership grants implicit `CONNECT` on all databases — the "login CONNECT-denied while having RESTORE privilege" scenario is structurally impossible in SQL Server's permission model.

---

#### ♻️ Refactor: RestoreHandler pre-flight BaselineExistsAsync check breaks the established handler pattern
`tools/console-apps/AdventureWorks.DbReset.Console/Verbs/Handlers/RestoreHandler.cs:40`
**Original confidence:** 78/100
**Dismissed because:** The check is intentional — a missing baseline before a restore is a clean, typeable failure that cannot be reliably distinguished from other SQL errors post-fact; `SnapshotHandler` has no equivalent because there is no pre-existing baseline to check before creating one. The asymmetry reflects a domain difference, not a pattern violation.

---

#### ⚠️ Important: `elapsed` declared before try block in RestoreHandler.RunAsync
`tools/console-apps/AdventureWorks.DbReset.Console/Verbs/Handlers/RestoreHandler.cs:49`
**Original confidence:** 88/100
**Dismissed because:** The compiler enforces definite assignment; this is idiomatic C# for capturing a value from inside a try-block and does not meet the bar for any severity level.

</details>
