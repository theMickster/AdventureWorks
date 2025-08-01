import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService } from '../loading/loading.service';

/** Manages global loading state for HTTP requests. Requests with X-Silent-Request header bypass loading. */
export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);

  if (req.headers.has('X-Silent-Request')) {
    return next(req.clone({ headers: req.headers.delete('X-Silent-Request') }));
  }

  loadingService.start();
  return next(req).pipe(finalize(() => loadingService.stop()));
};
