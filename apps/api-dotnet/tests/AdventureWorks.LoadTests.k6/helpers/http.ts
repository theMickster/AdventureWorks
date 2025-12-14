import { check } from 'k6';
import http from 'k6/http';
import { loadTestConfig } from '../config.ts';

function normalizePath(path: string): string {
  return path.startsWith('/') ? path : `/${path}`;
}

function buildUrl(path: string): string {
  return `${loadTestConfig.baseUrl}${normalizePath(path)}`;
}

function requireAuthToken(): string {
  if (!loadTestConfig.authToken) {
    throw new Error('K6_AUTH_TOKEN or AUTH_TOKEN is required for authenticated requests.');
  }

  return loadTestConfig.authToken;
}

export function checkResponse(response: http.Response, { name, expectedStatus = 200 }: { name?: string; expectedStatus?: number } = {}): boolean {
  const responseName = name || 'HTTP request';

  return check(response, {
    [`${responseName} returns ${expectedStatus}`]: (r) => r.status === expectedStatus,
    [`${responseName} duration under 1.5s`]: (r) => r.timings.duration < 1500,
  });
}

export function authenticatedGet(
  path: string,
  { expectedStatus = 200, headers = {}, tags = {}, ...requestOptions }: { expectedStatus?: number; headers?: Record<string, string>; tags?: Record<string, string> } = {},
): http.Response {
  const token = requireAuthToken();

  const response = http.get(buildUrl(path), {
    ...requestOptions,
    tags,
    responseCallback: http.expectedStatuses(expectedStatus),
    headers: {
      ...headers,
      Authorization: `Bearer ${token}`,
    },
  });

  checkResponse(response, { name: `GET ${normalizePath(path)}`, expectedStatus });
  return response;
}

export function authenticatedPost(
  path: string,
  payload: unknown,
  { expectedStatus = 200, headers = {}, tags = {}, ...requestOptions }: { expectedStatus?: number; headers?: Record<string, string>; tags?: Record<string, string> } = {},
): http.Response {
  const token = requireAuthToken();

  const response = http.post(buildUrl(path), JSON.stringify(payload), {
    ...requestOptions,
    tags,
    responseCallback: http.expectedStatuses(expectedStatus),
    headers: {
      'Content-Type': 'application/json',
      ...headers,
      Authorization: `Bearer ${token}`,
    },
  });

  checkResponse(response, { name: `POST ${normalizePath(path)}`, expectedStatus });
  return response;
}
