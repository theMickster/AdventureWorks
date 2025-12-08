import { Locator, Page } from '@playwright/test';

/**
 * Wraps the authenticated app shell (`AppLayoutComponent`) — sidebar, navbar,
 * breadcrumbs, theme toggle, user menu, and footer. All selectors already exist
 * in `libs/shared/app-layout/src/lib/app-layout/app-layout.html`; this is a
 * read-only Page Object wrapper, not UI work.
 */
export class AppLayoutPage {
  readonly shell: Locator;
  readonly sidebar: Locator;
  readonly navbar: Locator;
  readonly breadcrumbs: Locator;
  readonly themeToggle: Locator;
  readonly userMenu: Locator;
  readonly userMenuTrigger: Locator;
  readonly userMenuSignOut: Locator;
  readonly footer: Locator;

  readonly navDashboard: Locator;
  readonly navSales: Locator;
  readonly navSalesStores: Locator;
  readonly navSalesPersons: Locator;
  readonly navSalesOrders: Locator;
  readonly navSalesCustomers: Locator;
  readonly navHr: Locator;
  readonly navHrEmployees: Locator;
  readonly navHrDepartments: Locator;
  readonly navSamples: Locator;

  constructor(private readonly page: Page) {
    this.shell = page.locator('#aw-shell');
    this.sidebar = page.locator('#aw-sidebar');
    this.navbar = page.locator('#aw-navbar');
    this.breadcrumbs = page.locator('#aw-breadcrumbs');
    this.themeToggle = page.locator('#aw-theme-toggle');
    this.userMenu = page.locator('#aw-user-menu');
    this.userMenuTrigger = page.locator('#aw-user-menu-trigger');
    this.userMenuSignOut = page.locator('#aw-user-menu-signout');
    this.footer = page.locator('#aw-footer');

    this.navDashboard = page.locator('#aw-nav-dashboard');
    this.navSales = page.locator('#aw-nav-sales');
    this.navSalesStores = page.locator('#aw-nav-sales-stores');
    this.navSalesPersons = page.locator('#aw-nav-sales-persons');
    this.navSalesOrders = page.locator('#aw-nav-sales-orders');
    this.navSalesCustomers = page.locator('#aw-nav-sales-customers');
    this.navHr = page.locator('#aw-nav-hr');
    this.navHrEmployees = page.locator('#aw-nav-hr-employees');
    this.navHrDepartments = page.locator('#aw-nav-hr-departments');
    this.navSamples = page.locator('#aw-nav-samples');
  }

  /** Reads the rendered breadcrumb trail as plain text segments, e.g. ["Home", "Sales", "Stores"]. */
  async breadcrumbText(): Promise<string[]> {
    const items = await this.breadcrumbs.locator('li').allTextContents();
    return items.map((text) => text.trim());
  }

  async toggleTheme(): Promise<void> {
    await this.themeToggle.click();
  }

  async currentTheme(): Promise<string | null> {
    return this.page.locator('html').getAttribute('data-theme');
  }

  async openSalesSection(): Promise<void> {
    await this.navSales.locator('summary').click();
  }

  async openHrSection(): Promise<void> {
    await this.navHr.locator('summary').click();
  }
}
