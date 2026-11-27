import { Injectable, signal } from '@angular/core';

export type NotificationKind = 'success' | 'error';

export interface Notification {
  id: number;
  kind: NotificationKind;
  message: string;
}

const DISPLAY_MILLISECONDS = 4000;

/** Short-lived toast messages ("Employee saved", "Session expired"). */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private nextId = 1;
  private readonly items = signal<Notification[]>([]);

  readonly notifications = this.items.asReadonly();

  success(message: string): void {
    this.show('success', message);
  }

  error(message: string): void {
    this.show('error', message);
  }

  dismiss(id: number): void {
    this.items.update((items) => items.filter((item) => item.id !== id));
  }

  private show(kind: NotificationKind, message: string): void {
    const id = this.nextId++;
    this.items.update((items) => [...items, { id, kind, message }]);
    setTimeout(() => this.dismiss(id), DISPLAY_MILLISECONDS);
  }
}
