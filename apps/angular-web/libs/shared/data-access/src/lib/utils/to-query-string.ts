type QueryParamValue = string | number | boolean | undefined | null;

// Mapped-type constraint (not `Record<string, QueryParamValue>`): a plain interface without an
// index signature isn't assignable to `Record<string, X>` even when every property is
// compatible. The mapped form accepts any params interface without forcing callers to add one.
export function toQueryString<T extends { [K in keyof T]: QueryParamValue }>(params: T): string {
  const searchParams = new URLSearchParams();

  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null) {
      searchParams.append(key, String(value));
    }
  }

  const result = searchParams.toString();
  return result ? `?${result}` : '';
}
