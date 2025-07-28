import { ValidationError } from '../validation-error.model';

export class ApiValidationError extends Error {
  readonly errors: ValidationError[];
  readonly correlationId: string;

  constructor(errors: ValidationError[], correlationId: string) {
    super(`Validation failed: ${errors.length} error(s). CorrelationId: ${correlationId}`);
    this.name = 'ApiValidationError';
    this.errors = errors;
    this.correlationId = correlationId;
  }
}
