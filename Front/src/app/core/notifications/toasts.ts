import { ChangeDetectionStrategy, Component, inject } from '@angular/core';

import { NotificationService } from './notification.service';

@Component({
  selector: 'app-toasts',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="toasts" aria-live="polite">
      @for (toast of notifications.notifications(); track toast.id) {
        <div class="toast" [class.toast--error]="toast.kind === 'error'" role="status">
          <span>{{ toast.message }}</span>
          <button type="button" class="toast__close" aria-label="Dismiss" (click)="notifications.dismiss(toast.id)">×</button>
        </div>
      }
    </div>
  `,
  styleUrl: './toasts.scss',
})
export class Toasts {
  protected readonly notifications = inject(NotificationService);
}
