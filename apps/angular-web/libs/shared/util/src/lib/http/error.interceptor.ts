import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { NotificationService } from '../notification/notification.service';
import { AppInsightsService } from '../telemetry/app-insights.service';
import { ApiEmptyResultError } from './errors/api-empty-result-error';
import { ApiValidationError } from './errors/api-validation-error';
import { ValidationError } from './errors/validation-error.model';

/** Centralized HTTP error interceptor. Handles all error statuses with structured parsing, toast notifications, and App Insights telemetry. */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);
  const authService = inject(AuthService);
  const appInsightsService = inject(AppInsightsService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const correlationId = req.headers.get('X-Correlation-ID') ?? 'unknown';

      if (error.status === 0) {
        notificationService.error('Unable to reach the server. Please check your connection and try again.');
        trackToAppInsights(appInsightsService, error, correlationId);
        return throwError(() => error);
      }

      if (error.status === 400) {
        return throwError(() => handle400(error, correlationId));
      }

      if (error.status === 401) {
        notificationService.warning('Session expired. Redirecting to sign in...');
        if (!authService.isAuthenticated()) {
          authService.login();
        }
        trackToAppInsights(appInsightsService, error, correlationId);
        return throwError(() => error);
      }

      if (error.status === 403) {
        notificationService.error('You do not have permission to perform this action.');
        trackToAppInsights(appInsightsService, error, correlationId);
        return throwError(() => error);
      }

      if (error.status === 404) {
        trackToAppInsights(appInsightsService, error, correlationId);
        return throwError(() => error);
      }

      if (error.status >= 500) {
        notificationService.error('An unexpected error occurred. Please try again later.');
        trackToAppInsights(appInsightsService, error, correlationId);
        appInsightsService.trackException(error);
        return throwError(() => error);
      }

      trackToAppInsights(appInsightsService, error, correlationId);
      return throwError(() => error);
    }),
  );
};

function handle400(error: HttpErrorResponse, correlationId: string): ApiValidationError | ApiEmptyResultError {
  if (Array.isArray(error.error)) {
    return new ApiValidationError(error.error as ValidationError[], correlationId);
  }
  return new ApiEmptyResultError(typeof error.error === 'string' ? error.error : 'Bad request');
}

function trackToAppInsights(service: AppInsightsService, error: HttpErrorResponse, correlationId: string): void {
  service.trackEvent('HttpError', {
    status: String(error.status),
    url: error.url ?? 'unknown',
    correlationId,
  });
}
