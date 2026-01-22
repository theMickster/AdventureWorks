---
paths:
  - "apps/angular-web/libs/manufacturing/**"
---

# Manufacturing Library Rules

Rules the specs and code comments in `libs/manufacturing/**` do not already enforce. Cross-domain list, store, and testing rules are in `apps/angular-web/.claude/CLAUDE.md`.

- Do not normalise the quality scorecard's scrap-breakdown percentages to 100%. The API denominator is all scrapped units, including scrap with no recorded reason, so the listed rows can legitimately total less.
- Inside an `aw-card` (`bg-base-200`), use `hover:bg-base-300` for row hover. The `bg-base-200` hover used elsewhere is invisible on that surface.
