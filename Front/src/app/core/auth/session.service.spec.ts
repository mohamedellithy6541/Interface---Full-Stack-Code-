import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { provideApiConfiguration } from '../api/generated/api-configuration';
import { AuthResponse } from '../api/generated/models';
import { SessionService } from './session.service';

const API = 'http://api.test';

function authResponse(role: 'HR' | 'Employee', expiresInMs = 60_000): AuthResponse {
  return {
    accessToken: 'jwt-token',
    tokenType: 'Bearer',
    expiresAtUtc: new Date(Date.now() + expiresInMs).toISOString(),
    user: {
      id: 'user-1',
      email: 'someone@interface.local',
      displayName: 'Someone',
      role,
      employeeId: role === 'Employee' ? 'employee-1' : null,
    },
  };
}

describe('SessionService', () => {
  let session: SessionService;
  let http: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([{ path: 'login', children: [] }]), provideApiConfiguration(API)],
    });
    session = TestBed.inject(SessionService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function signInAs(role: 'HR' | 'Employee', expiresInMs?: number) {
    session.login('someone@interface.local', 'secret').subscribe();
    http.expectOne(`${API}/api/auth/login`).flush(authResponse(role, expiresInMs));
  }

  it('stores the token and user after a successful login', () => {
    signInAs('HR');

    expect(session.accessToken).toBe('jwt-token');
    expect(session.isHr()).toBe(true);
    expect(localStorage.getItem('interface.session')).toContain('jwt-token');
  });

  it('sends HR to the employee list and an employee to their own payslip', () => {
    signInAs('HR');
    expect(session.homeUrl()).toBe('/employees');

    signInAs('Employee');
    expect(session.homeUrl()).toBe('/employees/employee-1/payslip');
  });

  it('lets an employee view only their own payslip', () => {
    signInAs('Employee');

    expect(session.canViewPayslipOf('employee-1')).toBe(true);
    expect(session.canViewPayslipOf('employee-2')).toBe(false);
  });

  it('treats an expired token as signed out', () => {
    signInAs('HR', -1_000);

    expect(session.hasValidSession()).toBe(false);
    expect(session.accessToken).toBeNull();
    expect(localStorage.getItem('interface.session')).toBeNull();
  });

  it('forgets everything on logout', () => {
    signInAs('HR');

    session.logout();

    expect(session.currentUser()).toBeNull();
    expect(localStorage.getItem('interface.session')).toBeNull();
  });
});
