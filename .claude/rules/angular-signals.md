---
paths:
  - "apps/angular-web/**/*.ts"
---

# Angular 21 Signals & Zoneless Patterns

This workspace is **zoneless** (no Zone.js). Follow modern Angular signal APIs.

## Dependency Injection

- Use `inject()` function, not constructor injection
- Exception: constructors are fine when wrapping third-party libs that require them

## Signal-Based State

- Use `signal()` for local mutable state
- Use `computed()` for derived state (replaces most `ngOnChanges` logic)
- Use `effect()` only for side effects (logging, localStorage sync)
- Convert observables at boundaries: `toSignal(observable$)` from `@angular/core/rxjs-interop`

## Signal Component APIs

Prefer signal-based component APIs over decorators:

| Instead of | Use |
|---|---|
| `@Input()` | `input()` / `input.required()` |
| `@Output()` | `output()` |
| Two-way binding decorator | `model()` |
| `@ViewChild()` | `viewChild()` / `viewChildren()` |

Decorators are not banned — they're allowed when interop demands it — but signal APIs are the default.

## Change Detection

- Prefer `ChangeDetectionStrategy.OnPush` on all components
- Never import `zone.js` or `zone.js/testing`
- Never use `NgZone`, `ChangeDetectorRef.detectChanges()`, or `ApplicationRef.tick()`
