# AdventureWorks Cycling - Angular Web Application

Angular 21 frontend for AdventureWorks Cycling, built with Nx 22.x monorepo architecture.

## Quick Start

```bash
cd apps/angular-web
npm install
npx nx serve adventureworks-web
# Open http://localhost:4200
```

## Architecture

### Workspace Structure

```
apps/
  adventureworks-web/          # Main Angular application
  adventureworks-web-e2e/      # Playwright E2E tests
libs/
  shared/
    ui/                        # Shared UI components (type:ui, scope:shared)
    util/                      # Shared utilities & theme config (type:util, scope:shared)
```

### Library Dependency Rules

Libraries are tagged by **type** and **scope**. Dependencies are enforced by ESLint module boundaries:

```
feature      -> feature, ui, data-access, util
ui           -> ui, util
data-access  -> data-access, util
util         -> util only
```

Domain scopes: `shared`, `sales`, `hr`. A domain library can only depend on its own scope or `shared`.

### Import Paths

All libraries use the `@adventureworks-web/` prefix:

```typescript
import { SomeComponent } from '@adventureworks-web/shared/ui';
import { someUtil } from '@adventureworks-web/shared/util';
```

## Technology Stack

| Technology | Version | Purpose                                 |
| ---------- | ------- | --------------------------------------- |
| Angular    | 21.1.1  | UI framework (zoneless, Signals-based)  |
| Nx         | 22.5.1  | Monorepo tooling, caching, generators   |
| TypeScript | 5.9.2   | Language (strict mode, all 6 flags)     |
| esbuild    | -       | Build bundler (via @angular/build)      |
| Vitest     | -       | Unit testing (@angular/build:unit-test) |
| Playwright | -       | E2E testing                             |
| SCSS       | -       | Stylesheets                             |

## Design System

**Alpine Circuit v2** color system with light/dark theme support via CSS custom properties.

| Color             | Light     | Dark      | Use                 |
| ----------------- | --------- | --------- | ------------------- |
| Primary (Cyan)    | `#0891b2` | `#22d3ee` | Headers, links, nav |
| Secondary (Slate) | `#64748b` | `#94a3b8` | Body text, borders  |
| Accent (Teal)     | `#14b8a6` | `#2dd4bf` | Success, highlights |
| Pop (Crimson)     | `#dc2626` | `#f87171` | CTAs, sale badges   |

- Theme CSS: `apps/adventureworks-web/src/styles.scss`
- Tailwind config: `libs/shared/util/src/lib/theme/alpine-circuit-tailwind.ts` _(not yet installed — Feature #572)_

## Commands

```bash
# Development
npx nx serve adventureworks-web        # Dev server on :4200
npx nx build adventureworks-web        # Production build

# Testing
npx nx test adventureworks-web         # Unit tests (Vitest)
npx nx test adventureworks-web --coverage
npx nx e2e adventureworks-web-e2e      # E2E tests (Playwright)

# Linting
npx nx lint adventureworks-web

# Workspace-wide
npx nx affected -t build              # Build changed projects only
npx nx affected -t test               # Test changed projects only
npx nx graph                          # Visual dependency graph

# Generate new library
npx nx g @nx/angular:library \
  --directory=libs/{scope}/{type}-{name} \
  --tags="type:{type},scope:{scope}" \
  --importPath=@adventureworks-web/{scope}/{type}-{name} \
  --standalone=true --prefix=aw --no-interactive
```

## Build Optimization

Production builds (`--configuration=production`) include:

- AOT compilation (inherent with esbuild)
- No source maps
- Output hashing
- Bundle budgets: 500kB warn / 1MB error (initial), 4kB warn / 8kB error (component styles)
