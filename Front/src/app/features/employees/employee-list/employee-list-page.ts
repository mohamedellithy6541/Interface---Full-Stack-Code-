import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { EmployeeDto, EmployeeStatus } from '../../../core/api/generated/models';
import { EMPLOYEE_STATUS } from '../../../core/api/generated/models/employee-status-array';
import { toApiError } from '../../../core/http/api-error';
import { NotificationService } from '../../../core/notifications/notification.service';
import { ConfirmDialog } from '../../../shared/ui/confirm-dialog';
import { Pagination } from '../../../shared/ui/pagination';
import { EmployeeListStore } from '../data-access/employee-list.store';

@Component({
  selector: 'app-employee-list-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, DatePipe, DecimalPipe, Pagination, ConfirmDialog],
  providers: [EmployeeListStore],
  templateUrl: './employee-list-page.html',
  styleUrl: './employee-list-page.scss',
})
export class EmployeeListPage {
  protected readonly store = inject(EmployeeListStore);
  private readonly notifications = inject(NotificationService);

  protected readonly statuses = EMPLOYEE_STATUS;
  protected readonly pendingDelete = signal<EmployeeDto | null>(null);
  protected readonly deleting = signal(false);

  protected onSearch(event: Event): void {
    this.store.search((event.target as HTMLInputElement).value);
  }

  protected onDepartmentChange(event: Event): void {
    this.store.filterByDepartment((event.target as HTMLSelectElement).value);
  }

  protected onStatusChange(event: Event): void {
    this.store.filterByStatus((event.target as HTMLSelectElement).value as EmployeeStatus | '');
  }

  protected confirmDelete(): void {
    const employee = this.pendingDelete();
    if (!employee) return;

    this.deleting.set(true);
    this.store.delete(employee).subscribe({
      next: () => {
        this.notifications.success(`${employee.fullName} was deleted.`);
        this.closeDeleteDialog();
      },
      error: (error: unknown) => {
        this.notifications.error(toApiError(error).message);
        this.closeDeleteDialog();
      },
    });
  }

  protected closeDeleteDialog(): void {
    this.deleting.set(false);
    this.pendingDelete.set(null);
  }
}
