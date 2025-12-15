import { sleep } from 'k6';
import { createHandleSummary, getProfileOptions, loadTestConfig } from '../config.ts';
import { anonymousGet, authenticatedGet } from '../helpers/http.ts';

export const options = getProfileOptions('smoke', {}, { http_req_duration: ['p(95)<300'] });
export const handleSummary = createHandleSummary('smoke-product-review');

// Confirmed against the local AdventureWorks DB (Production.ProductReview): ProductID 937 has
// 2 reviews, the most of any product, making it a reliable non-empty sample.
const REVIEWED_PRODUCT_ID = 937;

export default function () {
  if (!loadTestConfig.authToken) {
    // Both review endpoints require auth. Rather than skip, send the requests without a
    // bearer token and assert the 401 explicitly via anonymousGet's expectedStatus override -
    // this is the negative case from US-996, not an unhandled script error.
    anonymousGet(`/api/v1/products/${REVIEWED_PRODUCT_ID}/reviews`, { expectedStatus: 401 });
    anonymousGet(`/api/v1/products/${REVIEWED_PRODUCT_ID}/reviews/statistics`, { expectedStatus: 401 });
    sleep(1);
    return;
  }

  authenticatedGet(`/api/v1/products/${REVIEWED_PRODUCT_ID}/reviews`);
  authenticatedGet(`/api/v1/products/${REVIEWED_PRODUCT_ID}/reviews/statistics`);

  sleep(1);
}
