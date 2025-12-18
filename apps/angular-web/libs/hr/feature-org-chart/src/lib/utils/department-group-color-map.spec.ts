import { resolveGroupColorClass } from './department-group-color-map';

describe('resolveGroupColorClass', () => {
  it('maps each of the 6 known group names to its DaisyUI class', () => {
    expect(resolveGroupColorClass('Executive General and Administration')).toBe('primary');
    expect(resolveGroupColorClass('Sales and Marketing')).toBe('accent');
    expect(resolveGroupColorClass('Research and Development')).toBe('info');
    expect(resolveGroupColorClass('Manufacturing')).toBe('warning');
    expect(resolveGroupColorClass('Inventory Management')).toBe('secondary');
    expect(resolveGroupColorClass('Quality Assurance')).toBe('success');
  });

  it('falls back to neutral for an unmapped group name', () => {
    expect(resolveGroupColorClass('Some Unknown Group')).toBe('neutral');
  });

  it('falls back to neutral for null, undefined, or empty string', () => {
    expect(resolveGroupColorClass(null)).toBe('neutral');
    expect(resolveGroupColorClass(undefined)).toBe('neutral');
    expect(resolveGroupColorClass('')).toBe('neutral');
  });
});
