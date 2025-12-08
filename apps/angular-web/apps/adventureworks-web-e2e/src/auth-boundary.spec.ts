import { expect, test } from '@playwright/test';
import { LoginFailedPage } from './page-objects/login-failed.page';
import { PublicLandingPage } from './page-objects/public-landing.page';

/**
 * Runs under the `chromium-unauthenticated` Playwright project (no storageState) —
 * see playwright.config.ts. Never gains an authenticated session.
 */
test.describe('Unauthenticated boundary smoke tests (US-683)', () => {
  test('unauthenticated root renders the public landing page, no redirect', async ({ page }) => {
    const landing = new PublicLandingPage(page);
    await landing.goto();

    await expect(page).toHaveURL('/');
    await expect(landing.loginButton).toBeVisible();
    await expect(landing.dashboardButton).toHaveCount(0);
    await expect(landing.logoutButton).toHaveCount(0);
  });

  test('login-failed route renders the sign-in-failed message and retry action', async ({ page }) => {
    const loginFailed = new LoginFailedPage(page);
    await loginFailed.goto();

    await expect(loginFailed.root).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Sign In Failed' })).toBeVisible();
    await expect(loginFailed.retryButton).toBeVisible();
    await expect(loginFailed.retryButton).toHaveText(/Try Again/);
  });

  test('unauthenticated navigation to /dashboard is redirected away by MsalGuard', async ({ page }) => {
    await page.goto('/dashboard');

    await expect(page).not.toHaveURL(/\/dashboard$/, { timeout: 10_000 });
    await expect(page.locator('#aw-shell')).toHaveCount(0);
  });
});
