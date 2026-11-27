import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, map } from 'rxjs';

import { AuthResponse, CurrentUserDto } from '../api/generated/models';
import { AuthService as AuthApi } from '../api/generated/services';

export const HR_ROLE = 'HR';
const STORAGE_KEY = 'interface.session';

interface Session {
  accessToken: string;
  expiresAtUtc: string;
  user: CurrentUserDto;
}

/** Holds the signed-in user and their JWT, and keeps them in localStorage across page reloads. */
@Injectable({ providedIn: 'root' })
export class SessionService {
  private readonly authApi = inject(AuthApi);
  private readonly router = inject(Router);
  private readonly session = signal<Session | null>(readStoredSession());

  readonly currentUser = computed(() => this.session()?.user ?? null);
  readonly isHr = computed(() => this.currentUser()?.role === HR_ROLE);

  /** The bearer token, or null when there is no session or it has expired. */
  get accessToken(): string | null {
    return this.hasValidSession() ? this.session()!.accessToken : null;
  }

  hasValidSession(): boolean {
    const session = this.session();
    if (!session) return false;
    if (Date.parse(session.expiresAtUtc) <= Date.now()) {
      this.clear();
      return false;
    }
    return true;
  }

  login(email: string, password: string): Observable<CurrentUserDto> {
    return this.authApi.login({ body: { email, password } }).pipe(map((response) => this.start(response)));
  }

  loginWithGoogle(idToken: string): Observable<CurrentUserDto> {
    return this.authApi.googleLogin({ body: { idToken } }).pipe(map((response) => this.start(response)));
  }

  logout(): void {
    this.endSession();
    void this.router.navigateByUrl('/login');
  }

  /** Forgets the token without navigating (used when the API rejects it). */
  endSession(): void {
    this.clear();
  }

  /** Where a user lands after signing in: HR sees the employee list, an employee their own payslip. */
  homeUrl(): string {
    const user = this.currentUser();
    if (!user || !this.hasValidSession()) return '/login';
    if (user.role === HR_ROLE) return '/employees';
    return user.employeeId ? `/employees/${user.employeeId}/payslip` : '/login';
  }

  canViewPayslipOf(employeeId: string | null): boolean {
    return this.isHr() || (!!employeeId && this.currentUser()?.employeeId === employeeId);
  }

  private start(response: AuthResponse): CurrentUserDto {
    const session: Session = {
      accessToken: response.accessToken,
      expiresAtUtc: response.expiresAtUtc,
      user: response.user,
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    this.session.set(session);
    return response.user;
  }

  private clear(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.session.set(null);
  }
}

function readStoredSession(): Session | null {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored ? (JSON.parse(stored) as Session) : null;
  } catch {
    return null;
  }
}
