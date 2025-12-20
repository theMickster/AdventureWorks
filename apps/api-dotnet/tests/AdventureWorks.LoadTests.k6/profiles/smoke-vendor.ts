import { sleep } from 'k6';
import { createHandleSummary, getProfileOptions, loadTestConfig } from '../config.ts';
import { authenticatedGet } from '../helpers/http.ts';

export const options = getProfileOptions('smoke', {}, { http_req_duration: ['p(95)<300'] });
export const handleSummary = createHandleSummary('smoke-vendor');

export default function () {
  if (!loadTestConfig.authToken) {
    console.warn('K6_AUTH_TOKEN/AUTH_TOKEN not provided; the vendor list endpoint requires auth, so smoke-vendor has nothing to check.');
    sleep(1);
    return;
  }

  authenticatedGet('/api/v1/vendors');
  authenticatedGet('/api/v1/vendors?creditRating=4');
  authenticatedGet('/api/v1/vendors?pageNumber=999999&pageSize=25');

  // creditRating=9 is out of the valid 1-5 range and must be rejected with 400.
  authenticatedGet('/api/v1/vendors?creditRating=9', { expectedStatus: 400 });

  sleep(1);
}
