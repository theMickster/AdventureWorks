---
paths:
  - "apps/angular-web/**/*.scss"
  - "apps/angular-web/**/*.html"
  - "apps/angular-web/**/*.ts"
---

# Alpine Circuit v2 Theming Rules

## No Raw Hex Values

Never hardcode hex colors in component styles or templates. Always use `var(--ac-*)` CSS custom properties.

## Semantic Color Map

Use the right variable for the right purpose:

| Variable                                                  | Use for                                |
| --------------------------------------------------------- | -------------------------------------- |
| `--ac-primary`                                            | Headers, links, nav active states      |
| `--ac-secondary`                                          | Body text, borders, muted UI           |
| `--ac-accent`                                             | Success states, highlights, badges     |
| `--ac-pop`                                                | CTAs, sale badges, destructive actions |
| `--ac-background`                                         | Page background                        |
| `--ac-surface`                                            | Card/panel backgrounds                 |
| `--ac-surface-alt`                                        | Alternate surface (teal tint)          |
| `--ac-border`                                             | Borders, dividers                      |
| `--ac-text`                                               | Primary body text                      |
| `--ac-text-muted`                                         | Secondary/helper text                  |
| `--ac-success`, `--ac-warning`, `--ac-error`, `--ac-info` | Status indicators                      |

## Utility Classes

Only these 4 utility classes are defined in `styles.scss`:

- `.ac-btn-primary` — primary button with hover
- `.ac-btn-pop` — CTA/pop button with hover
- `.ac-card` — surface background + border
- `.ac-text-gradient` — primary-to-accent gradient text

## Dark Mode

Theme switching uses `[data-theme="dark"]` attribute (not `@media (prefers-color-scheme)`). All `--ac-*` vars are re-mapped automatically — no per-component dark mode overrides needed.

## Tailwind (Feature 572)

When Tailwind is integrated, use `ac-*` Tailwind classes in templates (e.g., `bg-ac-primary`, `text-ac-muted`). Config is at `libs/shared/util/src/lib/theme/alpine-circuit-tailwind.ts`. CSS vars remain the source of truth in SCSS files.
