import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { SessionService } from './session.service';

// These guards only decide what the UI shows. The API enforces the same rules on every request.

export const authGuard: CanActivateFn = (_route, state) => {
  const session = inject(SessionService);
  return session.hasValidSession()
    ? true
    : inject(Router).createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

export const guestGuard: CanActivateFn = () => {
  const session = inject(SessionService);
  return session.hasValidSession() ? inject(Router).parseUrl(session.homeUrl()) : true;
};

export const hrGuard: CanActivateFn = () => {
  const session = inject(SessionService);
  return session.isHr() ? true : inject(Router).parseUrl(session.homeUrl());
};

export const ownPayslipGuard: CanActivateFn = (route) => {
  const session = inject(SessionService);
  return session.canViewPayslipOf(route.paramMap.get('id'))
    ? true
    : inject(Router).parseUrl(session.homeUrl());
};
