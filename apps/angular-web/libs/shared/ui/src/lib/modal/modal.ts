import { ChangeDetectionStrategy, Component, effect, ElementRef, input, model, output, viewChild } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'aw-modal',
  imports: [TranslatePipe],
  templateUrl: './modal.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/** Reusable modal using native `<dialog>` element with DaisyUI styling. Supports two-way `open` binding via model(). */
export class ModalComponent {
  readonly fieldId = input('aw-modal');
  readonly title = input('');
  readonly open = model(false);
  readonly closed = output<void>();

  private readonly dialogRef = viewChild<ElementRef<HTMLDialogElement>>('dialogEl');

  constructor() {
    effect(() => {
      const dialog = this.dialogRef()?.nativeElement;
      if (!dialog) return;
      if (this.open() && !dialog.open) {
        dialog.showModal();
      } else if (!this.open() && dialog.open) {
        dialog.close();
      }
    });
  }

  protected onDialogClose(): void {
    this.open.set(false);
    this.closed.emit();
  }

  protected onBackdropClick(event: MouseEvent): void {
    const dialog = this.dialogRef()?.nativeElement;
    if (event.target === dialog) {
      this.open.set(false);
      this.closed.emit();
    }
  }
}
