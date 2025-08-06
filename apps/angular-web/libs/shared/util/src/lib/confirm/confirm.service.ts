import { Injectable, signal } from '@angular/core';

/** Options passed to ConfirmService.confirm() to configure the dialog title, message, and button labels. */
export interface ConfirmOptions {
  readonly title: string;
  readonly message: string;
  readonly confirmLabel?: string;
  readonly cancelLabel?: string;
  readonly variant?: 'primary' | 'error';
}

export interface ConfirmState {
  readonly visible: boolean;
  readonly options: ConfirmOptions;
}

@Injectable({ providedIn: 'root' })
/**
 * Signal-based confirmation dialog service. Call confirm() to show a modal and await the user's choice.
 * Rendered by ConfirmDialogComponent in AppLayoutComponent (same pattern as NotificationService + ToastContainer).
 */
export class ConfirmService {
  private resolver: ((value: boolean) => void) | null = null;

  readonly state = signal<ConfirmState>({
    visible: false,
    options: { title: '', message: '' },
  });

  confirm(options: ConfirmOptions): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      this.resolver = resolve;
      this.state.set({ visible: true, options });
    });
  }

  resolve(value: boolean): void {
    this.state.set({ visible: false, options: { title: '', message: '' } });
    if (this.resolver) {
      this.resolver(value);
      this.resolver = null;
    }
  }
}
