import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { ConfirmService } from '@adventureworks-web/shared/util';
import { ModalComponent } from '../modal/modal';
import { ButtonComponent } from '../button/button';

@Component({
  selector: 'aw-confirm-dialog',
  imports: [ModalComponent, ButtonComponent, TranslatePipe],
  templateUrl: './confirm-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/** Global confirm dialog rendered in AppLayoutComponent. Reads state from ConfirmService and resolves on user action. */
export class ConfirmDialogComponent {
  private readonly confirmService = inject(ConfirmService);

  protected readonly visible = computed(() => this.confirmService.state().visible);
  protected readonly options = computed(() => this.confirmService.state().options);

  protected onConfirm(): void {
    this.confirmService.resolve(true);
  }

  protected onCancel(): void {
    this.confirmService.resolve(false);
  }
}
