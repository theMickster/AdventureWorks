import { TestBed } from '@angular/core/testing';
import { patchState, signalStore } from '@ngrx/signals';
import { unprotected } from '@ngrx/signals/testing';
import { withRequestStatus, setLoading, setLoaded, setError } from './with-request-status';

const TestStore = signalStore({ providedIn: 'root' }, withRequestStatus());

describe('withRequestStatus', () => {
  let store: InstanceType<typeof TestStore>;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    store = TestBed.inject(TestStore);
  });

  it('should have idle initial state', () => {
    expect(store.requestStatus()).toBe('idle');
    expect(store.error()).toBeNull();
  });

  it('should have all computed false initially', () => {
    expect(store.isLoading()).toBe(false);
    expect(store.isLoaded()).toBe(false);
    expect(store.hasError()).toBe(false);
  });

  it('should transition to loading', () => {
    patchState(unprotected(store), setLoading());
    expect(store.requestStatus()).toBe('loading');
    expect(store.isLoading()).toBe(true);
    expect(store.isLoaded()).toBe(false);
    expect(store.error()).toBeNull();
  });

  it('should transition to loaded', () => {
    patchState(unprotected(store), setLoaded());
    expect(store.requestStatus()).toBe('loaded');
    expect(store.isLoaded()).toBe(true);
    expect(store.isLoading()).toBe(false);
  });

  it('should transition to error', () => {
    patchState(unprotected(store), setError('Something failed'));
    expect(store.requestStatus()).toBe('error');
    expect(store.hasError()).toBe(true);
    expect(store.error()).toBe('Something failed');
    expect(store.isLoading()).toBe(false);
  });

  it('should clear error when transitioning to loading', () => {
    patchState(unprotected(store), setError('fail'));
    expect(store.error()).toBe('fail');

    patchState(unprotected(store), setLoading());
    expect(store.error()).toBeNull();
    expect(store.isLoading()).toBe(true);
  });
});
