import { sleep } from 'k6';
import { createHandleSummary, getProfileOptions, loadTestConfig } from '../config.ts';
import { authenticatedGet } from '../helpers/http.ts';

export const options = getProfileOptions('smoke', {}, { http_req_duration: ['p(95)<300'] });
export const handleSummary = createHandleSummary('smoke-work-order');

// Confirmed against the local AdventureWorks DB: WorkOrderID 13 (ProductID 747) has
// EndDate (2011-06-19) after DueDate (2011-06-14), so it exercises the completed-late path.
const WORK_ORDER_PRODUCT_ID = 747;

export default function () {
  if (!loadTestConfig.authToken) {
    console.warn('K6_AUTH_TOKEN/AUTH_TOKEN not provided; the Work Orders endpoint requires auth, so smoke-work-order has nothing to check.');
    sleep(1);
    return;
  }

  authenticatedGet('/api/v1/work-orders');
  authenticatedGet(`/api/v1/work-orders?productId=${WORK_ORDER_PRODUCT_ID}`);
  authenticatedGet('/api/v1/work-orders?hasScrapped=true');
  authenticatedGet('/api/v1/work-orders?startDate=2011-01-01&endDate=2011-12-31');

  // Rule-02: startDate must be <= endDate.
  authenticatedGet('/api/v1/work-orders?startDate=2011-12-31&endDate=2011-01-01', { expectedStatus: 400 });

  sleep(1);
}
