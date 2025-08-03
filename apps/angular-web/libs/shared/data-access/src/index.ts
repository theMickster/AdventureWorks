// Models
export type { SearchResult } from './lib/models/search-result.model';
export type { PaginationParams } from './lib/models/pagination-params.model';
export type { ValidationError } from './lib/models/validation-error.model';
export type { ApiError } from './lib/models/api-error.model';
export type { SlimReference } from './lib/models/slim-reference.model';
export type { LookupType } from './lib/models/lookup-type.model';
export type { AddressType } from './lib/models/lookup/address-type.model';
export type { CountryRegion } from './lib/models/lookup/country-region.model';
export type { Department } from './lib/models/lookup/department.model';
export type { SalesTerritory } from './lib/models/lookup/sales-territory.model';
export type { Shift } from './lib/models/lookup/shift.model';
export type { StateProvince } from './lib/models/lookup/state-province.model';

// Error classes
export { ApiValidationError } from './lib/models/errors/api-validation-error';
export { ApiEmptyResultError } from './lib/models/errors/api-empty-result-error';

// Store features
export type { RequestStatus } from './lib/store-features/with-request-status';
export { withRequestStatus, setLoading, setLoaded, setError } from './lib/store-features/with-request-status';
export { withPagination, setPaginationFromResult } from './lib/store-features/with-pagination';

// Services
export { LookupApiService } from './lib/services/lookup-api.service';

// Stores
export { LookupStore } from './lib/stores/lookup.store';

// Utilities
export { toQueryString } from './lib/utils/to-query-string';
