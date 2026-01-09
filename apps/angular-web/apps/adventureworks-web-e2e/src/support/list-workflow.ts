import { expect, Page, Response } from '@playwright/test';

type QueryState = Record<string, string | null>;

/** Waits for the list request caused by an interaction and rejects unsuccessful API responses. */
export async function waitForListResponse(
  page: Page,
  path: string,
  action: () => Promise<unknown>,
  method = 'GET',
): Promise<Response> {
  const responsePromise = page.waitForResponse(
    (response) => response.request().method() === method && new URL(response.url()).pathname === path,
  );

  await action();
  const response = await responsePromise;
  expect(response.status(), `${method} ${path} should succeed`).toBeLessThan(400);
  return response;
}

/** Asserts the relevant URL-query state without depending on parameter order. */
export async function expectQueryState(page: Page, expected: QueryState): Promise<void> {
  await expect
    .poll(() => {
      const url = new URL(page.url());
      return Object.fromEntries(Object.keys(expected).map((key) => [key, url.searchParams.get(key)]));
    })
    .toEqual(expected);
}
