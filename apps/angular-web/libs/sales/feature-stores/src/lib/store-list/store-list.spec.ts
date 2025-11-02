import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { patchState } from '@ngrx/signals';
import { setAllEntities } from '@ngrx/signals/entities';
import { unprotected } from '@ngrx/signals/testing';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { setError } from '@adventureworks-web/shared/data-access';
import { StoreStore } from '@adventureworks-web/sales/data-access';
import type { Store } from '@adventureworks-web/sales/data-access';
import { StoreListComponent } from './store-list';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockStore: Store = {
  id: 1,
  name: 'Northwest Bikes',
  modifiedDate: '2026-01-01T00:00:00',
  storeAddresses: [
    {
      id: 10,
      storeId: 1,
      addressTypeId: 3,
      addressTypeName: 'Main Office',
      addressLine1: '123 Main St',
      addressLine2: null,
      city: 'Seattle',
      stateProvinceCode: 'WA',
      stateProvinceName: 'Washington',
      countryRegionCode: 'US',
      countryRegionName: 'United States',
      postalCode: '98101',
      modifiedDate: '2026-01-01T00:00:00',
    },
  ],
  storeContacts: [],
  salesPerson: {
    id: 275,
    title: null,
    firstName: 'Michael',
    middleName: null,
    lastName: 'Blythe',
    suffix: null,
    jobTitle: 'Sales Representative',
    emailAddress: 'michael@example.com',
    territoryId: 1,
    salesQuota: 300000,
    bonus: 5000,
    commissionPct: 0.02,
    modifiedDate: '2026-01-01T00:00:00',
  },
};

function buildRoute(queryParams: Record<string, string> = {}) {
  return {
    snapshot: { queryParams },
    queryParams: { subscribe: vi.fn() },
  };
}

describe('StoreListComponent', () => {
  let component: StoreListComponent;
  let fixture: ComponentFixture<StoreListComponent>;
  let storeStore: InstanceType<typeof StoreStore>;
  let notificationService: NotificationService;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [StoreListComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    storeStore = TestBed.inject(StoreStore);
    notificationService = TestBed.inject(NotificationService);
    router = TestBed.inject(Router);

    vi.spyOn(storeStore, 'loadPage');
    vi.spyOn(storeStore, 'search');
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(StoreListComponent);
    component = fixture.componentInstance;
  });

  it('renders without errors', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('initializes with default load on no URL params', () => {
    fixture.detectChanges();

    expect(storeStore.loadPage).toHaveBeenCalledWith({ pageNumber: 1, pageSize: 25 });
    expect(storeStore.search).not.toHaveBeenCalled();
  });

  it('initializes with search from URL params', () => {
    route.snapshot.queryParams = { search: 'Bike', pageNumber: '2' };

    fixture.detectChanges();

    expect(storeStore.search).toHaveBeenCalledWith({
      params: { pageNumber: 2, pageSize: 25 },
      body: { name: 'Bike' },
    });
    expect(storeStore.loadPage).not.toHaveBeenCalled();
  });

  it('onSearch triggers filtered load and URL update', () => {
    fixture.detectChanges();

    component['onSearch']('Bike Shop');

    expect(storeStore.search).toHaveBeenCalledWith({
      params: { pageNumber: 1, pageSize: 25 },
      body: { name: 'Bike Shop' },
    });
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: { search: 'Bike Shop', pageNumber: 1, orderBy: null, sortOrder: null } }),
    );
  });

  it('onClearSearch resets to page 1 and clears URL param', () => {
    fixture.detectChanges();

    component['onClearSearch']();

    expect(storeStore.loadPage).toHaveBeenLastCalledWith({ pageNumber: 1, pageSize: 25 });
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: { search: null, pageNumber: null, orderBy: null, sortOrder: null } }),
    );
  });

  it('onPageChange updates page and URL', () => {
    fixture.detectChanges();

    component['onPageChange'](3);

    expect(storeStore.loadPage).toHaveBeenLastCalledWith({ pageNumber: 3, pageSize: 25 });
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: { pageNumber: 3 } }),
    );
  });

  it('rows computed correctly from store entities', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(storeStore), setAllEntities([mockStore]));
    fixture.detectChanges();

    const rows = component['rows']();
    expect(rows).toHaveLength(1);
    expect(rows[0]).toMatchObject({
      id: 1,
      name: 'Northwest Bikes',
      salesPerson: 'Michael Blythe',
      city: 'Seattle',
      state: 'Washington',
    });
  });

  it('rows shows dash for missing salesPerson', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    const storeNoSp: Store = { ...mockStore, salesPerson: null };
    patchState(unprotected(storeStore), setAllEntities([storeNoSp]));
    fixture.detectChanges();

    const rows = component['rows']();
    expect(rows[0]['salesPerson']).toBe('—');
  });

  it('rows shows dash for missing addresses', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    const storeNoAddr: Store = { ...mockStore, storeAddresses: [] };
    patchState(unprotected(storeStore), setAllEntities([storeNoAddr]));
    fixture.detectChanges();

    const rows = component['rows']();
    expect(rows[0]['city']).toBe('—');
    expect(rows[0]['state']).toBe('—');
  });

  it('shows error toast when store has error', async () => {
    vi.spyOn(notificationService, 'error');
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(storeStore), setError('load failed'));
    fixture.detectChanges();
    await fixture.whenStable();

    expect(notificationService.error).toHaveBeenCalledWith('Failed to load stores. Please try again.');
  });

  it('onSortChange with no active search calls loadPage with sort params', () => {
    fixture.detectChanges();

    component['onSortChange']({ column: 'name', direction: 'asc' });

    expect(storeStore.loadPage).toHaveBeenCalledWith(
      expect.objectContaining({ pageSize: 25, orderBy: 'name', sortOrder: 'asc' }),
    );
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: expect.objectContaining({ orderBy: 'name', sortOrder: 'asc' }) }),
    );
  });

  it('onSortChange with active search calls search with sort params', () => {
    fixture.detectChanges();
    component['searchTerm'].set('Bike');

    component['onSortChange']({ column: 'name', direction: 'desc' });

    expect(storeStore.search).toHaveBeenCalledWith({
      params: expect.objectContaining({ orderBy: 'name', sortOrder: 'desc' }),
      body: { name: 'Bike' },
    });
  });

  it('onSortChange with invalid column key does nothing', () => {
    fixture.detectChanges();
    const loadPageCallsBefore = (storeStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.length;
    const searchCallsBefore = (storeStore.search as ReturnType<typeof vi.spyOn>).mock.calls.length;

    component['onSortChange']({ column: 'salesPerson', direction: 'asc' });

    expect((storeStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.length).toBe(loadPageCallsBefore);
    expect((storeStore.search as ReturnType<typeof vi.spyOn>).mock.calls.length).toBe(searchCallsBefore);
  });

  it('initializes with sort state from URL params', () => {
    route.snapshot.queryParams = { orderBy: 'name', sortOrder: 'desc' };

    fixture.detectChanges();

    expect(storeStore.loadPage).toHaveBeenCalledWith({ pageNumber: 1, pageSize: 25, orderBy: 'name', sortOrder: 'desc' });
    expect(component['sortColumn']()).toBe('name');
    expect(component['sortDirection']()).toBe('desc');
  });

  it('onPageChange with active sort carries sort params', () => {
    fixture.detectChanges();
    component['sortColumn'].set('name');
    component['sortDirection'].set('asc');

    component['onPageChange'](2);

    expect(storeStore.loadPage).toHaveBeenLastCalledWith({ pageNumber: 2, pageSize: 25, orderBy: 'name', sortOrder: 'asc' });
  });

  it('onSearch with empty string reloads full list', () => {
    fixture.detectChanges();

    component['onSearch']('');

    expect(storeStore.loadPage).toHaveBeenLastCalledWith({ pageNumber: 1, pageSize: 25 });
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: { search: null, pageNumber: null, orderBy: null, sortOrder: null } }),
    );
  });

  it('onSearch with whitespace-only string reloads full list', () => {
    fixture.detectChanges();

    component['onSearch']('   ');

    expect(storeStore.loadPage).toHaveBeenLastCalledWith({ pageNumber: 1, pageSize: 25 });
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: { search: null, pageNumber: null, orderBy: null, sortOrder: null } }),
    );
  });
});
