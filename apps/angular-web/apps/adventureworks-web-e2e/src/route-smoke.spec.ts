import { expect, Page, Request, Response, test } from '@playwright/test';

type RouteExpectation = {
  path: string;
  title: string;
  root?: string;
  redirectedTo?: RegExp;
  authenticated?: boolean;
};

type NavigationMonitor = {
  finish: () => Promise<void>;
};

function isApiGet(request: Request): boolean {
  return request.method() === 'GET' && new URL(request.url()).pathname.startsWith('/api/');
}

function monitorNavigation(page: Page, label: string): NavigationMonitor {
  const pageErrors: string[] = [];
  const failedApiGets: string[] = [];
  const pendingApiGets = new Set<Request>();

  const onPageError = (error: Error): void => {
    pageErrors.push(error.message);
  };
  const onRequest = (request: Request): void => {
    if (isApiGet(request)) {
      pendingApiGets.add(request);
    }
  };
  const onResponse = (response: Response): void => {
    const request = response.request();
    if (!isApiGet(request)) {
      return;
    }

    if (response.status() >= 400) {
      failedApiGets.push(`${response.status()} ${request.url()}`);
    }
  };
  const onRequestFinished = (request: Request): void => {
    if (isApiGet(request)) {
      pendingApiGets.delete(request);
    }
  };
  const onRequestFailed = (request: Request): void => {
    if (!isApiGet(request)) {
      return;
    }

    pendingApiGets.delete(request);
  };

  page.on('pageerror', onPageError);
  page.on('request', onRequest);
  page.on('response', onResponse);
  page.on('requestfinished', onRequestFinished);
  page.on('requestfailed', onRequestFailed);

  return {
    finish: async (): Promise<void> => {
      await page.evaluate(
        () => new Promise<void>((resolve) => requestAnimationFrame(() => requestAnimationFrame(() => resolve()))),
      );
      await expect.poll(() => pendingApiGets.size, { message: `${label}: API GET requests did not finish` }).toBe(0);

      page.off('pageerror', onPageError);
      page.off('request', onRequest);
      page.off('response', onResponse);
      page.off('requestfinished', onRequestFinished);
      page.off('requestfailed', onRequestFailed);

      expect(pageErrors, `${label}: browser page errors`).toEqual([]);
      expect(failedApiGets, `${label}: failed API GET requests`).toEqual([]);
    },
  };
}

async function expectCurrentRoute(page: Page, route: RouteExpectation): Promise<void> {
  if (route.redirectedTo) {
    await expect(page).toHaveURL(route.redirectedTo);
  }
  await expect(page).toHaveTitle(`${route.title} | AdventureWorks`);
  if (route.authenticated !== false) {
    await expect(page.locator('#aw-shell')).toBeVisible();
  }
  if (route.root) {
    await expect(page.locator(route.root)).toBeVisible();
  }
}

async function visit(page: Page, route: RouteExpectation): Promise<void> {
  const monitor = monitorNavigation(page, route.path);
  await page.goto(route.path);
  await expectCurrentRoute(page, route);
  await monitor.finish();
}

async function followFirstResult(
  page: Page,
  label: string,
  trigger: string,
  expectedUrl: RegExp,
  route: Omit<RouteExpectation, 'path' | 'redirectedTo'>,
): Promise<void> {
  const monitor = monitorNavigation(page, label);
  const result = page.locator(trigger).first();
  await expect(result, `${label}: expected at least one result`).toBeVisible();
  await result.click();
  await expect(page).toHaveURL(expectedUrl);
  await expectCurrentRoute(page, { ...route, path: label });
  await monitor.finish();
}

const viewResult = 'a:text-is("View"), button:text-is("View")';

test.describe('Authenticated route and API GET smoke baseline', () => {
  test('public and shared pages', async ({ page }) => {
    const routes: RouteExpectation[] = [
      { path: '/', title: 'AdventureWorks Cycling', root: '#aw-landing', authenticated: false },
      { path: '/login-failed', title: 'Sign In Failed', root: '#aw-login-failed', authenticated: false },
      { path: '/dashboard', title: 'Dashboard' },
      { path: '/samples', title: 'Samples', root: '#aw-samples-page' },
      { path: '/route-that-does-not-exist', title: 'Page Not Found', root: '#aw-not-found' },
    ];

    for (const route of routes) {
      await test.step(route.path, () => visit(page, route));
    }
  });

  test('Sales routes', async ({ page }) => {
    await visit(page, {
      path: '/sales',
      title: 'Stores',
      root: '#aw-store-list-page',
      redirectedTo: /\/sales\/stores$/,
    });
    await visit(page, { path: '/sales/stores/new', title: 'New Store', root: '#aw-store-create-page' });
    await visit(page, { path: '/sales/stores', title: 'Stores', root: '#aw-store-list-page' });
    await followFirstResult(page, 'first store detail', viewResult, /\/sales\/stores\/\d+$/, {
      title: 'Store Detail',
      root: '#aw-store-detail-page',
    });
    await followFirstResult(page, 'store edit', '#aw-store-detail-edit-btn', /\/sales\/stores\/\d+\/edit$/, {
      title: 'Edit Store',
      root: '#aw-store-edit-page',
    });

    await visit(page, { path: '/sales/persons/new', title: 'New Sales Person', root: '#aw-sales-person-create-page' });
    await visit(page, { path: '/sales/persons', title: 'Sales Persons', root: '#aw-sales-person-list-page' });
    await followFirstResult(page, 'first sales person detail', viewResult, /\/sales\/persons\/\d+$/, {
      title: 'Sales Person Detail',
      root: '#aw-sales-person-detail-page',
    });
    await followFirstResult(page, 'sales person edit', '#aw-sales-person-detail-edit', /\/sales\/persons\/\d+\/edit$/, {
      title: 'Edit Sales Person',
      root: '#aw-sales-person-edit-page',
    });

    await visit(page, { path: '/sales/orders', title: 'Sales Orders', root: '#aw-order-list-page' });
    await followFirstResult(page, 'first sales order detail', viewResult, /\/sales\/orders\/\d+$/, {
      title: 'Sales Order Detail',
      root: '#aw-order-detail-page',
    });

    await visit(page, { path: '/sales/customers', title: 'Customers', root: '#aw-customer-list-page' });
    await followFirstResult(page, 'first customer detail', viewResult, /\/sales\/customers\/\d+$/, {
      title: 'Customer Detail',
      root: '#aw-customer-detail-page',
    });
  });

  test('Human Resources routes', async ({ page }) => {
    await visit(page, {
      path: '/hr',
      title: 'Employees',
      root: '#aw-employee-list-page',
      redirectedTo: /\/hr\/employees$/,
    });
    await visit(page, { path: '/hr/employees/new', title: 'New Employee', root: '#aw-employee-create-page' });
    await visit(page, { path: '/hr/employees', title: 'Employees', root: '#aw-employee-list-page' });
    await followFirstResult(page, 'first employee detail', viewResult, /\/hr\/employees\/\d+$/, {
      title: 'Employee Detail',
      root: '#aw-employee-detail-page',
    });
    await visit(page, { path: '/hr/org-chart', title: 'Organization Chart', root: '#aw-org-chart-page' });
    await visit(page, { path: '/hr/dashboard', title: 'HR Dashboard' });
    await visit(page, { path: '/hr/departments/new', title: 'New Department', root: '#aw-department-create-page' });
    await visit(page, { path: '/hr/departments', title: 'Departments', root: '#aw-department-list-page' });
    await followFirstResult(page, 'first department detail', viewResult, /\/hr\/departments\/\d+$/, {
      title: 'Department Detail',
      root: '#aw-department-detail-page',
    });
    await followFirstResult(
      page,
      'department edit',
      '#aw-department-detail-edit-btn',
      /\/hr\/departments\/\d+\/edit$/,
      {
        title: 'Edit Department',
        root: '#aw-department-edit-page',
      },
    );
  });

  test('Manufacturing routes', async ({ page }) => {
    await visit(page, {
      path: '/manufacturing',
      title: 'Manufacturing Dashboard',
      root: '#aw-manufacturing-kpi-dashboard-page',
    });
    await visit(page, {
      path: '/manufacturing/work-orders',
      title: 'Work Orders',
      root: '#aw-work-order-list-page',
    });
    await followFirstResult(page, 'first work order detail', viewResult, /\/manufacturing\/work-orders\/\d+$/, {
      title: 'Work Order Detail',
      root: '#aw-work-order-detail-page',
    });
  });

  test('Purchasing routes', async ({ page }) => {
    await visit(page, {
      path: '/purchasing',
      title: 'Vendors',
      root: '#aw-vendor-list-page',
      redirectedTo: /\/purchasing\/vendors$/,
    });
    await visit(page, {
      path: '/purchasing/analytics',
      title: 'Purchasing Analytics',
      root: '#aw-purchasing-analytics-page',
    });
    await visit(page, { path: '/purchasing/vendors', title: 'Vendors', root: '#aw-vendor-list-page' });
    await followFirstResult(page, 'first vendor detail', viewResult, /\/purchasing\/vendors\/\d+$/, {
      title: 'Vendor Detail',
      root: '#aw-vendor-detail-page',
    });
    await followFirstResult(
      page,
      'first purchase order detail',
      'a[href^="/purchasing/purchase-orders/"]',
      /\/purchasing\/purchase-orders\/\d+$/,
      { title: 'Purchase Order Detail', root: '#aw-purchase-order-detail-page' },
    );
  });
});
