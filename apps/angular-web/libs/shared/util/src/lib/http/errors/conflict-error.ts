/** Thrown for HTTP 409 responses. Body shape is `{ error, correlationId, timestamp }` — a plain object,
 *  not the array-of-ValidationError shape `ApiValidationError` expects — so it needs its own error type. */
export class ConflictError extends Error {
  readonly correlationId: string;

  constructor(message: string, correlationId: string) {
    super(message);
    this.name = 'ConflictError';
    this.correlationId = correlationId;
  }
}
