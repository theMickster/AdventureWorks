import { ChangeDetectionStrategy, Component, computed, forwardRef, input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { createControlValueAccessor } from '@adventureworks-web/shared/util';

@Component({
  selector: 'aw-textarea-field',
  imports: [TranslatePipe],
  templateUrl: './textarea-field.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TextareaFieldComponent),
      multi: true,
    },
  ],
})
/** Multi-line textarea field with Reactive Forms integration (CVA), DaisyUI styling, label, hint, and error display. */
export class TextareaFieldComponent implements ControlValueAccessor {
  readonly fieldId = input('aw-textarea-field');
  readonly label = input('');
  readonly placeholder = input('');
  readonly hint = input('');
  readonly rows = input(3);
  readonly errors = input<Record<string, string> | null>(null);

  protected readonly cva = createControlValueAccessor<string>('');

  protected readonly textareaClass = computed(() => {
    const classes = ['textarea', 'textarea-bordered', 'w-full'];
    if (this.errors()) classes.push('textarea-error');
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
    const target = event.target as HTMLTextAreaElement;
    this.cva.emitChange(target.value);
  }

  protected onBlur(): void {
    this.cva.markAsTouched();
  }
}
