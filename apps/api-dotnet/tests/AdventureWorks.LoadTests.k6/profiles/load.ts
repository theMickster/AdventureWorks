import http from 'k6/http';
import { sleep } from 'k6';
import { createHandleSummary, getProfileOptions, loadTestConfig } from '../config.ts';
import { authenticatedGet, checkResponse } from '../helpers/http.ts';

export const options = getProfileOptions('load');
export const handleSummary = createHandleSummary('load');

// Confirmed against the local AdventureWorks DB: BusinessEntityID 292 (Next-Door Bike Store)
// exists in Sales.Store.
const STORE_ID = 292;

export default function () {
  const healthResponse = http.get(`${loadTestConfig.baseUrl}/health`);
  checkResponse(healthResponse, { name: 'GET /health' });

  const versionResponse = http.get(`${loadTestConfig.baseUrl}/version`);
  checkResponse(versionResponse, { name: 'GET /version' });

  if (loadTestConfig.authToken) {
    // load profile is scoped to reads only; /api/v1/stores/search is a POST-shaped
    // write-style call and belongs in the stress profile instead.
    authenticatedGet('/api/v1/stores?pageNumber=1&pageSize=10');
    authenticatedGet(`/api/v1/stores/${STORE_ID}`);
    authenticatedGet(`/api/v1/stores/${STORE_ID}/addresses`);
    authenticatedGet(`/api/v1/stores/${STORE_ID}/contacts`);
  } else {
    console.warn('K6_AUTH_TOKEN/AUTH_TOKEN not provided; skipping protected store endpoints in load profile.');
  }

  sleep(0.5);
}
