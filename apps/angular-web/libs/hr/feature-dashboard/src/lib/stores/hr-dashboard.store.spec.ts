import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import type { EmployeeAggregates } from '@adventureworks-web/hr/data-access';
import { HrDashboardStore } from './hr-dashboard.store';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockAggregates: EmployeeAggregates = {
  totalEmployeeCount: 290,
  activeEmployeeCount: 290,
  terminatedEmployeeCount: 0,
  departmentCount: 16,
  departmentHeadcounts: [
    { departmentId: 1, departmentName: 'Engineering', groupName: 'Research and Development', activeEmployeeCount: 6 },
  ],
  tenureDistribution: {
    underOneYear: 32,
    oneToThreeYears: 58,
    threeToFiveYears: 71,
    fiveToTenYears: 89,
    tenPlusYears: 40,
  },
  payBandSummary: [{ departmentGroup: 'Research and Development', averageRate: 32.65, minRate: 8.62, maxRate: 50.48 }],
};

describe('HrDashboardStore', () => {
  let store: InstanceType<typeof HrDashboardStore>;
  let httpTesting: HttpTestingController;

  function flushLoad(): void {
    store.load();
    httpTesting.expectOne('https://api.test.com/v1/employees/aggregates').flush(mockAggregates);
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        HrDashboardStore,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    });
    store = TestBed.inject(HrDashboardStore);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('has idle initial state', () => {
    expect(store.aggregates()).toBeNull();
    expect(store.lastUpdated()).toBeNull();
    expect(store.requestStatus()).toBe('idle');
  });

  describe('load', () => {
    it('sets loading state immediately on call', () => {
      store.load();
      expect(store.isLoading()).toBe(true);

      httpTesting.expectOne('https://api.test.com/v1/employees/aggregates').flush(mockAggregates);
    });

    it('populates aggregates and stamps lastUpdated on success', () => {
      flushLoad();

      expect(store.isLoaded()).toBe(true);
      expect(store.aggregates()).toEqual(mockAggregates);
      expect(store.lastUpdated()).toBeInstanceOf(Date);
    });

    it('sets an error and leaves aggregates null on failure', () => {
      store.load();
      httpTesting
        .expectOne('https://api.test.com/v1/employees/aggregates')
        .flush('fail', { status: 500, statusText: 'Server Error' });

      expect(store.hasError()).toBe(true);
      expect(store.aggregates()).toBeNull();
    });

    it('is callable repeatedly and refreshes lastUpdated on each successful call (US-767 manual refresh)', async () => {
      flushLoad();
      const firstLastUpdated = store.lastUpdated();
      expect(firstLastUpdated).not.toBeNull();

      // Ensure the second timestamp is measurably later.
      await new Promise((resolve) => setTimeout(resolve, 5));

      flushLoad();
      const secondLastUpdated = store.lastUpdated();

      expect(secondLastUpdated).not.toBeNull();
      expect(secondLastUpdated?.getTime()).toBeGreaterThanOrEqual(firstLastUpdated?.getTime() ?? 0);
    });

    it('cancels a stale in-flight request when load() is called again before the first resolves (switchMap)', () => {
      store.load();
      store.load();

      const requests = httpTesting.match('https://api.test.com/v1/employees/aggregates');
      expect(requests).toHaveLength(2);
      expect(requests[0].cancelled).toBe(true);

      requests[1].flush(mockAggregates);

      expect(store.isLoaded()).toBe(true);
      expect(store.aggregates()).toEqual(mockAggregates);
    });
  });
});
