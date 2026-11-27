import { inject } from '@angular/core';
import { Routes } from '@angular/router';

import { authGuard, guestGuard, hrGuard, ownPayslipGuard } from './core/auth/auth.guards';
import { SessionService } from './core/auth/session.service';
import { Shell } from './core/layout/shell';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    title: 'Sign in · Interface Payroll',
    loadComponent: () => import('./features/auth/login-page').then((m) => m.LoginPage),
  },
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    children: [
      {
        path: 'employees',
        canActivate: [hrGuard],
        title: 'Employees · Interface Payroll',
        loadComponent: () =>
          import('./features/employees/employee-list/employee-list-page').then((m) => m.EmployeeListPage),
      },
      {
        path: 'employees/new',
        canActivate: [hrGuard],
        title: 'Add employee · Interface Payroll',
        loadComponent: () =>
          import('./features/employees/employee-form/employee-form-page').then((m) => m.EmployeeFormPage),
      },
      {
        path: 'employees/:id/edit',
        canActivate: [hrGuard],
        title: 'Edit employee · Interface Payroll',
        loadComponent: () =>
          import('./features/employees/employee-form/employee-form-page').then((m) => m.EmployeeFormPage),
      },
      {
        path: 'employees/:id/payslip',
        canActivate: [ownPayslipGuard],
        title: 'Payslip · Interface Payroll',
        loadComponent: () => import('./features/payslip/payslip-page').then((m) => m.PayslipPage),
      },
      { path: '', pathMatch: 'full', redirectTo: () => inject(SessionService).homeUrl() },
    ],
  },
  { path: '**', redirectTo: '' },
];
