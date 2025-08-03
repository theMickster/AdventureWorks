import { SlimReference } from '../slim-reference.model';
import { CountryRegion } from './country-region.model';

export interface StateProvince {
  readonly id: number;
  readonly code: string;
  readonly name: string;
  readonly isStateProvinceCodeUnavailable: boolean;
  readonly countryRegion: CountryRegion;
  readonly territory: SlimReference;
}
