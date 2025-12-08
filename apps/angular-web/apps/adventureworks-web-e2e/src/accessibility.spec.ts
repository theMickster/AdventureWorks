import AxeBuilder from '@axe-core/playwright';
import { expect, test } from '@playwright/test';

test.describe('Accessibility smoke tests (US-684)', () => {
  test('authenticated /dashboard has zero WCAG AA violations', async ({ page }) => {
    await page.goto('/dashboard');
    await expect(page.locator('#aw-shell')).toBeVisible();

    const results = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa']).analyze();

    expect(results.violations).toEqual([]);
  });

  test('unauthenticated /login-failed has zero WCAG AA violations', async ({ page }) => {
    await page.goto('/login-failed');
    await expect(page.locator('#aw-login-failed')).toBeVisible();

    const results = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa']).analyze();

    expect(results.violations).toEqual([]);
  });

  test('authenticated /sales/orders has zero WCAG AA violations', async ({ page }) => {
    await page.goto('/sales/orders');
    await expect(page.locator('#aw-order-list-page')).toBeVisible();

    const results = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa']).analyze();

    expect(results.violations).toEqual([]);
  });
});
