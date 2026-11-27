import { ChangeDetectionStrategy, Component, ElementRef, effect, input, output, viewChild } from '@angular/core';

/** A modal yes/no question built on the native <dialog> element (focus trapping and Esc for free). */
@Component({
  selector: 'app-confirm-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <dialog #dialog class="dialog" (cancel)="cancelled.emit()" (close)="onClose()">
      <h2 class="dialog__title">{{ title() }}</h2>
      <p class="dialog__message">{{ message() }}</p>
      <div class="dialog__actions">
        <button type="button" class="btn btn--ghost" (click)="cancelled.emit()">Cancel</button>
        <button type="button" class="btn btn--danger" [disabled]="busy()" (click)="confirmed.emit()">
          {{ busy() ? 'Working…' : confirmLabel() }}
        </button>
      </div>
    </dialog>
  `,
  styles: `
    .dialog { width: min(420px, calc(100vw - 2rem)); border: 0; border-radius: var(--radius-lg); padding: var(--space-6); box-shadow: var(--shadow-lg); }
    .dialog::backdrop { background: rgb(15 23 42 / 45%); }
    .dialog__title { margin: 0 0 var(--space-2); font-size: 1.125rem; }
    .dialog__message { margin: 0 0 var(--space-6); color: var(--color-muted); }
    .dialog__actions { display: flex; justify-content: flex-end; gap: var(--space-2); }
  `,
})
export class ConfirmDialog {
  readonly open = input(false);
  readonly title = input('Are you sure?');
  readonly message = input('');
  readonly confirmLabel = input('Confirm');
  readonly busy = input(false);
  readonly confirmed = output();
  readonly cancelled = output();

  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  constructor() {
    effect(() => {
      const dialog = this.dialog().nativeElement;
      if (this.open() && !dialog.open) dialog.showModal();
      if (!this.open() && dialog.open) dialog.close();
    });
  }

  protected onClose(): void {
    if (this.open()) this.cancelled.emit();
  }
}
