export class ApiEmptyResultError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'ApiEmptyResultError';
  }
}
