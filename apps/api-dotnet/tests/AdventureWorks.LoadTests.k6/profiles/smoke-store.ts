import { sleep } from 'k6';
import { createHandleSummary, getProfileOptions, loadTestConfig } from '../config.ts';
import { authenticatedGet, authenticatedPost } from '../helpers/http.ts';

export const options = getProfileOptions('smoke', {}, { http_req_duration: ['p(95)<300'] });
export const handleSummary = createHandleSummary('smoke-store');

// Confirmed against the local AdventureWorks DB: BusinessEntityID 292 (Next-Door Bike Store)
// exists in Sales.Store.
const STORE_ID = 292;

export default function () {
  if (!loadTestConfig.authToken) {
    console.warn('K6_AUTH_TOKEN/AUTH_TOKEN not provided; all Store endpoints require auth, so smoke-store has nothing to check.');
    sleep(1);
    return;
  }

  authenticatedGet('/api/v1/stores');
  authenticatedGet(`/api/v1/stores/${STORE_ID}`);
  authenticatedPost('/api/v1/stores/search?pageNumber=1&pageSize=10', { name: 'bike' });
  authenticatedGet(`/api/v1/stores/${STORE_ID}/addresses`);
  authenticatedGet(`/api/v1/stores/${STORE_ID}/contacts`);

  // ReadStoreAddressController/ReadStoreContactController don't verify the parent store exists
  // (only storeId <= 0 -> 400), so the required 404 check must target the store detail endpoint.
  authenticatedGet('/api/v1/stores/999999999', { expectedStatus: 404 });

  sleep(1);
}
