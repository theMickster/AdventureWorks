import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import type { EmployeeOrgTreeItem } from '@adventureworks-web/hr/data-access';
import type { Department } from '@adventureworks-web/shared/data-access';
import { OrgChartStore } from './org-chart.store';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

// Verified against the local AdventureWorks DB: CEO, VP Sales, and Brian Welcker's 3 direct reports.
// Note Brian Welcker's own parentEmployeeId is null in the real data (CEO's OrganizationNode is SQL
// NULL, so the hierarchyid ancestor self-join misses) — build-org-tree.ts reparents him under the CEO.
const mockItems: EmployeeOrgTreeItem[] = [
  {
    employeeId: 1,
    fullName: 'Ken Sánchez',
    jobTitle: 'Chief Executive Officer',
    departmentName: 'Executive',
    organizationLevel: null,
    parentEmployeeId: null,
  },
  {
    employeeId: 273,
    fullName: 'Brian Welcker',
    jobTitle: 'Vice President of Sales',
    departmentName: 'Sales',
    organizationLevel: 1,
    parentEmployeeId: null,
  },
  {
    employeeId: 274,
    fullName: 'Stephen Jiang',
    jobTitle: 'North American Sales Manager',
    departmentName: 'Sales',
    organizationLevel: 2,
    parentEmployeeId: 273,
  },
  {
    employeeId: 285,
    fullName: 'Syed Abbas',
    jobTitle: 'Pacific Sales Manager',
    departmentName: 'Sales',
    organizationLevel: 2,
    parentEmployeeId: 273,
  },
  {
    employeeId: 287,
    fullName: 'Amy Alberts',
    jobTitle: 'European Sales Manager',
    departmentName: 'Sales',
    organizationLevel: 2,
    parentEmployeeId: 273,
  },
];

const mockDepartments: Department[] = [
  { id: 1, name: 'Executive', groupName: 'Executive General and Administration', modifiedDate: '2008-04-30T00:00:00' },
  { id: 2, name: 'Sales', groupName: 'Sales and Marketing', modifiedDate: '2008-04-30T00:00:00' },
];

describe('OrgChartStore', () => {
  let store: InstanceType<typeof OrgChartStore>;
  let httpTesting: HttpTestingController;

  function flushLoad(): void {
    store.load();
    httpTesting.expectOne('https://api.test.com/v1/employees/org-tree').flush(mockItems);
    httpTesting.expectOne('https://api.test.com/v1/departments').flush(mockDepartments);
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        OrgChartStore,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    });
    store = TestBed.inject(OrgChartStore);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('has idle initial state', () => {
    expect(store.tree()).toBeNull();
    expect(store.requestStatus()).toBe('idle');
    expect(store.expandedIds().size).toBe(0);
  });

  describe('load', () => {
    it('builds the tree, computes stats, and seeds expandedIds to CEO + level-1 on success', () => {
      flushLoad();

      expect(store.isLoaded()).toBe(true);
      expect(store.tree()?.employeeId).toBe(1);
      expect(store.tree()?.colorClass).toBe('primary');
      expect(store.stats().totalEmployees).toBe(5);
      expect(store.expandedIds()).toEqual(new Set([1, 273]));
      expect(store.highlightedId()).toBeNull();
    });

    it('sets an error and does not crash the tree/stats computed signals on failure', () => {
      store.load();
      httpTesting.expectOne('https://api.test.com/v1/employees/org-tree').flush('fail', { status: 500, statusText: 'Server Error' });
      // forkJoin unsubscribes the sibling request as soon as one errors — it's cancelled, not
      // open, so flushing it would throw. Only flush if it's still actually pending.
      const departmentsRequests = httpTesting.match('https://api.test.com/v1/departments');
      if (departmentsRequests.length > 0 && !departmentsRequests[0].cancelled) {
        departmentsRequests[0].flush(mockDepartments);
      }

      expect(store.hasError()).toBe(true);
      expect(store.tree()).toBeNull();
      expect(store.stats().totalEmployees).toBe(0);
    });
  });

  describe('toggleExpanded', () => {
    it('adds an id not yet expanded, using a new Set reference', () => {
      flushLoad();
      const before = store.expandedIds();

      store.toggleExpanded(274);

      expect(store.expandedIds()).not.toBe(before);
      expect(store.expandedIds().has(274)).toBe(true);
    });

    it('removes an id that is already expanded', () => {
      flushLoad();

      store.toggleExpanded(1);

      expect(store.expandedIds().has(1)).toBe(false);
    });
  });

  describe('searchAndExpand', () => {
    it('expands the ancestor chain and highlights the match for "Stephen Jiang" (chain 273 -> 1)', () => {
      flushLoad();
      store.toggleExpanded(1);
      store.toggleExpanded(273);

      store.searchAndExpand('Stephen Jiang');

      expect(store.highlightedId()).toBe(274);
      expect(store.expandedIds().has(273)).toBe(true);
      expect(store.expandedIds().has(1)).toBe(true);
    });

    it('leaves expansion untouched when there is no match', () => {
      flushLoad();
      const before = store.expandedIds();

      store.searchAndExpand('Nobody Matches This');

      expect(store.expandedIds()).toBe(before);
      expect(store.highlightedId()).toBeNull();
    });

    it('clears the highlight without touching expansion when the term is cleared', () => {
      flushLoad();
      store.searchAndExpand('Stephen Jiang');
      const expandedAfterSearch = store.expandedIds();

      store.searchAndExpand('');

      expect(store.highlightedId()).toBeNull();
      expect(store.expandedIds()).toBe(expandedAfterSearch);
    });
  });
});
