import { describe, it, expect } from 'vitest';
import { FormControl } from '@angular/forms';
import { maxFutureDateValidator } from './max-future-date.validator';

function daysFromToday(days: number): string {
  const date = new Date();
  date.setDate(date.getDate() + days);
  return date.toISOString().slice(0, 10);
}

describe('maxFutureDateValidator', () => {
  it('returns null for an empty value', () => {
    const ctrl = new FormControl('');
    expect(maxFutureDateValidator(30)(ctrl)).toBeNull();
  });

  it('returns null for today', () => {
    const ctrl = new FormControl(daysFromToday(0));
    expect(maxFutureDateValidator(30)(ctrl)).toBeNull();
  });

  it('returns null for a date exactly at the boundary', () => {
    const ctrl = new FormControl(daysFromToday(30));
    expect(maxFutureDateValidator(30)(ctrl)).toBeNull();
  });

  it('returns { maxFutureDate: true } for a date beyond the boundary', () => {
    const ctrl = new FormControl(daysFromToday(31));
    expect(maxFutureDateValidator(30)(ctrl)).toEqual({ maxFutureDate: true });
  });

  it('supports a different window size', () => {
    const ctrl = new FormControl(daysFromToday(91));
    expect(maxFutureDateValidator(90)(ctrl)).toEqual({ maxFutureDate: true });
  });
});
