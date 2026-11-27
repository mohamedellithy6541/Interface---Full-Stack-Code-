import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { SessionService } from '../auth/session.service';
import { NotificationService } from '../notifications/notification.service';

/**
 * Global HTTP error handling: an expired or rejected token sends the user back to the login page.
 * Every other error is passed on, so the page that made the request can show it in context.
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const session = inject(SessionService);
  const router = inject(Router);
  const notifications = inject(NotificationService);

  return next(request).pipe(
    catchError((error: unknown) => {
      const isLoginRequest = request.url.includes('/api/auth/login') || request.url.includes('/api/auth/google');

      if (error instanceof HttpErrorResponse && error.status === 401 && !isLoginRequest) {
        session.endSession();
        notifications.error('Your session has expired. Please sign in again.');
        void router.navigate(['/login'], { queryParams: { returnUrl: router.url } });
      }

      return throwError(() => error);
    }),
  );
};
