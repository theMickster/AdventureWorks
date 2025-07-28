import { ApiEmptyResultError } from './api-empty-result-error';

describe('ApiEmptyResultError', () => {
  it('should be an instance of Error', () => {
    const error = new ApiEmptyResultError('No results found.');
    expect(error).toBeInstanceOf(Error);
  });

  it('should have name ApiEmptyResultError', () => {
    const error = new ApiEmptyResultError('No results found.');
    expect(error.name).toBe('ApiEmptyResultError');
  });

  it('should preserve the message', () => {
    const error = new ApiEmptyResultError('No results found.');
    expect(error.message).toBe('No results found.');
  });
});
