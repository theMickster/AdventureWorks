import { expect, test } from '@playwright/test';
import { AppLayoutPage } from './page-objects/app-layout.page';

/**
 * Baselines are committed under `test-snapshots/` (Playwright's default `toMatchSnapshot`
 * directory convention, configured via `snapshotDir`/`testDir`). First run requires
 * `--update-snapshots` — see the e2e README.
 */
test.describe('Visual regression smoke tests (US-684)', () => {
  test('/dashboard renders consistently in light and dark theme', async ({ page }) => {
    const layout = new AppLayoutPage(page);
    await page.goto('/dashboard');

    // Force a known starting theme so light/dark screenshots are deterministic.
    await page.evaluate(() => localStorage.setItem('aw-theme', 'alpine-circuit'));
    await page.reload();
    await expect(layout.shell).toBeVisible();

    await expect(page).toHaveScreenshot('dashboard-light.png');

    await layout.toggleTheme();
    await expect(page).toHaveScreenshot('dashboard-dark.png');
  });
});
