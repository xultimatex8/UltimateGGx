import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { IS_STARTUP_REQUEST } from '../../shared/http/startup-http-context';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const isStartupRequest = req.context.get(IS_STARTUP_REQUEST);

  return next(req).pipe(
    catchError(error => {
      if (error.status >= 500 && !isStartupRequest) {
        router.navigate(['/500']);
      }

      return throwError(() => error);
    })
  );
};