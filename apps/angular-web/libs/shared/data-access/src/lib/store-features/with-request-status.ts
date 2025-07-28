import { computed } from '@angular/core';
import { signalStoreFeature, withComputed, withState } from '@ngrx/signals';

export type RequestStatus = 'idle' | 'loading' | 'loaded' | 'error';

export function setLoading(): {
  requestStatus: RequestStatus;
  error: null;
} {
  return { requestStatus: 'loading', error: null };
}

export function setLoaded(): {
  requestStatus: RequestStatus;
  error: null;
} {
  return { requestStatus: 'loaded', error: null };
}

export function setError(error: string): {
  requestStatus: RequestStatus;
  error: string;
} {
  return { requestStatus: 'error', error };
}

export function withRequestStatus() {
  return signalStoreFeature(
    withState<{ requestStatus: RequestStatus; error: string | null }>({
      requestStatus: 'idle',
      error: null,
    }),
    withComputed((store) => ({
      isLoading: computed(() => store.requestStatus() === 'loading'),
      isLoaded: computed(() => store.requestStatus() === 'loaded'),
      hasError: computed(() => store.requestStatus() === 'error'),
    })),
  );
}
