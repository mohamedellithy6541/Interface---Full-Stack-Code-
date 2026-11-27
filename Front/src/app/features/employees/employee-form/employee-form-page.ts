import { Component, DestroyRef, OnInit, computed, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { EMPLOYEE_STATUS } from '../../../core/api/generated/models/employee-status-array';
import { EmployeesService } from '../../../core/api/generated/services';
import { ApiErrorCodes } from '../../../core/http/api-error-codes';
import { toApiError } from '../../../core/http/api-error';
import { NotificationService } from '../../../core/notifications/notification.service';
import { FieldError } from '../../../shared/forms/field-error';
import { applyServerErrors } from '../../../shared/forms/server-errors';
import { todayIsoDate } from '../../../shared/forms/validators';
import { EmployeeLimits, createEmployeeForm, fillEmployeeForm, toEmployeeRequest } from './employee-form.model';
import { PayComponentsEditor } from './pay-components-editor';

/** Create (/employees/new) and edit (/employees/:id/edit) share this page. */
@Component({
  selector: 'app-employee-form-page',
  imports: [ReactiveFormsModule, RouterLink, FieldError, PayComponentsEditor],
  templateUrl: './employee-form-page.html',
  styleUrl: './employee-form-page.scss',
})
export class EmployeeFormPage implements OnInit {
  /** Route parameter; missing when creating. */
  readonly id = input<string>();

  private readonly api = inject(EmployeesService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly form = createEmployeeForm();
  protected readonly limits = EmployeeLimits;
  protected readonly statuses = EMPLOYEE_STATUS;
  protected readonly today = todayIsoDate();

  protected readonly isEdit = computed(() => !!this.id());
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saveErrors = signal<string[]>([]);
  protected readonly departments = signal<string[]>([]);

  ngOnInit(): void {
    this.api.getDepartments()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (departments) => this.departments.set(departments), error: () => undefined });

    const id = this.id();
    if (id) this.loadEmployee(id);
  }

  protected submit(): void {
    this.saveErrors.set([]);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.saveErrors.set(['Please fix the highlighted fields.']);
      return;
    }

    const id = this.id();
    const body = toEmployeeRequest(this.form.getRawValue());
    const save$ = id ? this.api.updateEmployee({ id, body }) : this.api.createEmployee({ body });

    this.saving.set(true);
    save$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (employee) => {
        this.notifications.success(`${employee.fullName} was ${id ? 'updated' : 'created'}.`);
        void this.router.navigateByUrl('/employees');
      },
      error: (error: unknown) => this.showSaveError(error),
    });
  }

  private loadEmployee(id: string): void {
    this.loading.set(true);
    this.api.getEmployee({ id })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (employee) => {
          fillEmployeeForm(this.form, employee);
          this.loading.set(false);
        },
        error: (error: unknown) => {
          this.loadError.set(toApiError(error).message);
          this.loading.set(false);
        },
      });
  }

  private showSaveError(error: unknown): void {
    const apiError = toApiError(error);
    const fieldErrors = apiError.code === ApiErrorCodes.emailTaken
      ? { email: [apiError.message] }
      : apiError.fieldErrors;

    const unmatched = applyServerErrors(this.form, fieldErrors);
    const hasFieldErrors = Object.keys(fieldErrors).length > 0;

    this.saveErrors.set(hasFieldErrors ? ['Please fix the highlighted fields.', ...unmatched] : [apiError.message]);
    this.saving.set(false);
  }
}
