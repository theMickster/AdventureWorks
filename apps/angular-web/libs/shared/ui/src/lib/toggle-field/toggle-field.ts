import { ChangeDetectionStrategy, Component, forwardRef, input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { createControlValueAccessor } from '@adventureworks-web/shared/util';

@Component({
  selector: 'aw-toggle-field',
  imports: [TranslatePipe],
  templateUrl: './toggle-field.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ToggleFieldComponent),
      multi: true,
    },
  ],
})
/** Boolean toggle switch with Reactive Forms integration (CVA) and DaisyUI styling. */
export class ToggleFieldComponent implements ControlValueAccessor {
  readonly fieldId = input('aw-toggle-field');
  readonly label = input('');

  protected readonly cva = createControlValueAccessor<boolean>(false);

  writeValue(val: boolean): void {
    this.cva.writeValue(val ?? false);
  }

  registerOnChange(fn: (val: boolean) => void): void {
    this.cva.registerOnChange(fn);
  }

  registerOnTouched(fn: () => void): void {
    this.cva.registerOnTouched(fn);
  }

  setDisabledState(isDisabled: boolean): void {
    this.cva.setDisabledState(isDisabled);
  }

  protected onToggle(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.cva.emitChange(target.checked);
  }
}
