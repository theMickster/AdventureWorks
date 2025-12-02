import { Component, signal, WritableSignal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../auth/auth.service';
import { AuthUser } from '../auth/auth.model';
import { LoadingService } from '../loading/loading.service';
import { NotificationService } from '../notification/notification.service';
import { AppInsightsService } from '../telemetry/app-insights.service';
import { ENVIRONMENT } from '../environment/environment.token';
import { provideMockEnvironment } from './provide-mock-environment';
import { provideMockAuthService } from './provide-mock-auth-service';
import { provideMockNotificationService } from './provide-mock-notification-service';
import { provideMockLoadingService } from './provide-mock-loading-service';
import { provideMockAppInsightsService } from './provide-mock-app-insights-service';
import { buildActivatedRoute } from './build-activated-route';
import { renderComponent } from './render-component';
import { mockSearchResult } from './mock-search-result';

describe('provideMockEnvironment', () => {
  it('defaults to the literal duplicated across existing specs', () => {
    TestBed.configureTestingModule({ providers: [provideMockEnvironment()] });
    const env = TestBed.inject(ENVIRONMENT);

    expect(env).toEqual({
      production: false,
      defaultLocale: 'en',
      api: { primary: { baseUrl: 'https://api.test.com', name: 'Test API' } },
    });
  });

  it('merges overrides over the defaults', () => {
    TestBed.configureTestingModule({ providers: [provideMockEnvironment({ production: true })] });
    const env = TestBed.inject(ENVIRONMENT);

    expect(env.production).toBe(true);
    expect(env.defaultLocale).toBe('en');
  });
});

describe('provideMockAuthService', () => {
  it('defaults to unauthenticated with no user', () => {
    TestBed.configureTestingModule({ providers: [provideMockAuthService()] });
    const auth = TestBed.inject(AuthService);

    expect(auth.isAuthenticated()).toBe(false);
    expect(auth.user()).toBeNull();
    expect(auth.displayName()).toBe('');
    expect(auth.userInitials()).toBe('');
  });

  it('applies overrides', () => {
    TestBed.configureTestingModule({ providers: [provideMockAuthService({ isAuthenticated: signal(true) })] });
    const auth = TestBed.inject(AuthService);

    expect(auth.isAuthenticated()).toBe(true);
  });

  it('displayName and userInitials update reactively when the user signal is set', () => {
    TestBed.configureTestingModule({ providers: [provideMockAuthService()] });
    const auth = TestBed.inject(AuthService);

    (auth.user as WritableSignal<AuthUser | null>).set({
      name: 'Michael Blythe',
      email: 'michael@example.com',
      oid: 'oid-1',
      username: 'michael',
    });

    expect(auth.displayName()).toBe('Michael Blythe');
    expect(auth.userInitials()).toBe('MB');
  });

  it('remains reactive when the caller supplies its own user override signal', () => {
    const user = signal<AuthUser | null>({
      name: 'Tsvi Reiter',
      email: 'tsvi@example.com',
      oid: 'oid-2',
      username: 'tsvi',
    });
    TestBed.configureTestingModule({ providers: [provideMockAuthService({ user })] });
    const auth = TestBed.inject(AuthService);

    expect(auth.displayName()).toBe('Tsvi Reiter');
    expect(auth.userInitials()).toBe('TR');

    user.set(null);

    expect(auth.displayName()).toBe('');
    expect(auth.userInitials()).toBe('');
  });
});

describe('provideMockNotificationService', () => {
  it('defaults to an empty notification queue', () => {
    TestBed.configureTestingModule({ providers: [provideMockNotificationService()] });
    const notifications = TestBed.inject(NotificationService);

    expect(notifications.notifications()).toEqual([]);
  });

  it('applies overrides', () => {
    const errorSpy = vi.fn();
    TestBed.configureTestingModule({ providers: [provideMockNotificationService({ error: errorSpy })] });
    const notifications = TestBed.inject(NotificationService);

    notifications.error('boom');
    expect(errorSpy).toHaveBeenCalledWith('boom');
  });
});

describe('provideMockLoadingService', () => {
  it('defaults to not loading', () => {
    TestBed.configureTestingModule({ providers: [provideMockLoadingService()] });
    const loading = TestBed.inject(LoadingService);

    expect(loading.isLoading()).toBe(false);
  });

  it('applies overrides', () => {
    TestBed.configureTestingModule({ providers: [provideMockLoadingService({ isLoading: signal(true) })] });
    const loading = TestBed.inject(LoadingService);

    expect(loading.isLoading()).toBe(true);
  });
});

describe('provideMockAppInsightsService', () => {
  it('defaults to no-op methods', async () => {
    TestBed.configureTestingModule({ providers: [provideMockAppInsightsService()] });
    const appInsights = TestBed.inject(AppInsightsService);

    await expect(appInsights.initialize()).resolves.toBeUndefined();
    expect(() => appInsights.trackEvent('event')).not.toThrow();
    expect(() => appInsights.trackException(new Error('fail'))).not.toThrow();
  });

  it('applies overrides', () => {
    const trackEventSpy = vi.fn();
    TestBed.configureTestingModule({ providers: [provideMockAppInsightsService({ trackEvent: trackEventSpy })] });
    const appInsights = TestBed.inject(AppInsightsService);

    appInsights.trackEvent('event', { key: 'value' });
    expect(trackEventSpy).toHaveBeenCalledWith('event', { key: 'value' });
  });
});

describe('buildActivatedRoute', () => {
  it('defaults to empty queryParams and null paramMap lookups', () => {
    const route = buildActivatedRoute();

    expect(route.snapshot.queryParams).toEqual({});
    expect(route.snapshot.paramMap.get('anything')).toBeNull();
  });

  it('resolves paramMap.get("id") from the id option, other keys stay null', () => {
    const route = buildActivatedRoute({ id: '275' });

    expect(route.snapshot.paramMap.get('id')).toBe('275');
    expect(route.snapshot.paramMap.get('other')).toBeNull();
  });

  it('reflects queryParams in both the snapshot and the reactive observable', () => {
    const route = buildActivatedRoute({ queryParams: { search: 'Bike' } });

    expect(route.snapshot.queryParams).toEqual({ search: 'Bike' });

    let emitted: Record<string, string> | undefined;
    route.queryParams.subscribe((value) => (emitted = value));
    expect(emitted).toEqual({ search: 'Bike' });
  });

  it('a custom paramMapGet overrides the id shortcut', () => {
    const paramMapGet = vi.fn().mockReturnValue('custom-value');
    const route = buildActivatedRoute({ id: '275', paramMapGet });

    expect(route.snapshot.paramMap.get('id')).toBe('custom-value');
  });
});

describe('mockSearchResult', () => {
  it('defaults pageNumber and pageSize', () => {
    const result = mockSearchResult();

    expect(result.pageNumber).toBe(1);
    expect(result.pageSize).toBe(10);
  });

  it('computes pagination fields for the first page', () => {
    const result = mockSearchResult({ pageNumber: 1, pageSize: 10, totalRecords: 25 });

    expect(result.totalPages).toBe(3);
    expect(result.hasPreviousPage).toBe(false);
    expect(result.hasNextPage).toBe(true);
  });

  it('computes pagination fields for a middle page', () => {
    const result = mockSearchResult({ pageNumber: 2, pageSize: 10, totalRecords: 25 });

    expect(result.totalPages).toBe(3);
    expect(result.hasPreviousPage).toBe(true);
    expect(result.hasNextPage).toBe(true);
  });

  it('computes pagination fields for the last page', () => {
    const result = mockSearchResult({ pageNumber: 3, pageSize: 10, totalRecords: 25 });

    expect(result.totalPages).toBe(3);
    expect(result.hasPreviousPage).toBe(true);
    expect(result.hasNextPage).toBe(false);
  });

  it('empty results produce zeroed pagination', () => {
    const result = mockSearchResult({ results: [] });

    expect(result.totalRecords).toBe(0);
    expect(result.totalPages).toBe(0);
    expect(result.hasPreviousPage).toBe(false);
    expect(result.hasNextPage).toBe(false);
  });

  it('preserves an explicit results: null instead of coercing to []', () => {
    const result = mockSearchResult({ results: null });

    expect(result.results).toBeNull();
    expect(result.totalRecords).toBe(0);
    expect(result.totalPages).toBe(0);
  });

  it('an explicit totalRecords wins over results.length when they disagree', () => {
    const result = mockSearchResult({ results: [1, 2], totalRecords: 50, pageSize: 10 });

    expect(result.totalRecords).toBe(50);
    expect(result.totalPages).toBe(5);
    expect(result.results).toEqual([1, 2]);
  });

  it('pageSize of 0 short-circuits to zero totalPages instead of dividing by zero', () => {
    const result = mockSearchResult({ pageSize: 0, totalRecords: 10 });

    expect(result.totalPages).toBe(0);
    expect(Number.isFinite(result.totalPages)).toBe(true);
    expect(result.hasNextPage).toBe(false);
  });

  it('an exact multiple of pageSize does not overshoot totalPages by one', () => {
    const result = mockSearchResult({ pageNumber: 2, pageSize: 10, totalRecords: 20 });

    expect(result.totalPages).toBe(2);
    expect(result.hasNextPage).toBe(false);
    expect(result.hasPreviousPage).toBe(true);
  });
});

@Component({ selector: 'aw-test-render-component', template: '<p>test</p>' })
class TestRenderComponent {}

describe('renderComponent', () => {
  it('returns fixture, component, and element', async () => {
    const { fixture, component, element } = await renderComponent(TestRenderComponent);

    expect(component).toBeInstanceOf(TestRenderComponent);
    expect(fixture.componentInstance).toBe(component);
    expect(element).toBe(fixture.nativeElement);
  });

  it('injects the 5 default mock providers with the expected mock shapes', async () => {
    await renderComponent(TestRenderComponent);

    expect(TestBed.inject(ENVIRONMENT).api.primary.baseUrl).toBe('https://api.test.com');
    expect(TestBed.inject(AuthService).isAuthenticated()).toBe(false);
    expect(TestBed.inject(NotificationService).notifications()).toEqual([]);
    expect(TestBed.inject(LoadingService).isLoading()).toBe(false);
    expect(() => TestBed.inject(AppInsightsService).trackEvent('event')).not.toThrow();
  });

  it('an override provider shadows the matching default', async () => {
    await renderComponent(TestRenderComponent, {
      providers: [provideMockAuthService({ isAuthenticated: signal(true) })],
    });

    expect(TestBed.inject(AuthService).isAuthenticated()).toBe(true);
  });

  it('options.imports is threaded through to TestBed, not silently dropped', async () => {
    // TranslateModule.forRoot() registers TranslateService; if renderComponent dropped
    // options.imports, TestBed.inject(TranslateService) below would throw NullInjectorError.
    await renderComponent(TestRenderComponent, {
      imports: [TranslateModule.forRoot()],
    });

    expect(() => TestBed.inject(TranslateService)).not.toThrow();
  });
});
