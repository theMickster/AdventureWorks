import { describe, it, expect } from 'vitest';
import { FormControl } from '@angular/forms';
import { minDateValidator } from './min-date.validator';

describe('minDateValidator', () => {
  it('returns null for an empty value', () => {
    const ctrl = new FormControl('');
    expect(minDateValidator(new Date())(ctrl)).toBeNull();
  });

  it('returns null for a date equal to minDate', () => {
    const minDate = new Date('2026-01-15');
    const ctrl = new FormControl('2026-01-15');
    expect(minDateValidator(minDate)(ctrl)).toBeNull();
  });

  it('returns null for a date after minDate', () => {
    const minDate = new Date('2026-01-15');
    const ctrl = new FormControl('2026-02-01');
    expect(minDateValidator(minDate)(ctrl)).toBeNull();
  });

  it('returns { minDate: true } for a date before minDate', () => {
    const minDate = new Date('2026-01-15');
    const ctrl = new FormControl('2026-01-14');
    expect(minDateValidator(minDate)(ctrl)).toEqual({ minDate: true });
  });
});
