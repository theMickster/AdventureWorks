import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal, WritableSignal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AuthService } from '../auth/auth.service';
import { NotificationService } from '../notification/notification.service';
import { AppInsightsService } from '../telemetry/app-insights.service';
import { ENVIRONMENT } from '../environment/environment.token';
import { Environment } from '../environment/environment.model';
import { ApiEmptyResultError } from './errors/api-empty-result-error';
import { ApiValidationError } from './errors/api-validation-error';
import { errorInterceptor } from './error.interceptor';

const mockEnvironment: Environment = {
  production: false,
  defaultLocale: 'en',
  api: { primary: { baseUrl: 'https://localhost:5001/api', name: 'Primary' } },
};

describe('errorInterceptor', () => {
  let httpClient: HttpClient;
  let httpTesting: HttpTestingController;
  let notificationService: NotificationService;
  let authService: AuthService;
  let appInsightsService: AppInsightsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        {
          provide: AuthService,
          useValue: {
            isAuthenticated: signal(false),
            login: vi.fn(),
          },
        },
        {
          provide: AppInsightsService,
          useValue: {
            trackEvent: vi.fn(),
            trackException: vi.fn(),
          },
        },
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    notificationService = TestBed.inject(NotificationService);
    authService = TestBed.inject(AuthService);
    appInsightsService = TestBed.inject(AppInsightsService);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should throw ApiValidationError for 400 with array body', () => {
    const toastSpy = vi.spyOn(notificationService, 'error');
    const errors = [{ propertyName: 'name', errorCode: 'R01', errorMessage: 'Required', correlationId: 'c-1' }];

    httpClient.get('/api/test').subscribe({
      error: (err: unknown) => {
        expect(err).toBeInstanceOf(ApiValidationError);
        expect((err as ApiValidationError).errors).toHaveLength(1);
        expect((err as ApiValidationError).correlationId).toBeTruthy();
      },
    });

    httpTesting.expectOne('/api/test').flush(errors, { status: 400, statusText: 'Bad Request' });
    expect(toastSpy).not.toHaveBeenCalled();
  });

  it('should throw ApiEmptyResultError for 400 with string body', () => {
    const toastSpy = vi.spyOn(notificationService, 'error');

    httpClient.get('/api/test').subscribe({
      error: (err: unknown) => {
        expect(err).toBeInstanceOf(ApiEmptyResultError);
        expect((err as ApiEmptyResultError).message).toBe('No results');
      },
    });

    httpTesting.expectOne('/api/test').flush('No results', { status: 400, statusText: 'Bad Request' });
    expect(toastSpy).not.toHaveBeenCalled();
  });

  it('should show warning toast and trigger login for 401 when not authenticated', () => {
    const warnSpy = vi.spyOn(notificationService, 'warning');

    httpClient.get('/api/test').subscribe({
      error: () => {
        /* expected */
      },
    });

    httpTesting.expectOne('/api/test').flush('', { status: 401, statusText: 'Unauthorized' });

    expect(warnSpy).toHaveBeenCalledWith('Session expired. Redirecting to sign in...');
    expect(authService.login).toHaveBeenCalled();
  });

  it('should show warning toast but NOT trigger login for 401 when already authenticated', () => {
    (authService.isAuthenticated as WritableSignal<boolean>).set(true);
    const warnSpy = vi.spyOn(notificationService, 'warning');

    httpClient.get('/api/test').subscribe({
      error: () => {
        /* expected */
      },
    });

    httpTesting.expectOne('/api/test').flush('', { status: 401, statusText: 'Unauthorized' });

    expect(warnSpy).toHaveBeenCalled();
    expect(authService.login).not.toHaveBeenCalled();
  });

  it('should show error toast for 403', () => {
    const errorSpy = vi.spyOn(notificationService, 'error');

    httpClient.get('/api/test').subscribe({
      error: () => {
        /* expected */
      },
    });

    httpTesting.expectOne('/api/test').flush('', { status: 403, statusText: 'Forbidden' });

    expect(errorSpy).toHaveBeenCalledWith('You do not have permission to perform this action.');
  });

  it('should re-throw 404 without toast', () => {
    const errorSpy = vi.spyOn(notificationService, 'error');
    const warnSpy = vi.spyOn(notificationService, 'warning');

    httpClient.get('/api/test').subscribe({
      error: (err: unknown) => {
        expect(err).toBeInstanceOf(HttpErrorResponse);
        expect((err as HttpErrorResponse).status).toBe(404);
      },
    });

    httpTesting.expectOne('/api/test').flush('', { status: 404, statusText: 'Not Found' });

    expect(errorSpy).not.toHaveBeenCalled();
    expect(warnSpy).not.toHaveBeenCalled();
  });

  it('should track to App Insights and show toast for 500', () => {
    const errorSpy = vi.spyOn(notificationService, 'error');

    httpClient.get('/api/test').subscribe({
      error: () => {
        /* expected */
      },
    });

    httpTesting.expectOne('/api/test').flush('', { status: 500, statusText: 'Internal Server Error' });

    expect(errorSpy).toHaveBeenCalledWith('An unexpected error occurred. Please try again later.');
    expect(appInsightsService.trackException).toHaveBeenCalled();
    expect(appInsightsService.trackEvent).toHaveBeenCalledWith('HttpError', expect.objectContaining({ status: '500' }));
  });

  it('should show network error toast for status 0', () => {
    const errorSpy = vi.spyOn(notificationService, 'error');

    httpClient.get('/api/test').subscribe({
      error: () => {
        /* expected */
      },
    });

    httpTesting.expectOne('/api/test').error(new ProgressEvent('error'));

    expect(errorSpy).toHaveBeenCalledWith('Unable to reach the server. Please check your connection and try again.');
    expect(appInsightsService.trackEvent).toHaveBeenCalledWith('HttpError', expect.objectContaining({ status: '0' }));
  });

  it('should track all non-400 errors to App Insights with correlationId', () => {
    httpClient.get('/api/test').subscribe({
      error: () => {
        /* expected */
      },
    });

    httpTesting.expectOne('/api/test').flush('', { status: 403, statusText: 'Forbidden' });

    expect(appInsightsService.trackEvent).toHaveBeenCalledWith(
      'HttpError',
      expect.objectContaining({
        status: '403',
        correlationId: expect.any(String),
      }),
    );
  });
});
