import http from 'k6/http';
import { sleep } from 'k6';
import { createHandleSummary, getProfileOptions, loadTestConfig } from '../config.ts';
import { authenticatedGet, authenticatedPost, checkResponse } from '../helpers/http.ts';

export const options = getProfileOptions('smoke');
export const handleSummary = createHandleSummary('smoke');

export default function () {
  const healthResponse = http.get(`${loadTestConfig.baseUrl}/health`);
  checkResponse(healthResponse, { name: 'GET /health' });

  const versionResponse = http.get(`${loadTestConfig.baseUrl}/version`);
  checkResponse(versionResponse, { name: 'GET /version' });

  if (loadTestConfig.authToken) {
    authenticatedGet('/api/v1/stores?pageNumber=1&pageSize=10');
    authenticatedPost('/api/v1/stores/search?pageNumber=1&pageSize=10', { name: 'bike' });
  } else {
    console.warn('K6_AUTH_TOKEN/AUTH_TOKEN not provided; skipping protected store endpoints in smoke profile.');
  }

  sleep(1);
}
