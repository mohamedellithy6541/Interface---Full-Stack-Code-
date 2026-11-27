import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { ApiConfiguration } from '../api/generated/api-configuration';
import { SessionService } from './session.service';

/** Adds "Authorization: Bearer <token>" to requests that go to our API, never to other hosts. */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const token = inject(SessionService).accessToken;
  const apiRoot = inject(ApiConfiguration).rootUrl;

  if (!token || !request.url.startsWith(apiRoot)) {
    return next(request);
  }

  return next(request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
