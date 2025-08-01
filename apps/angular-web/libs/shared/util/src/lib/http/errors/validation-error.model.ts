export interface ValidationError {
  readonly propertyName: string;
  readonly errorCode: string;
  readonly errorMessage: string;
  readonly correlationId: string;
}
