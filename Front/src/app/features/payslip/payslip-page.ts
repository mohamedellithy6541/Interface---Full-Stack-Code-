import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { catchError, filter, map, of, switchMap, tap } from 'rxjs';

import { PayslipDto } from '../../core/api/generated/models';
import { PayslipsService } from '../../core/api/generated/services';
import { SessionService } from '../../core/auth/session.service';
import { ApiError, toApiError } from '../../core/http/api-error';
import { currentIsoMonth } from '../../shared/forms/validators';

const MONTH_PATTERN = /^\d{4}-(0[1-9]|1[0-2])$/;

/** Shows one employee's payslip for a chosen month. The calculation itself happens on the server. */
@Component({
  selector: 'app-payslip-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, DecimalPipe],
  templateUrl: './payslip-page.html',
  styleUrl: './payslip-page.scss',
})
export class PayslipPage {
  /** Route parameter: the employee id. */
  readonly id = input.required<string>();

  private readonly api = inject(PayslipsService);
  protected readonly session = inject(SessionService);

  protected readonly month = signal(currentIsoMonth());
  protected readonly payslip = signal<PayslipDto | null>(null);
  protected readonly error = signal<ApiError | null>(null);
  protected readonly loading = signal(false);

  /** 422 means "valid request, but this payslip cannot be produced", which is a warning, not a failure. */
  protected readonly isBusinessRuleError = computed(() => this.error()?.status === 422);

  constructor() {
    toObservable(computed(() => ({ employeeId: this.id(), month: this.month() })))
      .pipe(
        filter((request) => MONTH_PATTERN.test(request.month)),
        tap(() => {
          this.loading.set(true);
          this.error.set(null);
        }),
        switchMap((request) =>
          this.api.getPayslip(request).pipe(
            map((payslip) => ({ payslip, error: null })),
            catchError((error: unknown) => of({ payslip: null, error: toApiError(error) })),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe(({ payslip, error }) => {
        this.payslip.set(payslip);
        this.error.set(error);
        this.loading.set(false);
      });
  }

  protected onMonthChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    if (value) this.month.set(value);
  }

  protected print(): void {
    window.print();
  }
}
