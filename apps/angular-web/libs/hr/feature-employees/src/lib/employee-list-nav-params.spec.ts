import { describe, expect, it } from 'vitest';
import { extractEmployeeListNavParams } from './employee-list-nav-params';

describe('extractEmployeeListNavParams', () => {
  it('returns all 3 keys when all params are present', () => {
    const params = { name: 'Terri Duffy', status: 'active', pageNumber: '2' };

    const result = extractEmployeeListNavParams(params);

    expect(result).toEqual({ name: 'Terri Duffy', status: 'active', pageNumber: '2' });
  });

  it('omits a key that is absent from params', () => {
    const params = { status: 'terminated', pageNumber: '3' };

    const result = extractEmployeeListNavParams(params);

    expect(result).not.toHaveProperty('name');
    expect(Object.keys(result)).toHaveLength(2);
  });

  it('returns an empty object for empty input', () => {
    expect(extractEmployeeListNavParams({})).toEqual({});
  });

  it('does not include unrelated params', () => {
    const params = { search: 'acme', foo: 'bar', name: 'Terri' };

    const result = extractEmployeeListNavParams(params);

    expect(result).not.toHaveProperty('search');
    expect(result).not.toHaveProperty('foo');
    expect(result).toEqual({ name: 'Terri' });
  });

  it('preserves string type for all values', () => {
    const params = { pageNumber: '2' };

    const result = extractEmployeeListNavParams(params);

    expect(typeof result['pageNumber']).toBe('string');
    expect(result['pageNumber']).toBe('2');
  });
});
