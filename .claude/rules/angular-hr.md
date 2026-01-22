---
paths:
  - "apps/angular-web/libs/hr/**"
---

# HR Library Rules

Rules the specs and code comments in `libs/hr/**` do not already enforce. Cross-domain list, store, and testing rules are in `apps/angular-web/.claude/CLAUDE.md`.

- Derive department group-name options only through `extractGroupNames()` in `feature-departments`. Never inline a local dedupe and sort.
- In `EmployeeCreateComponent.onSubmit`, keep `submitted.set(true)` before `markAllAsTouched()`. That signal write is what makes the `*Errors` computeds recompute; a bare `markAsTouched()` is not tracked.
- Keep `EmployeeDetailComponent`'s `isLoading` and `isSaving` separate. A save must not blank the page.
- After a Personal Info save, set `employee()` from the PUT response. Never re-fetch with `getEmployee`.
- Never export the Hire, Terminate, or Rehire modals from `feature-employees`' `index.ts`. `EmployeeDetailComponent` is their only consumer, and their completion detection relies on being the sole active consumer of `EmployeeStore`'s request-status signals.
- Never wrap `OrgNodeComponent`'s children container in `@if (isExpanded())`. Collapse runs through the `grid-template-rows` transition in `org-node.css`, and the post-expand `scrollIntoView` timing needs the child nodes mounted.
