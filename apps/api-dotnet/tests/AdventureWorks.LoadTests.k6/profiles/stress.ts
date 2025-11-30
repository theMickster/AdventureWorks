import http from 'k6/http';
import { sleep } from 'k6';
import { Rate } from 'k6/metrics';
import { createHandleSummary, getProfileOptions, loadTestConfig } from '../config.ts';
import { authenticatedGet, authenticatedPost, checkResponse } from '../helpers/http.ts';

const breakingPointErrorRate = new Rate('breaking_point_error_rate');

export const options = getProfileOptions('stress');
export const handleSummary = createHandleSummary('stress');

export default function () {
  const healthResponse = http.get(`${loadTestConfig.baseUrl}/health`);
  checkResponse(healthResponse, { name: 'GET /health' });

  const versionResponse = http.get(`${loadTestConfig.baseUrl}/version`);
  checkResponse(versionResponse, { name: 'GET /version' });

  let hasServerError = healthResponse.status >= 500 || versionResponse.status >= 500;

  if (loadTestConfig.authToken) {
    const storesResponse = authenticatedGet('/api/v1/stores?pageNumber=1&pageSize=10');
    const storeSearchResponse = authenticatedPost('/api/v1/stores/search?pageNumber=1&pageSize=10', { name: 'bike' });
    hasServerError = hasServerError || storesResponse.status >= 500 || storeSearchResponse.status >= 500;
  } else {
    console.warn('K6_AUTH_TOKEN/AUTH_TOKEN not provided; skipping protected store endpoints in stress profile.');
  }

  breakingPointErrorRate.add(hasServerError);
  sleep(0.25);
}
