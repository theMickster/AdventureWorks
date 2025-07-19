import { HttpInterceptorFn } from '@angular/common/http';

/** Adds a unique X-Correlation-ID header to every outgoing HTTP request for end-to-end tracing. */
export const correlationIdInterceptor: HttpInterceptorFn = (req, next) => {
  const correlationId = crypto.randomUUID();
  const cloned = req.clone({
    setHeaders: { 'X-Correlation-ID': correlationId },
  });
  return next(cloned);
};
