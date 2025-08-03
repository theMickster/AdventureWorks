import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { LookupStore } from './lookup.store';
import { Department } from '../models/lookup/department.model';
import { SalesTerritory } from '../models/lookup/sales-territory.model';

const mockEnvironment = {
  production: false,
  defaultLocale: 'en',
  api: {
    primary: { baseUrl: 'https://localhost:44369/api', name: 'Test API' },
  },
};

const mockDepartments: Department[] = [
  { id: 1, name: 'Engineering', groupName: 'Research', modifiedDate: '2024-01-01T00:00:00' },
  { id: 2, name: 'Sales', groupName: 'Sales and Marketing', modifiedDate: '2024-01-01T00:00:00' },
];

const mockTerritories: SalesTerritory[] = [
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

describe('LookupStore', () => {
  let store: InstanceType<typeof LookupStore>;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: ENVIRONMENT, useValue: mockEnvironment }],
    });
    store = TestBed.inject(LookupStore);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should have empty initial state for all collections', () => {
    expect(store.departments()).toEqual([]);
    expect(store.shifts()).toEqual([]);
    expect(store.territories()).toEqual([]);
    expect(store.addressTypes()).toEqual([]);
    expect(store.countryRegions()).toEqual([]);
    expect(store.stateProvinces()).toEqual([]);
  });

  it('should have idle initial requestStatus', () => {
    expect(store.requestStatus()).toBe('idle');
    expect(store.isLoading()).toBe(false);
    expect(store.isLoaded()).toBe(false);
    expect(store.hasError()).toBe(false);
  });

  it('should fetch and populate departments', () => {
    store.loadDepartments();

    const req = httpTesting.expectOne('https://localhost:44369/api/v1/departments');
    expect(req.request.method).toBe('GET');
    req.flush(mockDepartments);

    expect(store.departments()).toEqual(mockDepartments);
    expect(store.departments().length).toBe(2);
  });

  it('should transition requestStatus from loading to loaded', () => {
    store.loadDepartments();
    expect(store.isLoading()).toBe(true);

    const req = httpTesting.expectOne('https://localhost:44369/api/v1/departments');
    req.flush(mockDepartments);

    expect(store.isLoaded()).toBe(true);
    expect(store.isLoading()).toBe(false);
  });

  it('should cache departments and not make a second request', () => {
    store.loadDepartments();
    httpTesting.expectOne('https://localhost:44369/api/v1/departments').flush(mockDepartments);

    store.loadDepartments();
    httpTesting.expectNone('https://localhost:44369/api/v1/departments');

    expect(store.departments()).toEqual(mockDepartments);
  });

  it('should set error state on HTTP failure', () => {
    store.loadDepartments();

    const req = httpTesting.expectOne('https://localhost:44369/api/v1/departments');
    req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

    expect(store.hasError()).toBe(true);
    expect(store.error()).toBeTruthy();
  });

  it('should fetch and populate territories independently', () => {
    store.loadTerritories();

    const req = httpTesting.expectOne('https://localhost:44369/api/v1/territories');
    expect(req.request.method).toBe('GET');
    req.flush(mockTerritories);

    expect(store.territories()).toEqual(mockTerritories);
    expect(store.departments()).toEqual([]);
  });
});
