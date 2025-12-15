import { sleep } from 'k6';
import { createHandleSummary, getProfileOptions, loadTestConfig } from '../config.ts';
import { anonymousGet, authenticatedGet } from '../helpers/http.ts';

export const options = getProfileOptions('smoke', {}, { http_req_duration: ['p(95)<300'] });
export const handleSummary = createHandleSummary('smoke-production');

export default function () {
  // [AllowAnonymous] by design (ReadProductCategoriesController) - reference/dropdown data.
  anonymousGet('/api/v1/products/categories');

  if (!loadTestConfig.authToken) {
    console.warn('K6_AUTH_TOKEN/AUTH_TOKEN not provided; Products/ProductModels endpoints require auth, so smoke-production has nothing further to check.');
    sleep(1);
    return;
  }

  authenticatedGet('/api/v1/products');
  authenticatedGet('/api/v1/products/316');
  authenticatedGet('/api/v1/product-models');

  // Product's id route parameter is int-typed, so unlike Departments' short id, 999999999 fits
  // cleanly and yields a genuine 404 (no 400-width binding quirk).
  authenticatedGet('/api/v1/products/999999999', { expectedStatus: 404 });

  sleep(1);
}
