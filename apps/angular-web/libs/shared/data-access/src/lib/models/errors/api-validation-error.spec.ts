import { ApiValidationError } from './api-validation-error';
import { ValidationError } from '../validation-error.model';

describe('ApiValidationError', () => {
  const errors: ValidationError[] = [
    {
      propertyName: 'firstName',
      errorCode: 'Rule-01',
      errorMessage: 'First name is required.',
      correlationId: 'abc-123',
    },
  ];

  it('should be an instance of Error', () => {
    const error = new ApiValidationError(errors, 'abc-123');
    expect(error).toBeInstanceOf(Error);
  });

  it('should have name ApiValidationError', () => {
    const error = new ApiValidationError(errors, 'abc-123');
    expect(error.name).toBe('ApiValidationError');
  });

  it('should expose errors array', () => {
    const error = new ApiValidationError(errors, 'abc-123');
    expect(error.errors).toHaveLength(1);
    expect(error.errors[0].propertyName).toBe('firstName');
  });

  it('should expose correlationId', () => {
    const error = new ApiValidationError(errors, 'abc-123');
    expect(error.correlationId).toBe('abc-123');
  });

  it('should include error count and correlationId in message', () => {
    const error = new ApiValidationError(errors, 'abc-123');
    expect(error.message).toContain('1 error(s)');
    expect(error.message).toContain('abc-123');
  });
});
