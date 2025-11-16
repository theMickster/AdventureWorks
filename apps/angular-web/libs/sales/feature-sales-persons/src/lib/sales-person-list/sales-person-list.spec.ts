import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { provideTranslateService } from '@ngx-translate/core';
import { SalesPersonStore } from '@adventureworks-web/sales/data-access';
import type { SalesPerson } from '@adventureworks-web/sales/data-access';
import { NotificationService } from '@adventureworks-web/shared/util';
import { SalesPersonListComponent } from './sales-person-list';

const person274: SalesPerson = {
  id: 274,
  title: null,
  firstName: 'Stephen',
  middleName: null,
  lastName: 'Jiang',
  suffix: null,
  jobTitle: 'North American Sales Manager',
  emailAddress: 'stephen.jiang@adventure-works.com',
  territoryId: 2,
  salesQuota: 300000,
  bonus: 0,
  commissionPct: 0.012,
  salesYtd: 559697.5639,
  territoryName: 'Northeast',
  modifiedDate: '2023-01-01T00:00:00',
};

const person275: SalesPerson = {
  id: 275,
  title: null,
  firstName: 'Michael',
  middleName: null,
  lastName: 'Blythe',
  suffix: null,
  jobTitle: 'Sales Representative',
  emailAddress: 'michael.blythe@adventure-works.com',
  territoryId: 3,
  salesQuota: 250000,
  bonus: 4100,
  commissionPct: 0.012,
  salesYtd: 3763178.1787,
  territoryName: 'Central',
  modifiedDate: '2023-01-01T00:00:00',
};

const person276: SalesPerson = {
  id: 276,
  title: null,
  firstName: 'Linda',
  middleName: null,
  lastName: 'Mitchell',
  suffix: null,
  jobTitle: 'Sales Representative',
  emailAddress: 'linda.mitchell@adventure-works.com',
  territoryId: null,
  salesQuota: null,
  bonus: 2000,
  commissionPct: 0.012,
  salesYtd: 4251368.5497,
  territoryName: null,
  modifiedDate: '2023-01-01T00:00:00',
};

const allPersons = [person274, person275, person276];

function buildRoute(queryParams: Record<string, string> = {}) {
  return {
    snapshot: { queryParams },
    queryParams: { subscribe: vi.fn() },
  };
}

describe('SalesPersonListComponent', () => {
  let component: SalesPersonListComponent;
  let fixture: ComponentFixture<SalesPersonListComponent>;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  const mockEntities = signal<SalesPerson[]>(allPersons);
  const mockHasError = signal(false);
  const mockStore = {
    isLoading: signal(false),
    hasError: mockHasError,
    entities: mockEntities,
    loadPage: vi.fn(),
  };

  const mockNotificationService = { error: vi.fn(), success: vi.fn() };

  beforeEach(async () => {
    route = buildRoute();
    mockEntities.set(allPersons);
    mockHasError.set(false);
    mockStore.loadPage.mockReset();
    mockNotificationService.error.mockReset();

    await TestBed.configureTestingModule({
      imports: [SalesPersonListComponent],
      providers: [
        provideRouter([]),
        provideTranslateService(),
        { provide: SalesPersonStore, useValue: mockStore },
        { provide: ActivatedRoute, useValue: route },
        { provide: NotificationService, useValue: mockNotificationService },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(SalesPersonListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('renders all rows when no filter is active', () => {
    const rows = component['rows']();
    expect(rows).toHaveLength(3);
  });

  it('filters by name case-insensitively', () => {
    component['searchTerm'].set('stephen');
    const rows = component['rows']();
    expect(rows).toHaveLength(1);
    expect(rows[0]['name']).toBe('Stephen Jiang');
  });

  it('filters by jobTitle', () => {
    component['searchTerm'].set('sales representative');
    const rows = component['rows']();
    expect(rows).toHaveLength(2);
  });

  it('clear search restores full list', () => {
    component['searchTerm'].set('stephen');
    expect(component['rows']()).toHaveLength(1);

    component['sortColumn'].set('salesYtd');
    component['sortDirection'].set('desc');
    component['onClearSearch']();
    expect(component['rows']()).toHaveLength(3);
    expect(component['sortColumn']()).toBe('');
    expect(component['sortDirection']()).toBe('asc');
  });

  it('sorts ascending by name', () => {
    component['sortColumn'].set('name');
    component['sortDirection'].set('asc');
    const rows = component['rows']();
    expect(rows[0]['name']).toBe('Linda Mitchell');
    expect(rows[1]['name']).toBe('Michael Blythe');
    expect(rows[2]['name']).toBe('Stephen Jiang');
  });

  it('sorts descending by salesYtd', () => {
    component['sortColumn'].set('salesYtd');
    component['sortDirection'].set('desc');
    const rows = component['rows']();
    // Linda has highest salesYtd (4251368.5497)
    expect(rows[0]['id']).toBe(276);
    // Stephen has lowest salesYtd (559697.5639)
    expect(rows[2]['id']).toBe(274);
  });

  it('ignores unknown sort column', () => {
    component['sortColumn'].set('unknown_column');
    component['sortDirection'].set('asc');
    const rows = component['rows']();
    // Should return original order unchanged
    expect(rows).toHaveLength(3);
    expect(rows[0]['id']).toBe(274);
    expect(rows[1]['id']).toBe(275);
    expect(rows[2]['id']).toBe(276);
  });

  it('formats commissionPct 0.012 as "1.20%"', () => {
    const rows = component['rows']();
    const row = rows.find((r) => r['id'] === 274);
    expect(row?.['commissionPct']).toBe('1.20%');
  });

  it('renders "—" for null territoryName', () => {
    const rows = component['rows']();
    const lindaRow = rows.find((r) => r['id'] === 276);
    expect(lindaRow?.['territory']).toBe('—');
  });

  it('calls router.navigate with search param on onSearch', () => {
    component['sortColumn'].set('salesYtd');
    component['onSearch']('jiang');
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        queryParams: { search: 'jiang', orderBy: null, sortOrder: null },
        queryParamsHandling: 'merge',
      }),
    );
    expect(component['sortColumn']()).toBe('');
    expect(component['sortDirection']()).toBe('asc');
  });

  it('restores search and sort state from URL params on ngOnInit', () => {
    route.snapshot.queryParams = { search: 'linda', orderBy: 'salesYtd', sortOrder: 'desc' };

    const freshFixture = TestBed.createComponent(SalesPersonListComponent);
    freshFixture.detectChanges();
    const freshComponent = freshFixture.componentInstance;

    expect(freshComponent['searchTerm']()).toBe('linda');
    expect(freshComponent['sortColumn']()).toBe('salesYtd');
    expect(freshComponent['sortDirection']()).toBe('desc');
  });

  it('shows error notification when store load fails', () => {
    fixture.detectChanges();
    mockHasError.set(true);
    fixture.detectChanges();
    expect(mockNotificationService.error).toHaveBeenCalledWith('Failed to load sales persons. Please try again.');
  });
});
