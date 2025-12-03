import { describe, it, expect } from 'vitest';
import { FormControl } from '@angular/forms';
import { minAgeValidator } from './min-age.validator';

describe('minAgeValidator', () => {
  it('returns null for an empty value', () => {
    const ctrl = new FormControl('');
    expect(minAgeValidator(18)(ctrl)).toBeNull();
  });

  it('returns null for a date exactly at the minimum age', () => {
    const cutoff = new Date();
    cutoff.setFullYear(cutoff.getFullYear() - 18);
    const ctrl = new FormControl(cutoff.toISOString().slice(0, 10));
    expect(minAgeValidator(18)(ctrl)).toBeNull();
  });

  it('returns null for a date older than the minimum age', () => {
    const ctrl = new FormControl('1985-03-15');
    expect(minAgeValidator(18)(ctrl)).toBeNull();
  });

  it('returns { minAge: true } for a date younger than the minimum age', () => {
    const tooYoung = new Date();
    tooYoung.setFullYear(tooYoung.getFullYear() - 17);
    const ctrl = new FormControl(tooYoung.toISOString().slice(0, 10));
    expect(minAgeValidator(18)(ctrl)).toEqual({ minAge: true });
  });

  it('supports a different minimum age', () => {
    const tooYoung = new Date();
    tooYoung.setFullYear(tooYoung.getFullYear() - 20);
    const ctrl = new FormControl(tooYoung.toISOString().slice(0, 10));
    expect(minAgeValidator(21)(ctrl)).toEqual({ minAge: true });
  });
});
