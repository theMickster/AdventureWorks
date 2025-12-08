import { Locator, Page } from '@playwright/test';

/**
 * Wraps the public nav (`libs/public/feature-landing/src/lib/public-nav/public-nav.html`)
 * rendered by the unauthenticated (and always-reachable) landing route at `/`. Selectors
 * already exist on the component; this is a read-only Page Object wrapper.
 */
export class PublicLandingPage {
  readonly loginButton: Locator;
  readonly dashboardButton: Locator;
  readonly logoutButton: Locator;

  constructor(private readonly page: Page) {
    this.loginButton = page.locator('#aw-public-login-btn');
    this.dashboardButton = page.locator('#aw-public-dashboard-btn');
    this.logoutButton = page.locator('#aw-public-logout-btn');
  }

  async goto(): Promise<void> {
    await this.page.goto('/');
  }
}
