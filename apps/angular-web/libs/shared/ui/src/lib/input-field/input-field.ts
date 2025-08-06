import { ChangeDetectionStrategy, Component, computed, forwardRef, input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { createControlValueAccessor } from '@adventureworks-web/shared/util';

@Component({
  selector: 'aw-input-field',
  imports: [TranslatePipe],
  templateUrl: './input-field.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputFieldComponent),
      multi: true,
    },
  ],
})
/** Text input field with Reactive Forms integration (CVA), DaisyUI styling, label, hint, and error display. */
export class InputFieldComponent implements ControlValueAccessor {
  readonly fieldId = input('aw-input-field');
  readonly label = input('');
  readonly placeholder = input('');
  readonly hint = input('');
  readonly inputType = input<'text' | 'email' | 'password' | 'number' | 'tel' | 'url'>('text');
  readonly errors = input<Record<string, string> | null>(null);

  protected readonly cva = createControlValueAccessor<string>('');

  protected readonly inputClass = computed(() => {
    const classes = ['input', 'input-bordered', 'w-full'];
    if (this.errors()) classes.push('input-error');
    return classes.join(' ');
  });

  protected readonly errorEntries = computed(() => {
    const errs = this.errors();
    return errs ? Object.entries(errs) : [];
  });

  protected readonly ariaDescribedBy = computed(() => {
    const id = this.fieldId();
    if (this.errors()) {
      return this.errorEntries()
        .map((e) => `${id}-error-${e[0]}`)
        .join(' ');
    }
    return this.hint() ? `${id}-hint` : null;
  });

  writeValue(val: string): void {
    this.cva.writeValue(val ?? '');
  }

  registerOnChange(fn: (val: string) => void): void {
    this.cva.registerOnChange(fn);
  }

  registerOnTouched(fn: () => void): void {
    this.cva.registerOnTouched(fn);
  }

  setDisabledState(isDisabled: boolean): void {
    this.cva.setDisabledState(isDisabled);
  }

  protected onInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.cva.emitChange(target.value);
  }

  protected onBlur(): void {
    this.cva.markAsTouched();
  }
}
