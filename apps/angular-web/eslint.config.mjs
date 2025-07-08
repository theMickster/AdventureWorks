import nx from '@nx/eslint-plugin';
import angular from 'angular-eslint';
import prettierConfig from 'eslint-config-prettier';

export default [
  ...nx.configs['flat/base'],
  ...nx.configs['flat/typescript'],
  ...nx.configs['flat/javascript'],
  {
    ignores: ['**/dist', '**/vite.config.*.timestamp*', '**/vitest.config.*.timestamp*'],
  },
  {
    files: ['**/*.ts', '**/*.tsx', '**/*.js', '**/*.jsx'],
    rules: {
      '@nx/enforce-module-boundaries': [
        'error',
        {
          enforceBuildableLibDependency: true,
          allow: ['^.*/eslint(\\.base)?\\.config\\.[cm]?[jt]s$'],
          depConstraints: [
            // Type constraints
            {
              sourceTag: 'type:feature',
              onlyDependOnLibsWithTags: ['type:feature', 'type:ui', 'type:data-access', 'type:util'],
            },
            { sourceTag: 'type:ui', onlyDependOnLibsWithTags: ['type:ui', 'type:util'] },
            { sourceTag: 'type:data-access', onlyDependOnLibsWithTags: ['type:data-access', 'type:util'] },
            { sourceTag: 'type:util', onlyDependOnLibsWithTags: ['type:util'] },
            // Domain scope constraints
            { sourceTag: 'scope:shared', onlyDependOnLibsWithTags: ['scope:shared'] },
            { sourceTag: 'scope:sales', onlyDependOnLibsWithTags: ['scope:sales', 'scope:shared'] },
            { sourceTag: 'scope:hr', onlyDependOnLibsWithTags: ['scope:hr', 'scope:shared'] },
            { sourceTag: 'scope:app', onlyDependOnLibsWithTags: ['scope:shared', 'scope:sales', 'scope:hr'] },
          ],
        },
      ],
    },
  },
  {
    files: ['**/*.ts'],
    ...angular.configs.tsRecommended[0],
    rules: {
      ...angular.configs.tsRecommended[0].rules,
      '@angular-eslint/component-class-suffix': 'error',
      '@angular-eslint/directive-class-suffix': 'error',
      '@angular-eslint/component-selector': ['error', { type: 'element', prefix: 'aw', style: 'kebab-case' }],
      '@angular-eslint/directive-selector': ['error', { type: 'attribute', prefix: 'aw', style: 'camelCase' }],
      '@angular-eslint/prefer-standalone': 'error',
      '@angular-eslint/prefer-on-push-component-change-detection': 'error',
      '@typescript-eslint/no-explicit-any': 'error',
    },
  },
  {
    files: ['**/*.html'],
    ...angular.configs.templateRecommended[0],
  },
  {
    files: ['**/*.html'],
    ...angular.configs.templateAccessibility[0],
  },
  prettierConfig,
];
