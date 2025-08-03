import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { LookupApiService } from './lookup-api.service';
import { Department } from '../models/lookup/department.model';
import { Shift } from '../models/lookup/shift.model';
import { SalesTerritory } from '../models/lookup/sales-territory.model';
import { AddressType } from '../models/lookup/address-type.model';
import { CountryRegion } from '../models/lookup/country-region.model';
import { StateProvince } from '../models/lookup/state-province.model';

const mockEnvironment = {
  production: false,
  defaultLocale: 'en',
  api: {
    primary: { baseUrl: 'https://localhost:44369/api', name: 'Test API' },
  },
};

describe('LookupApiService', () => {
  let service: LookupApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: ENVIRONMENT, useValue: mockEnvironment }],
    });
    service = TestBed.inject(LookupApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should be injectable', () => {
    expect(service).toBeTruthy();
  });

  it('should GET departments', () => {
    const mockData: Department[] = [
      { id: 1, name: 'Engineering', groupName: 'Research', modifiedDate: '2024-01-01T00:00:00' },
    ];

    service.getDepartments().subscribe((result) => {
      expect(result).toEqual(mockData);
    });

    const req = httpTesting.expectOne('https://localhost:44369/api/v1/departments');
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });

  it('should GET shifts', () => {
    const mockData: Shift[] = [
      { id: 1, name: 'Day', startTime: '07:00:00', endTime: '15:00:00', modifiedDate: '2024-01-01T00:00:00' },
    ];

    service.getShifts().subscribe((result) => {
      expect(result).toEqual(mockData);
    });

    const req = httpTesting.expectOne('https://localhost:44369/api/v1/shifts');
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });

  it('should GET territories', () => {
    const mockData: SalesTerritory[] = [
      {
        id: 1,
        name: 'Northwest',
        group: 'North America',
        salesYtd: 1000,
        salesLastYear: 900,
        costYtd: 500,
        costLastYear: 450,
        countryRegion: { code: 'US', name: 'United States' },
      },
    ];

    service.getTerritories().subscribe((result) => {
      expect(result).toEqual(mockData);
    });

    const req = httpTesting.expectOne('https://localhost:44369/api/v1/territories');
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });

  it('should GET address types', () => {
    const mockData: AddressType[] = [{ id: 1, name: 'Billing' }];

    service.getAddressTypes().subscribe((result) => {
      expect(result).toEqual(mockData);
    });

    const req = httpTesting.expectOne('https://localhost:44369/api/v1/addressTypes');
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });

  it('should GET country regions', () => {
    const mockData: CountryRegion[] = [{ code: 'US', name: 'United States' }];

    service.getCountryRegions().subscribe((result) => {
      expect(result).toEqual(mockData);
    });

    const req = httpTesting.expectOne('https://localhost:44369/api/v1/countries');
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });

  it('should GET state provinces', () => {
    const mockData: StateProvince[] = [
      {
        id: 1,
        code: 'WA',
        name: 'Washington',
        isStateProvinceCodeUnavailable: false,
        countryRegion: { code: 'US', name: 'United States' },
        territory: { id: 1, name: 'Northwest', code: 'NW' },
      },
    ];

    service.getStateProvinces().subscribe((result) => {
      expect(result).toEqual(mockData);
    });

    const req = httpTesting.expectOne('https://localhost:44369/api/v1/states');
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });
});
