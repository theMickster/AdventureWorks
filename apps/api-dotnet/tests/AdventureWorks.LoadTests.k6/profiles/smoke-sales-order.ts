import { sleep } from 'k6';
import { createHandleSummary, getProfileOptions, loadTestConfig } from '../config.ts';
import { authenticatedGet } from '../helpers/http.ts';

export const options = getProfileOptions('smoke', {}, { http_req_duration: ['p(95)<300'] });
export const handleSummary = createHandleSummary('smoke-sales-order');

// Confirmed against the local AdventureWorks DB: SalesOrderID 43659 (SO43659) exists in
// Sales.SalesOrderHeader.
const SALES_ORDER_ID = 43659;

export default function () {
  if (!loadTestConfig.authToken) {
    console.warn('K6_AUTH_TOKEN/AUTH_TOKEN not provided; all Sales Order endpoints require auth, so smoke-sales-order has nothing to check.');
    sleep(1);
    return;
  }

  authenticatedGet('/api/v1/sales-orders');
  authenticatedGet(`/api/v1/sales-orders/${SALES_ORDER_ID}`);
  authenticatedGet('/api/v1/sales/dashboard');
  authenticatedGet('/api/v1/sales-reasons');
  // US-997's ADO acceptance criteria text says "sales-territories", but there is no such route -
  // ReadSalesTerritoryController exposes GET /api/v1/territories.
  authenticatedGet('/api/v1/territories');
  authenticatedGet('/api/v1/ship-methods');
  authenticatedGet('/api/v1/special-offers');

  authenticatedGet('/api/v1/sales-orders/999999999', { expectedStatus: 404 });

  sleep(1);
}
