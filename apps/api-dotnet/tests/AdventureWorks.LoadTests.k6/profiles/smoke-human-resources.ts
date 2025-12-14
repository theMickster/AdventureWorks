import { sleep } from 'k6';
import { createHandleSummary, getProfileOptions, loadTestConfig } from '../config.ts';
import { authenticatedGet } from '../helpers/http.ts';

export const options = getProfileOptions('smoke', {}, { http_req_duration: ['p(95)<300'] });
export const handleSummary = createHandleSummary('smoke-human-resources');

export default function () {
  if (!loadTestConfig.authToken) {
    console.warn('K6_AUTH_TOKEN/AUTH_TOKEN not provided; all HumanResources endpoints require auth, so smoke-human-resources has nothing to check.');
    sleep(1);
    return;
  }

  authenticatedGet('/api/v1/employees');
  authenticatedGet('/api/v1/employees/1');
  authenticatedGet('/api/v1/departments');
  authenticatedGet('/api/v1/departments/1');
  authenticatedGet('/api/v1/shifts');

  authenticatedGet('/api/v1/employees/999999999', { expectedStatus: 404 });
  // Departments' id parameter binds to a C# `short` (max 32767); an id as large as
  // 999999999 fails short-conversion model binding and yields 400, not 404. 2147
  // mirrors the Postman collection's "Departments > By Id - Not Found" precedent.
  authenticatedGet('/api/v1/departments/2147', { expectedStatus: 404 });

  sleep(1);
}
