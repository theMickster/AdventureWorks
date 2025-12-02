import { describe, expect, it } from 'vitest';
import { extractOrderListNavParams } from './order-list-nav-params';

describe('extractOrderListNavParams', () => {
  it('returns all 8 keys when all params are present', () => {
    const params = {
      orderDateFrom: '2025-01-01',
      orderDateTo: '2025-01-31',
      status: '5',
      salesPersonId: '275',
      territoryId: '3',
      pageNumber: '2',
      orderBy: 'orderDate',
      sortOrder: 'desc',
    };

    const result = extractOrderListNavParams(params);

    expect(result).toEqual({
      orderDateFrom: '2025-01-01',
      orderDateTo: '2025-01-31',
      status: '5',
      salesPersonId: '275',
      territoryId: '3',
      pageNumber: '2',
      orderBy: 'orderDate',
      sortOrder: 'desc',
    });
  });

  it('omits a key that is absent from params', () => {
    const params = {
      orderDateTo: '2025-01-31',
      status: '5',
      salesPersonId: '275',
      territoryId: '3',
      pageNumber: '2',
      orderBy: 'orderDate',
      sortOrder: 'desc',
    };

    const result = extractOrderListNavParams(params);

    expect(result).not.toHaveProperty('orderDateFrom');
    expect(Object.keys(result)).toHaveLength(7);
  });

  it('returns an empty object for empty input', () => {
    expect(extractOrderListNavParams({})).toEqual({});
  });

  it('does not include unrelated params', () => {
    const params = {
      search: 'acme',
      foo: 'bar',
      orderBy: 'orderDate',
    };

    const result = extractOrderListNavParams(params);

    expect(result).not.toHaveProperty('search');
    expect(result).not.toHaveProperty('foo');
    expect(result).toEqual({ orderBy: 'orderDate' });
  });

  it('preserves string type for all values', () => {
    const params = { salesPersonId: '275' };

    const result = extractOrderListNavParams(params);

    expect(typeof result['salesPersonId']).toBe('string');
    expect(result['salesPersonId']).toBe('275');
  });
});
