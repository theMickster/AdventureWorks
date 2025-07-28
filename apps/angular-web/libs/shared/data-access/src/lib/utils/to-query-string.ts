import { PaginationParams } from '../models/pagination-params.model';

export function toQueryString(params: PaginationParams): string {
  const searchParams = new URLSearchParams();

  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null) {
      searchParams.append(key, String(value));
    }
  }

  const result = searchParams.toString();
  return result ? `?${result}` : '';
}
