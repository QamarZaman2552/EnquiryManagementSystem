import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth';
import { catchError, switchMap, throwError } from 'rxjs';

let isRefreshing = false;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  const cloned = req.clone({ withCredentials: true });

  return next(cloned).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && !isRefreshing && !req.url.includes('/Auth/refresh')) {
        isRefreshing = true;
        return auth.refreshToken().pipe(
          switchMap(() => {
            isRefreshing = false;
            return next(cloned);
          }),
          catchError((refreshErr) => {
            isRefreshing = false;
            auth.clearSession();
            return throwError(() => refreshErr);
          })
        );
      }
      return throwError(() => err);
    })
  );
};
