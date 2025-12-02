import { describe, expect, it } from 'vitest';
import { extractListNavParams } from './list-nav-params';

describe('extractListNavParams', () => {
  it('returns all four params when all are present', () => {
    const result = extractListNavParams({
      search: 'acme',
      pageNumber: '3',
      orderBy: 'name',
      sortOrder: 'desc',
    });

    expect(result).toEqual({
      search: 'acme',
      pageNumber: '3',
      orderBy: 'name',
      sortOrder: 'desc',
    });
  });

  it('omits pageNumber when absent', () => {
    const result = extractListNavParams({ search: 'acme', orderBy: 'name', sortOrder: 'asc' });

    expect(result).not.toHaveProperty('pageNumber');
    expect(result).toEqual({ search: 'acme', orderBy: 'name', sortOrder: 'asc' });
  });

  it('omits search when absent', () => {
    const result = extractListNavParams({ pageNumber: '2', orderBy: 'name', sortOrder: 'asc' });

    expect(result).not.toHaveProperty('search');
  });

  it('returns empty object for empty input', () => {
    expect(extractListNavParams({})).toEqual({});
  });

  it('does not include unrelated params', () => {
    const result = extractListNavParams({ foo: 'bar', id: '42' });

    expect(result).toEqual({});
  });

  it('preserves pageNumber as a string without coercing to number', () => {
    const result = extractListNavParams({ pageNumber: '5' });

    expect(result['pageNumber']).toBe('5');
    expect(typeof result['pageNumber']).toBe('string');
  });

  it('drops a param whose value is an empty string', () => {
    const result = extractListNavParams({ search: '', orderBy: 'name' });

    expect(result).not.toHaveProperty('search');
    expect(result).toHaveProperty('orderBy', 'name');
  });
});
