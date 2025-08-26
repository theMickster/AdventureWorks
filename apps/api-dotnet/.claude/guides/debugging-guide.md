# VS Code Debugging Guide — AdventureWorks API

How to debug the .NET API in VS Code without false-positive breaks on exceptions
that the global exception handler is intentionally catching.

## TL;DR

After cloning the repo, do this once:

1. Start a debug session at least once (so the `coreclr` debugger registers exception filters).
2. Open the **Run & Debug** view (`⇧⌘D`).
3. In the **Breakpoints** section, find **CLR Exceptions → User-Unhandled**.
4. Click the pencil icon to edit its condition and paste:

   ```
   !FluentValidation.ValidationException,System.Collections.Generic.KeyNotFoundException
   ```

5. Press Enter. The leading `!` turns the list into an _ignore_ list — these
   exception types will no longer break the debugger.

## Why this is needed

The API treats certain exceptions as part of the normal request lifecycle:

| Thrown from                                | Type                                   | HTTP result |
| ------------------------------------------ | -------------------------------------- | ----------- |
| Command handlers (`ValidateAndThrowAsync`) | `FluentValidation.ValidationException` | `400`       |
| Query / repository layer                   | `KeyNotFoundException`                 | `404`       |

Both are translated by [`ExceptionHandlerMiddleware`](../../src/AdventureWorks.API/libs/Middleware/ExceptionHandlerMiddleware.cs) into a serialized error body with the `X-Correlation-Id` header. They are expected control flow, not bugs.

With **Just My Code** enabled (the default), the **User-Unhandled Exceptions**
filter breaks whenever an exception leaves user code and is about to be caught
by a framework or non-user assembly — even if your own middleware catches it
further up the stack. The targeted ignore-list above tells that filter to skip
the two types we know are safe.

## Why we don't fix this in `launch.json`

The `coreclr` debug adapter does not expose any exception-filter field in
`launch.json`. (That's a Visual Studio full-IDE feature stored in
`.vs/ExceptionSettings.xml`, which VS Code does not consume.) Exception filter
conditions are stored in per-workspace state under
`~/Library/Application Support/Code/User/workspaceStorage/` and cannot be
checked into source control. This guide is the source of truth — new
developers must run the steps once per clone.

## Adding more exceptions to the ignore list

If a future feature introduces another exception type that the global handler
intentionally catches:

1. Update [`ExceptionHandlerMiddleware`](../../src/AdventureWorks.API/libs/Middleware/ExceptionHandlerMiddleware.cs)
   to translate it to the correct HTTP status. (Per the
   "Exception and Contract Policy" in [`apps/api-dotnet/.claude/CLAUDE.md`](../CLAUDE.md),
   any new expected exception type must be handled in the same change.)
2. Append the fully-qualified type name to the **User-Unhandled** condition.
   The leading `!` applies to the whole list:

   ```
   !FluentValidation.ValidationException,System.Collections.Generic.KeyNotFoundException,AdventureWorks.Common.Exceptions.ConfigurationException
   ```

3. Update this guide so the next developer copies the right list.

## Alternatives considered (and rejected)

- **`"justMyCode": false` in `launch.json`** — too broad. Disables JMC stepping,
  pulls in framework symbols on first launch, and changes too much else.
- **Try/catch around `ValidateAndThrowAsync` per handler** — defeats the
  centralized middleware and adds boilerplate.
- **`[DebuggerHidden]` / `[DebuggerNonUserCode]`** — can't be applied to
  FluentValidation source.

## Verifying it works

1. Apply the ignore list (steps in TL;DR above).
2. Hit `F5`.
3. From a REST client, `POST https://localhost:44369/api/v1/Addresses` with an empty body `{ }`.
4. Expected: response is `400 Bad Request` with the validation errors. VS
   Code does **not** pause inside `CreateAddressCommandHandler.Handle`.

If VS Code still breaks, the most common cause is editing the condition before
the debugger had registered any exception filters. Stop the debug session,
start one, then re-edit the condition.

## Related files

- [`.vscode/launch.json`](../../../../.vscode/launch.json) — the `coreclr` launch config (no exception fields, by design)
- [`ExceptionHandlerMiddleware.cs`](../../src/AdventureWorks.API/libs/Middleware/ExceptionHandlerMiddleware.cs) — what the ignored exceptions are translated into
- [`apps/api-dotnet/.claude/CLAUDE.md`](../CLAUDE.md) — Exception and Contract Policy
