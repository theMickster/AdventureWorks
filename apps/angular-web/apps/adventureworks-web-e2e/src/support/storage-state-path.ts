import { workspaceRoot } from '@nx/devkit';
import { join } from 'node:path';

/**
 * Path to the persisted authenticated storage state (cookies + localStorage) produced by the
 * `setup` Playwright project (see `global-setup.ts` and `playwright.config.ts`). Anchored on
 * `workspaceRoot` rather than `__dirname` — the Nx Playwright plugin evaluates this module
 * in a context where `__dirname` is not defined.
 */
export const STORAGE_STATE_PATH = join(
  workspaceRoot,
  'apps/adventureworks-web-e2e/playwright/.auth/user.json',
);
