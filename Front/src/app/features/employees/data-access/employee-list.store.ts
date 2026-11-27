import { Injectable, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { Observable, catchError, debounceTime, of, switchMap, tap } from 'rxjs';

import { EmployeeDto, EmployeeDtoPagedResult, EmployeeStatus } from '../../../core/api/generated/models';
import { EmployeesService } from '../../../core/api/generated/services';
import { toApiError } from '../../../core/http/api-error';

export interface EmployeeFilters {
  search: string;
  department: string;
  status: EmployeeStatus | '';
  page: number;
  pageSize: number;
}

const DEFAULT_FILTERS: EmployeeFilters = { search: '', department: '', status: '', page: 1, pageSize: 10 };
const FILTER_DEBOUNCE_MS = 250;

/**
 * State and data loading for the employee list. Every filter change reloads the page;
 * switchMap cancels a request that is still running when the user keeps typing.
 */
@Injectable()
export class EmployeeListStore {
  private readonly api = inject(EmployeesService);

  readonly filters = signal<EmployeeFilters>(DEFAULT_FILTERS);
  readonly result = signal<EmployeeDtoPagedResult | null>(null);
  readonly departments = signal<string[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  constructor() {
    toObservable(this.filters)
      .pipe(
        debounceTime(FILTER_DEBOUNCE_MS),
        tap(() => {
          this.loading.set(true);
          this.error.set(null);
        }),
        switchMap((filters) =>
          this.api.getEmployees(toQuery(filters)).pipe(
            catchError((error: unknown) => {
              this.error.set(toApiError(error).message);
              return of(null);
            }),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((result) => {
        if (result) this.result.set(result);
        this.loading.set(false);
      });

    this.loadDepartments();
  }

  search(term: string): void {
    this.filters.update((filters) => ({ ...filters, search: term, page: 1 }));
  }

  filterByDepartment(department: string): void {
    this.filters.update((filters) => ({ ...filters, department, page: 1 }));
  }

  filterByStatus(status: EmployeeStatus | ''): void {
    this.filters.update((filters) => ({ ...filters, status, page: 1 }));
  }

  goToPage(page: number): void {
    this.filters.update((filters) => ({ ...filters, page }));
  }

  resetFilters(): void {
    this.filters.set({ ...DEFAULT_FILTERS, pageSize: this.filters().pageSize });
  }

  delete(employee: EmployeeDto): Observable<void> {
    return this.api.deleteEmployee({ id: employee.id }).pipe(
      tap(() => {
        const isLastItemOnPage = this.result()?.items.length === 1;
        const page = this.filters().page;
        // A new object, even with the same values, makes the list reload.
        this.filters.update((filters) => ({ ...filters, page: isLastItemOnPage && page > 1 ? page - 1 : page }));
        this.loadDepartments();
      }),
    );
  }

  private loadDepartments(): void {
    this.api.getDepartments().subscribe({
      next: (departments) => this.departments.set(departments),
      error: () => this.departments.set([]),
    });
  }
}

function toQuery(filters: EmployeeFilters) {
  return {
    Search: filters.search.trim() || undefined,
    Department: filters.department || undefined,
    Status: filters.status || undefined,
    Page: filters.page,
    PageSize: filters.pageSize,
  };
}
