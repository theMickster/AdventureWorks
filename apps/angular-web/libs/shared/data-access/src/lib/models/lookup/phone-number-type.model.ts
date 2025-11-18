/** Lookup value for a phone number category (e.g. Cell, Home, Work) from `GET /v1/phoneNumberTypes`. */
export interface PhoneNumberType {
  readonly id: number;
  readonly name: string;
}
