---
paths:
  - "apps/angular-web/**/*"
---

# Nx Library Guardrails

Generator commands are documented in `.claude/CLAUDE.md`. This file covers **guardrails only**.

## Valid Tag Values

Tags are enforced by `eslint.config.mjs`. Use only these exact values:

- **type**: `type:feature`, `type:ui`, `type:data-access`, `type:util`
- **scope**: `scope:shared`, `scope:sales`, `scope:hr`, `scope:public`

Never invent new tags without updating the ESLint `depConstraints` array.

`scope:public` is one-directional: public libraries may depend on `scope:shared`, but no `scope:sales`/`scope:hr`/`scope:shared` library may depend back on `scope:public` — it sits at the top of the public-facing dependency graph, isolated from authenticated feature code.

## Import Rules

- Cross-library imports MUST use the `@adventureworks-web/` path alias (defined in `tsconfig.base.json`)
- Never use relative paths (`../../../libs/...`) across library boundaries
- Each library's `index.ts` is its public API — only export what consumers need

## CLI

- Always use `npx nx` — never `ng` directly
- All commands run from `apps/angular-web/` (the Nx workspace root)

## Post-Generation Checklist

After running any `nx g` command, verify:

1. `tsconfig.base.json` has the new path alias
2. `project.json` has correct `tags` array
3. `index.ts` barrel file exports the intended public API
