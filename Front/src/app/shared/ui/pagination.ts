import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <nav class="pagination" aria-label="Pagination">
      <span class="pagination__summary">
        @if (totalCount() > 0) {
          Showing {{ firstItem() }}–{{ lastItem() }} of {{ totalCount() }}
        } @else {
          No results
        }
      </span>
      <div class="pagination__buttons">
        <button type="button" class="btn btn--ghost" [disabled]="page() <= 1" (click)="pageChange.emit(page() - 1)">
          Previous
        </button>
        <span class="pagination__page">Page {{ page() }} of {{ totalPages() || 1 }}</span>
        <button type="button" class="btn btn--ghost" [disabled]="page() >= totalPages()" (click)="pageChange.emit(page() + 1)">
          Next
        </button>
      </div>
    </nav>
  `,
  styles: `
    .pagination { display: flex; align-items: center; justify-content: space-between; gap: var(--space-3); flex-wrap: wrap; }
    .pagination__summary, .pagination__page { color: var(--color-muted); font-size: 0.875rem; }
    .pagination__buttons { display: flex; align-items: center; gap: var(--space-2); }
  `,
})
export class Pagination {
  readonly page = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly totalCount = input.required<number>();
  readonly totalPages = input.required<number>();
  readonly pageChange = output<number>();

  protected readonly firstItem = computed(() => (this.page() - 1) * this.pageSize() + 1);
  protected readonly lastItem = computed(() => Math.min(this.page() * this.pageSize(), this.totalCount()));
}
