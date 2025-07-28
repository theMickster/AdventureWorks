export interface ApiError {
  readonly error: string;
  readonly correlationId: string;
  readonly timestamp: string;
}
