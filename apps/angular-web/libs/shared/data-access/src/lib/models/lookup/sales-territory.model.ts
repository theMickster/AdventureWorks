import { CountryRegion } from './country-region.model';

export interface SalesTerritory {
  readonly id: number;
  readonly name: string;
  readonly group: string;
  readonly salesYtd: number;
  readonly salesLastYear: number;
  readonly costYtd: number;
  readonly costLastYear: number;
  readonly countryRegion: CountryRegion;
}
