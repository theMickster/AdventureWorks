import { sleep } from 'k6';
import { createHandleSummary, getProfileOptions, loadTestConfig } from '../config.ts';
import { anonymousGet, authenticatedGet } from '../helpers/http.ts';

export const options = getProfileOptions('smoke', {}, { http_req_duration: ['p(95)<300'] });
export const handleSummary = createHandleSummary('smoke-person');

export default function () {
  anonymousGet('/api/v1/countries');
  anonymousGet('/api/v1/states');

  if (!loadTestConfig.authToken) {
    console.warn('K6_AUTH_TOKEN/AUTH_TOKEN not provided; Person endpoints require auth, so smoke-person has nothing further to check.');
    sleep(1);
    return;
  }

  // firstName is required by SearchPersonsQueryValidator; bare /persons returns 400.
  authenticatedGet('/api/v1/persons?firstName=Ken');
  authenticatedGet('/api/v1/persons/1');

  // 999999999 fits within int range, so unlike Departments' short id, this is a clean 404 (no 400-width quirk).
  authenticatedGet('/api/v1/persons/999999999', { expectedStatus: 404 });

  sleep(1);
}
