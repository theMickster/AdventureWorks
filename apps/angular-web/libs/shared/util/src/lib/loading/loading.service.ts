import { computed, Injectable, signal } from '@angular/core';

/** Tracks active loading operations via ref-counting. Used by HTTP interceptors and components. */
@Injectable({ providedIn: 'root' })
export class LoadingService {
  private readonly activeRequests = signal(0);

  /** Whether any loading operation is in progress. */
  readonly isLoading = computed(() => this.activeRequests() > 0);

  /** Increment the active request count. */
  start(): void {
    this.activeRequests.update((n) => n + 1);
  }

  /** Decrement the active request count (floors at 0). */
  stop(): void {
    this.activeRequests.update((n) => Math.max(0, n - 1));
  }
}
