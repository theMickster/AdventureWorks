/**
 * ALPINE CIRCUIT v2 - Color System
 * Theme: Racing lines + glacial cool + CRIMSON POP
 * Mood: Athletic, premium, crisp, fast, passionate
 * 
 * The crimson accent provides:
 * - Call-to-action buttons
 * - Sale/promo highlights  
 * - Error states (semantic alignment)
 * - Passion/energy moments in the UI
 */

// ═══════════════════════════════════════════════════════════════════
// BASIC COLOR LIST
// ═══════════════════════════════════════════════════════════════════

export const alpineCircuitColors = {
  // === LIGHT THEME ===
  light: {
    // Primary - Alpine Deep Cyan (depth, mountain, pro)
    primary: {
      50:  '#ecfeff',
      100: '#cffafe',
      200: '#a5f3fc',
      300: '#67e8f9',
      400: '#22d3ee',
      500: '#06b6d4',
      600: '#0891b2',  // Main
      700: '#0e7490',
      800: '#155e75',
      900: '#164e63',  // Logo dark
    },
    
    // Secondary - Slate Silver (metallic, premium)
    secondary: {
      50:  '#f8fafc',
      100: '#f1f5f9',
      200: '#e2e8f0',
      300: '#cbd5e1',
      400: '#94a3b8',
      500: '#64748b',  // Main
      600: '#475569',
      700: '#334155',
      800: '#1e293b',
      900: '#0f172a',
    },
    
    // Accent - Glacial Teal (energy, speed, highlight)
    accent: {
      50:  '#f0fdfa',
      100: '#ccfbf1',
      200: '#99f6e4',
      300: '#5eead4',
      400: '#2dd4bf',
      500: '#14b8a6',  // Main
      600: '#0d9488',
      700: '#0f766e',
      800: '#115e59',
      900: '#134e4a',
    },
    
    // ★ NEW: Pop - Racing Crimson (CTA, passion, energy)
    pop: {
      50:  '#fef2f2',
      100: '#fee2e2',
      200: '#fecaca',
      300: '#fca5a5',
      400: '#f87171',
      500: '#ef4444',
      600: '#dc2626',  // Main - vibrant but not harsh
      700: '#b91c1c',  // Deep crimson
      800: '#991b1b',
      900: '#7f1d1d',
    },
    
    // Semantic
    background: '#ffffff',
    surface: '#f8fafc',
    surfaceAlt: '#f0fdfa',
    border: '#e2e8f0',
    text: '#164e63',
    textMuted: '#64748b',
    
    // Semantic States (pop color integrates naturally)
    success: '#059669',
    warning: '#d97706',
    error: '#dc2626',  // Matches pop-600
    info: '#0891b2',   // Matches primary-600
  },

  // === DARK THEME ===
  dark: {
    // Primary - Cyan (brightened for dark)
    primary: {
      50:  '#164e63',
      100: '#155e75',
      200: '#0e7490',
      300: '#0891b2',
      400: '#06b6d4',
      500: '#22d3ee',  // Main (brighter)
      600: '#67e8f9',
      700: '#a5f3fc',
      800: '#cffafe',
      900: '#ecfeff',
    },
    
    // Secondary - Slate (inverted)
    secondary: {
      50:  '#0f172a',
      100: '#1e293b',
      200: '#334155',
      300: '#475569',
      400: '#64748b',
      500: '#94a3b8',  // Main
      600: '#cbd5e1',
      700: '#e2e8f0',
      800: '#f1f5f9',
      900: '#f8fafc',
    },
    
    // Accent - Teal (brightened)
    accent: {
      50:  '#134e4a',
      100: '#115e59',
      200: '#0f766e',
      300: '#0d9488',
      400: '#14b8a6',
      500: '#2dd4bf',  // Main (brighter)
      600: '#5eead4',
      700: '#99f6e4',
      800: '#ccfbf1',
      900: '#f0fdfa',
    },
    
    // ★ Pop - Crimson (brightened for dark mode visibility)
    pop: {
      50:  '#7f1d1d',
      100: '#991b1b',
      200: '#b91c1c',
      300: '#dc2626',
      400: '#ef4444',
      500: '#f87171',  // Main (brighter for dark)
      600: '#fca5a5',
      700: '#fecaca',
      800: '#fee2e2',
      900: '#fef2f2',
    },
    
    // Semantic
    background: '#0c1821',
    surface: '#132634',
    surfaceAlt: '#1a3344',
    border: '#334155',
    text: '#e0f2fe',
    textMuted: '#94a3b8',
    
    // Semantic States
    success: '#10b981',
    warning: '#f59e0b',
    error: '#f87171',  // Matches pop-500 dark
    info: '#22d3ee',   // Matches primary-500 dark
  }
};

// ═══════════════════════════════════════════════════════════════════
// TAILWIND CSS CONFIGURATION
// ═══════════════════════════════════════════════════════════════════

export const alpineCircuitTailwind = {
  theme: {
    extend: {
      colors: {
        // Core brand
        'ac-primary': {
          50:  '#ecfeff',
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
          50:  '#f8fafc',
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
          50:  '#f0fdfa',
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
        // ★ NEW: Pop color for CTAs and highlights
        'ac-pop': {
          50:  '#fef2f2',
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

// ═══════════════════════════════════════════════════════════════════
// CSS CUSTOM PROPERTIES (for Angular/theme switching)
// ═══════════════════════════════════════════════════════════════════

export const alpineCircuitCSSVars = `
/* Alpine Circuit v2 - CSS Custom Properties */

:root {
  /* Surfaces */
  --ac-background: #ffffff;
  --ac-surface: #f8fafc;
  --ac-surface-alt: #f0fdfa;
  --ac-border: #e2e8f0;
  
  /* Text */
  --ac-text: #164e63;
  --ac-text-muted: #64748b;
  
  /* Brand Colors (light mode values) */
  --ac-primary: #0891b2;
  --ac-primary-hover: #0e7490;
  --ac-secondary: #64748b;
  --ac-accent: #14b8a6;
  --ac-pop: #dc2626;
  --ac-pop-hover: #b91c1c;
  
  /* Semantic */
  --ac-success: #059669;
  --ac-warning: #d97706;
  --ac-error: #dc2626;
  --ac-info: #0891b2;
}

[data-theme="dark"],
.dark {
  /* Surfaces */
  --ac-background: #0c1821;
  --ac-surface: #132634;
  --ac-surface-alt: #1a3344;
  --ac-border: #334155;
  
  /* Text */
  --ac-text: #e0f2fe;
  --ac-text-muted: #94a3b8;
  
  /* Brand Colors (dark mode - brightened) */
  --ac-primary: #22d3ee;
  --ac-primary-hover: #67e8f9;
  --ac-secondary: #94a3b8;
  --ac-accent: #2dd4bf;
  --ac-pop: #f87171;
  --ac-pop-hover: #fca5a5;
  
  /* Semantic */
  --ac-success: #10b981;
  --ac-warning: #f59e0b;
  --ac-error: #f87171;
  --ac-info: #22d3ee;
}

/* Utility classes for common patterns */
.ac-btn-primary {
  background-color: var(--ac-primary);
  color: white;
}
.ac-btn-primary:hover {
  background-color: var(--ac-primary-hover);
}

.ac-btn-pop {
  background-color: var(--ac-pop);
  color: white;
}
.ac-btn-pop:hover {
  background-color: var(--ac-pop-hover);
}

.ac-card {
  background-color: var(--ac-surface);
  border: 1px solid var(--ac-border);
}

.ac-text-gradient {
  background: linear-gradient(135deg, var(--ac-primary), var(--ac-accent));
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}
`;

// ═══════════════════════════════════════════════════════════════════
// USAGE EXAMPLES
// ═══════════════════════════════════════════════════════════════════

/*
RECOMMENDED COLOR USAGE:

1. PRIMARY (Cyan) - Main brand, headers, links, nav active states
   - "Shop Now" links
   - Selected navigation items
   - Section headers

2. SECONDARY (Slate) - Supporting text, borders, disabled states
   - Body text muted
   - Input borders
   - Dividers

3. ACCENT (Teal) - Success states, positive actions, highlights
   - "In Stock" badges
   - Completed steps
   - Feature highlights

4. POP (Crimson) - CTAs, sales, urgency, passion moments
   - "Add to Cart" buttons
   - "Sale" badges
   - Price highlights
   - Limited time offers
   - Racing/competition themes

The cyan + crimson combo creates visual tension that draws the eye 
to important actions while maintaining the cool, athletic brand feel.
*/