import { signal, WritableSignal } from '@angular/core';

/** Signal-backed bridge returned by createControlValueAccessor. Delegates CVA lifecycle to a simple object. */
export interface ControlValueAccessorBridge<T> {
  readonly value: WritableSignal<T>;
  readonly disabled: WritableSignal<boolean>;
  writeValue(val: T): void;
  registerOnChange(fn: (val: T) => void): void;
  registerOnTouched(fn: () => void): void;
  setDisabledState(isDisabled: boolean): void;
  emitChange(val: T): void;
  markAsTouched(): void;
}

/** Factory that returns a signal-backed CVA bridge. Form components delegate their ControlValueAccessor methods to this. */
export function createControlValueAccessor<T>(defaultValue: T): ControlValueAccessorBridge<T> {
  const value = signal<T>(defaultValue);
  const disabled = signal(false);
  let onChange: (val: T) => void = () => {
    /* no-op until registered */
  };
  let onTouched: () => void = () => {
    /* no-op until registered */
  };

  return {
    value,
    disabled,
    writeValue: (val: T) => value.set(val),
    registerOnChange: (fn: (val: T) => void) => {
      onChange = fn;
    },
    registerOnTouched: (fn: () => void) => {
      onTouched = fn;
    },
    setDisabledState: (isDisabled: boolean) => disabled.set(isDisabled),
    emitChange: (val: T) => {
      value.set(val);
      onChange(val);
    },
    markAsTouched: () => onTouched(),
  };
}
