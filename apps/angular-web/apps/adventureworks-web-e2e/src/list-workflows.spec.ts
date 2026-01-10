import { expect, Page, test } from '@playwright/test';
import { expectQueryState, waitForListResponse } from './support/list-workflow';

const pageErrors = new WeakMap<Page, string[]>();

test.beforeEach(({ page }) => {
  const errors: string[] = [];
  pageErrors.set(page, errors);
  page.on('pageerror', (error) => errors.push(error.message));
});

test.afterEach(({ page }) => {
  expect(pageErrors.get(page), 'browser page errors').toEqual([]);
});

test.describe('Authenticated read-only list workflows', () => {
  test('searches, clears, and sorts stores', async ({ page }) => {
    await waitForListResponse(page, '/api/v1/stores', () => page.goto('/sales/stores'));
    await expect(page.locator('#aw-store-list-table-table')).toBeVisible();

    await page.locator('#aw-store-search-input').fill('Bike');
    await waitForListResponse(
      page,
      '/api/v1/stores/search',
      () => page.locator('#aw-store-search-btn').click(),
      'POST',
    );
    await expectQueryState(page, { search: 'Bike', pageNumber: '1', orderBy: null, sortOrder: null });
    await expect.poll(() => page.locator('#aw-store-list-table-table tbody tr').count()).toBeGreaterThan(0);
    await expect(page.locator('#aw-store-list-table-table tbody')).toContainText(/Bike/i);

    await waitForListResponse(page, '/api/v1/stores', () => page.locator('#aw-store-clear-btn').click());
    await expectQueryState(page, { search: null, pageNumber: null, orderBy: null, sortOrder: null });

    await waitForListResponse(page, '/api/v1/stores', () =>
      page.locator('#aw-store-list-table-th-name button').click(),
    );
    await expectQueryState(page, { search: null, pageNumber: null, orderBy: 'name', sortOrder: 'asc' });
  });

  test('filters and resets employees', async ({ page }) => {
    await waitForListResponse(page, '/api/v1/employees', () => page.goto('/hr/employees'));
    await expect(page.locator('#aw-employee-list-table-table')).toBeVisible();

    await page.locator('#aw-employee-filter-name').fill('Ken Sánchez');
    await page.locator('#aw-employee-filter-status-select').selectOption('active');
    await waitForListResponse(
      page,
      '/api/v1/employees/search',
      () => page.locator('#aw-employee-filter-apply-btn').click(),
      'POST',
    );
    await expectQueryState(page, { name: 'Ken Sánchez', status: 'active', pageNumber: '1' });
    await expect(page.locator('#aw-employee-list-table-table tbody')).toContainText('Ken Sánchez');
    await expect(page.locator('#aw-employee-list-table-table tbody')).toContainText('active');

    await waitForListResponse(page, '/api/v1/employees', () => page.locator('#aw-employee-filter-reset-btn').click());
    await expectQueryState(page, { name: null, status: null, pageNumber: null });
    await expect(page.locator('#aw-employee-filter-name')).toHaveValue('');
    await expect(page.locator('#aw-employee-filter-status-select')).toHaveValue('');
  });

  test('filters, sorts, and resets work orders', async ({ page }) => {
    await waitForListResponse(page, '/api/v1/work-orders', () => page.goto('/manufacturing/work-orders'));
    await expect(page.locator('#aw-work-order-list-table-table')).toBeVisible();

    await page.locator('#aw-work-order-filter-has-scrapped').selectOption('true');
    await waitForListResponse(page, '/api/v1/work-orders', () =>
      page.locator('#aw-work-order-filter-apply-btn').click(),
    );
    await expectQueryState(page, { hasScrapped: 'true', pageNumber: '1', orderBy: null, sortOrder: null });
    await expect.poll(() => page.locator('#aw-work-order-list-table-table tbody tr').count()).toBeGreaterThan(0);

    await waitForListResponse(page, '/api/v1/work-orders', () =>
      page.locator('#aw-work-order-list-table-th-startDate button').click(),
    );
    await expectQueryState(page, { hasScrapped: 'true', pageNumber: '1', orderBy: 'startDate', sortOrder: 'asc' });

    await waitForListResponse(page, '/api/v1/work-orders', () =>
      page.locator('#aw-work-order-filter-reset-btn').click(),
    );
    await expectQueryState(page, { hasScrapped: null, pageNumber: null, orderBy: null, sortOrder: null });
    await expect(page.locator('#aw-work-order-filter-has-scrapped')).toHaveValue('');
  });
});
