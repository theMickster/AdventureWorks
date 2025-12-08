import { Locator, Page } from '@playwright/test';

/**
 * Wraps `LoginFailedComponent` (`apps/adventureworks-web/src/app/login-failed/login-failed.ts`).
 * Selectors already exist on the component; this is a read-only Page Object wrapper.
 */
export class LoginFailedPage {
  readonly root: Locator;
  readonly retryButton: Locator;

  constructor(private readonly page: Page) {
    this.root = page.locator('#aw-login-failed');
    this.retryButton = page.locator('#aw-login-failed-retry');
  }

  async goto(): Promise<void> {
    await this.page.goto('/login-failed');
  }
}
