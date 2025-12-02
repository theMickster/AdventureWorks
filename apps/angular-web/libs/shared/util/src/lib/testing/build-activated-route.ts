import { BehaviorSubject } from 'rxjs';

/** Options for {@link buildActivatedRoute}. */
export interface BuildActivatedRouteOptions {
  id?: string;
  queryParams?: Record<string, string>;
  paramMapGet?: (key: string) => string | null;
}

/** Mock `ActivatedRoute` shape returned by {@link buildActivatedRoute}. */
export interface MockActivatedRoute {
  snapshot: {
    queryParams: Record<string, string>;
    paramMap: { get: (key: string) => string | null };
  };
  queryParams: BehaviorSubject<Record<string, string>>;
}

/**
 * Builds a mock `ActivatedRoute` that is a strict superset of every inline shape duplicated across
 * existing specs — snapshot-only reads (`snapshot.queryParams`, `paramMap.get()`) AND reactive
 * subscription via the `queryParams` observable, on the same object.
 */
export function buildActivatedRoute(options: BuildActivatedRouteOptions = {}): MockActivatedRoute {
  const queryParams = options.queryParams ?? {};
  const paramMapGet = options.paramMapGet ?? ((key: string) => (key === 'id' ? (options.id ?? null) : null));

  return {
    snapshot: {
      queryParams,
      paramMap: { get: paramMapGet },
    },
    queryParams: new BehaviorSubject<Record<string, string>>(queryParams),
  };
}
