/**
 * Alpine Circuit v2 - Tailwind CSS Color Configuration
 * Source: docs/alpine-circuit-colors.ts
 */

export const alpineCircuitTailwind = {
  theme: {
    extend: {
      colors: {
        // Core brand
        'ac-primary': {
          50: '#ecfeff',
          100: '#cffafe',
          200: '#a5f3fc',
          300: '#67e8f9',
          400: '#22d3ee',
          500: '#06b6d4',
          600: '#0891b2',
          700: '#0e7490',
          800: '#155e75',
          900: '#164e63',
          DEFAULT: '#0891b2',
        },
        'ac-secondary': {
          50: '#f8fafc',
          100: '#f1f5f9',
          200: '#e2e8f0',
          300: '#cbd5e1',
          400: '#94a3b8',
          500: '#64748b',
          600: '#475569',
          700: '#334155',
          800: '#1e293b',
          900: '#0f172a',
          DEFAULT: '#64748b',
        },
        'ac-accent': {
          50: '#f0fdfa',
          100: '#ccfbf1',
          200: '#99f6e4',
          300: '#5eead4',
          400: '#2dd4bf',
          500: '#14b8a6',
          600: '#0d9488',
          700: '#0f766e',
          800: '#115e59',
          900: '#134e4a',
          DEFAULT: '#14b8a6',
        },
        // Pop color for CTAs and highlights
        'ac-pop': {
          50: '#fef2f2',
          100: '#fee2e2',
          200: '#fecaca',
          300: '#fca5a5',
          400: '#f87171',
          500: '#ef4444',
          600: '#dc2626',
          700: '#b91c1c',
          800: '#991b1b',
          900: '#7f1d1d',
          DEFAULT: '#dc2626',
        },
        // Semantic shortcuts
        'ac-bg': 'var(--ac-background)',
        'ac-surface': 'var(--ac-surface)',
        'ac-border': 'var(--ac-border)',
        'ac-text': 'var(--ac-text)',
        'ac-muted': 'var(--ac-text-muted)',
      },
    },
  },
};
