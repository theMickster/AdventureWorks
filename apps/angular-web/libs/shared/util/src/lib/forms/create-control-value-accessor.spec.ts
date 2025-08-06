import { describe, it, expect, vi } from 'vitest';
import { createControlValueAccessor } from './create-control-value-accessor';

describe('createControlValueAccessor', () => {
  it('should set the default value on creation', () => {
    const cva = createControlValueAccessor('hello');
    expect(cva.value()).toBe('hello');
  });

  it('should set disabled to false by default', () => {
    const cva = createControlValueAccessor('');
    expect(cva.disabled()).toBe(false);
  });

  it('writeValue should set the value signal', () => {
    const cva = createControlValueAccessor('');
    cva.writeValue('updated');
    expect(cva.value()).toBe('updated');
  });

  it('registerOnChange + emitChange should call the registered onChange function', () => {
    const cva = createControlValueAccessor('');
    const onChangeSpy = vi.fn();
    cva.registerOnChange(onChangeSpy);

    cva.emitChange('new-value');

    expect(onChangeSpy).toHaveBeenCalledWith('new-value');
  });

  it('registerOnTouched + markAsTouched should call the registered onTouched function', () => {
    const cva = createControlValueAccessor('');
    const onTouchedSpy = vi.fn();
    cva.registerOnTouched(onTouchedSpy);

    cva.markAsTouched();

    expect(onTouchedSpy).toHaveBeenCalledOnce();
  });

  it('setDisabledState should set the disabled signal', () => {
    const cva = createControlValueAccessor('');
    cva.setDisabledState(true);
    expect(cva.disabled()).toBe(true);

    cva.setDisabledState(false);
    expect(cva.disabled()).toBe(false);
  });

  it('emitChange should both update the value signal and call onChange', () => {
    const cva = createControlValueAccessor(0);
    const onChangeSpy = vi.fn();
    cva.registerOnChange(onChangeSpy);

    cva.emitChange(42);

    expect(cva.value()).toBe(42);
    expect(onChangeSpy).toHaveBeenCalledWith(42);
  });

  it('emitChange should not throw when no onChange is registered', () => {
    const cva = createControlValueAccessor('');
    expect(() => cva.emitChange('value')).not.toThrow();
    expect(cva.value()).toBe('value');
  });

  it('markAsTouched should not throw when no onTouched is registered', () => {
    const cva = createControlValueAccessor('');
    expect(() => cva.markAsTouched()).not.toThrow();
  });
});
