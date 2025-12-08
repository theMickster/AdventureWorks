---
paths:
  - "apps/angular-web/**/*.css"
  - "apps/angular-web/**/*.html"
  - "apps/angular-web/**/*.ts"
---

# Alpine Circuit v2 Theming Rules

## Class Preference Order

When styling, use this priority:

1. **DaisyUI component class** — `btn btn-primary`, `card`, `badge badge-accent`, `alert alert-info`
2. **Tailwind `ac-*` utility** — `bg-ac-primary`, `text-ac-accent-400`, `border-ac-secondary-200`
3. **DaisyUI semantic utility** — `bg-base-200`, `text-base-content`, `text-primary`
4. **CSS custom property** — `var(--ac-primary)` in component CSS (legacy — avoid in new code)

## No Raw Hex Values

Never hardcode hex colors in component styles or templates. Use DaisyUI theme classes or Tailwind `ac-*` utilities.

## Muted/Secondary Text: Use `text-secondary`, Never `text-base-content/NN`

For de-emphasized text (captions, subtitles, empty-state descriptions, field hints), use the
plain `text-secondary` utility — it's a solid, WCAG AA-safe color in both themes.

**Never use Tailwind's opacity modifier on `text-base-content`** (e.g. `text-base-content/50`,
`/60`, `/70`). It compiles to `color-mix(in oklab, var(--color-base-content) N%, transparent)` —
true runtime alpha compositing against whatever background sits behind the element, not a
precomputed color. The effective contrast then depends on theme, ancestor background, and the
chosen percentage all at once, none of which get checked by writing the class — this is exactly
how several WCAG AA color-contrast violations shipped (axe-core caught them; see `git log` for
the fix). If you need a different "weight" of muted text than `secondary` provides, pick another
solid semantic color and verify its contrast ratio against every background it can appear on —
don't reach for an opacity modifier.

This is mechanically enforced, not just convention: the `@nx/workspace-no-text-color-opacity`
ESLint rule (`apps/angular-web/tools/eslint-rules/`) blocks any `text-*/NN` opacity modifier in
`.html` templates and inline `@Component({ template: ... })` strings, and `nx lint` is a hard,
blocking CI gate.

## Available DaisyUI Semantic Colors

These map to Alpine Circuit colors and switch automatically with light/dark theme:

| DaisyUI Class      | Light Value            | Use for                           |
| ------------------ | ---------------------- | --------------------------------- |
| `primary`          | `#0891b2`              | Headers, links, nav active states |
| `secondary`        | `#64748b`              | Body text, borders, muted UI      |
| `accent`           | `#14b8a6`              | Highlights, badges                |
| `error`            | `#dc2626`              | CTAs, destructive actions, errors |
| `success`          | `#059669`              | Success states                    |
| `warning`          | `#d97706`              | Warning indicators                |
| `info`             | `#0891b2`              | Info alerts                       |
| `base-100/200/300` | `#fff/#f8fafc/#f0fdfa` | Surfaces, backgrounds             |
| `base-content`     | `#164e63`              | Primary body text                 |

## Alpine Circuit Tailwind Utilities

Full 50-900 color scales for brand colors (defined via `@theme` in `styles.css`):

- `bg-ac-primary-{50-900}`, `text-ac-primary-{50-900}`
- `bg-ac-secondary-{50-900}`, `bg-ac-accent-{50-900}`, `bg-ac-pop-{50-900}`
- Shorthand defaults: `bg-ac-primary` = `#0891b2`, `bg-ac-pop` = `#dc2626`

## Legacy CSS Custom Properties

Still available in `styles.css` for backward compat (`:root` / `[data-theme="dark"]`):

- `--ac-primary`, `--ac-secondary`, `--ac-accent`, `--ac-pop`
- `--ac-background`, `--ac-surface`, `--ac-border`, `--ac-text`, `--ac-text-muted`
- `.ac-btn-primary`, `.ac-card`, `.ac-text-gradient` — prefer DaisyUI equivalents

## Dark Mode

- **DaisyUI**: Handled by `data-theme` attribute on `<html>` — `alpine-circuit` (light) or `alpine-circuit-dark` (dark)
- **CSS vars**: Also respond to `[data-theme="dark"]` and `.dark` class for backward compat
- No per-component dark mode overrides needed

## Critical: No Sass

Tailwind v4 is incompatible with Sass. Global styles must be `.css`, not `.scss`. The Angular builder's inline style language is set to `css`.
