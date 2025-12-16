import { sleep } from 'k6';
import { createHandleSummary, getProfileOptions, loadTestConfig } from '../config.ts';
import { authenticatedGet } from '../helpers/http.ts';

export const options = getProfileOptions('smoke', {}, { http_req_duration: ['p(95)<300'] });
export const handleSummary = createHandleSummary('smoke-sales-person');

// Confirmed against the local AdventureWorks DB: BusinessEntityID 275 is Michael Blythe in
// Sales.SalesPerson.
const SALES_PERSON_ID = 275;

export default function () {
  if (!loadTestConfig.authToken) {
    console.warn('K6_AUTH_TOKEN/AUTH_TOKEN not provided; all Sales Person endpoints require auth, so smoke-sales-person has nothing to check.');
    sleep(1);
    return;
  }

  authenticatedGet('/api/v1/salespersons');
  authenticatedGet(`/api/v1/salespersons/${SALES_PERSON_ID}`);
  authenticatedGet(`/api/v1/salespersons/${SALES_PERSON_ID}/performance`);

  authenticatedGet('/api/v1/salespersons/999999999', { expectedStatus: 404 });

  sleep(1);
}
