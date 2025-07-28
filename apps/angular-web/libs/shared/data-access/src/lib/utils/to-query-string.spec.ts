import { toQueryString } from './to-query-string';

describe('toQueryString', () => {
  it('should return empty string for empty params', () => {
    expect(toQueryString({})).toBe('');
  });

  it('should serialize all params', () => {
    const result = toQueryString({
      pageNumber: 2,
      pageSize: 10,
      sortOrder: 'asc',
      orderBy: 'name',
    });
    expect(result).toBe('?pageNumber=2&pageSize=10&sortOrder=asc&orderBy=name');
  });

  it('should omit undefined values', () => {
    const result = toQueryString({
      pageNumber: 1,
      pageSize: undefined,
    });
    expect(result).toBe('?pageNumber=1');
  });

  it('should serialize single param', () => {
    expect(toQueryString({ pageNumber: 1 })).toBe('?pageNumber=1');
  });

  it('should serialize sortOrder correctly', () => {
    expect(toQueryString({ sortOrder: 'desc' })).toBe('?sortOrder=desc');
  });
});
