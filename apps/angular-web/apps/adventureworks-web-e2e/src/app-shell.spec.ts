import { expect, test } from '@playwright/test';
import { AppLayoutPage } from './page-objects/app-layout.page';
import { PublicLandingPage } from './page-objects/public-landing.page';

/**
 * Runs under the authenticated Playwright projects (chromium/firefox/webkit — see
 * playwright.config.ts). The unauthenticated-root case lives in auth-boundary.spec.ts,
 * which runs under the unauthenticated project instead.
 */
test.describe('App shell smoke tests (US-682)', () => {
  test('authenticated direct navigation to /dashboard renders the app shell', async ({ page }) => {
    const layout = new AppLayoutPage(page);
    await page.goto('/dashboard');

    await expect(page).toHaveURL(/\/dashboard$/);
    await expect(layout.shell).toBeVisible();
    await expect(layout.sidebar).toBeVisible();
    await expect(layout.navbar).toBeVisible();
  });

  test('authenticated root still renders the public landing page (regression guard)', async ({ page }) => {
    const landing = new PublicLandingPage(page);
    await landing.goto();

    await expect(page).toHaveURL('/');
    await expect(landing.dashboardButton).toBeVisible();

    await landing.dashboardButton.click();
    await expect(page).toHaveURL(/\/dashboard$/);
  });

  test('sidebar navigation to Sales > Stores updates the URL and breadcrumbs', async ({ page }) => {
    const layout = new AppLayoutPage(page);
    await page.goto('/dashboard');

    await layout.openSalesSection();
    await layout.navSalesStores.click();

    await expect(page).toHaveURL(/\/sales\/stores$/);
    await expect.poll(() => layout.breadcrumbText()).toEqual(['Home', 'Sales', 'Stores']);
  });

  test('theme toggle flips data-theme and swaps the sun/moon icon', async ({ page }) => {
    const layout = new AppLayoutPage(page);
    await page.goto('/dashboard');

    // Force a known starting theme so the toggle's before/after states are deterministic.
    await page.evaluate(() => localStorage.setItem('aw-theme', 'alpine-circuit'));
    await page.reload();
    await expect(layout.shell).toBeVisible();
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'alpine-circuit');
    await expect(layout.themeToggle.locator('i')).toHaveClass(/fa-moon/);

    await layout.toggleTheme();

    await expect(page.locator('html')).toHaveAttribute('data-theme', 'alpine-circuit-dark');
    await expect(layout.themeToggle.locator('i')).toHaveClass(/fa-sun/);
  });
});
