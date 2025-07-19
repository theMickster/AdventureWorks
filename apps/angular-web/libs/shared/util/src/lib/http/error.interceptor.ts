import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../notification/notification.service';

/** Centralized HTTP error interceptor. Handles 401, 403, 5xx with toast notifications. Passes 400 through for form-level handling. */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        notificationService.warning('Session expired. Please sign in again.');
        router.navigate(['/dashboard']);
      } else if (error.status === 403) {
        notificationService.error('You do not have permission to perform this action.');
      } else if (error.status >= 500) {
        const correlationId = req.headers.get('X-Correlation-ID') ?? 'unknown';
        console.error(`[${correlationId}] Server error ${error.status}:`, error.message);
        notificationService.error('An unexpected error occurred. Please try again later.');
      }

      return throwError(() => error);
    }),
  );
};
