import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { BehaviorSubject } from 'rxjs';
import { patchState } from '@ngrx/signals';
import { setAllEntities } from '@ngrx/signals/entities';
import { unprotected } from '@ngrx/signals/testing';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { setError } from '@adventureworks-web/shared/data-access';
import { VendorStore } from '@adventureworks-web/purchasing/data-access';
import type { VendorListItem } from '@adventureworks-web/purchasing/data-access';
import { VendorListComponent } from './vendor-list';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

let queryParamsSub: BehaviorSubject<Record<string, string>>;

const selectId = (v: VendorListItem) => v.vendorId;

// Real AdventureWorks vendor rows (BusinessEntityID 1576, 1678).
const mockVendor: VendorListItem = {
  vendorId: 1576,
  name: 'Superior Bicycles',
  accountNumber: 'SUPERIOR0001',
  creditRatingLabel: 'Superior',
  preferredVendorStatus: true,
  activeFlag: true,
  totalSpend: 5034266.74,
  poCount: 50,
  isHighRisk: false,
};

const mockHighRiskVendor: VendorListItem = {
  vendorId: 1678,
  name: 'Proseware, Inc.',
  accountNumber: 'PROSEWAR0001',
  creditRatingLabel: 'Average',
  preferredVendorStatus: false,
  activeFlag: false,
  totalSpend: 2593901.31,
  poCount: 51,
  isHighRisk: true,
};

function buildRoute(queryParams: Record<string, string> = {}) {
  queryParamsSub = new BehaviorSubject<Record<string, string>>(queryParams);
  return {
    queryParams: queryParamsSub,
  };
}

describe('VendorListComponent', () => {
  let component: VendorListComponent;
  let fixture: ComponentFixture<VendorListComponent>;
  let vendorStore: InstanceType<typeof VendorStore>;
  let notificationService: NotificationService;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [VendorListComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    vendorStore = TestBed.inject(VendorStore);
    notificationService = TestBed.inject(NotificationService);
    router = TestBed.inject(Router);

    vi.spyOn(vendorStore, 'loadPage');
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(VendorListComponent);
    component = fixture.componentInstance;
  });

  it('renders without errors', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('loads page 1 with no filters when no URL params', () => {
    fixture.detectChanges();

    expect(vendorStore.loadPage).toHaveBeenCalledWith({ pageNumber: 1, pageSize: 25 });
  });

  it('restores filter params from the URL and forwards them to loadPage', () => {
    queryParamsSub.next({ creditRating: '4', preferredVendorStatus: 'true', activeFlag: 'true' });

    fixture.detectChanges();

    expect(vendorStore.loadPage).toHaveBeenCalledWith({
      pageNumber: 1,
      pageSize: 25,
      creditRating: 4,
      preferredVendorStatus: true,
      activeFlag: true,
    });
  });

  it('drops a non-numeric creditRating URL param instead of forwarding NaN', () => {
    queryParamsSub.next({ creditRating: 'abc' });
    fixture.detectChanges();

    const lastCall = (vendorStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.at(-1)?.[0] as Record<string, unknown>;
    expect('creditRating' in lastCall).toBe(false);
  });

  it('drops an out-of-range creditRating URL param (9) instead of forwarding it to a server 400', () => {
    queryParamsSub.next({ creditRating: '9' });
    fixture.detectChanges();

    const lastCall = (vendorStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.at(-1)?.[0] as Record<string, unknown>;
    expect('creditRating' in lastCall).toBe(false);
  });

  it('drops a zero creditRating URL param (below the 1-5 range)', () => {
    queryParamsSub.next({ creditRating: '0' });
    fixture.detectChanges();

    const lastCall = (vendorStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.at(-1)?.[0] as Record<string, unknown>;
    expect('creditRating' in lastCall).toBe(false);
  });

  it('resets the credit-rating select to "All ratings" for an out-of-range bookmarked URL instead of showing an unmatched value', () => {
    queryParamsSub.next({ creditRating: '9' });
    fixture.detectChanges();

    expect(component['filterForm'].getRawValue().creditRating).toBe('');
  });

  it('clamps junk pageNumber from the URL to 1', () => {
    queryParamsSub.next({ pageNumber: '-3' });
    fixture.detectChanges();

    expect(vendorStore.loadPage).toHaveBeenCalledWith(expect.objectContaining({ pageNumber: 1 }));
  });

  it('applies a filter, resets to page 1, and writes back to the URL with merge', () => {
    fixture.detectChanges();
    component['filterForm'].patchValue({ creditRating: '4' });

    component['onApplyFilters']();
    queryParamsSub.next({ creditRating: '4', pageNumber: '1' });
    fixture.detectChanges();

    expect(vendorStore.loadPage).toHaveBeenLastCalledWith(
      expect.objectContaining({ pageNumber: 1, pageSize: 25, creditRating: 4 }),
    );
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        queryParams: expect.objectContaining({ creditRating: 4, pageNumber: 1 }),
        queryParamsHandling: 'merge',
      }),
    );
  });

  it('omits an unchecked toggle from the API call and nulls its URL param', () => {
    fixture.detectChanges();
    component['filterForm'].patchValue({ preferredVendorStatus: true });
    component['filterForm'].patchValue({ preferredVendorStatus: false });

    component['onApplyFilters']();
    queryParamsSub.next({ pageNumber: '1' });
    fixture.detectChanges();

    const lastCall = (vendorStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.at(-1)?.[0] as Record<string, unknown>;
    expect('preferredVendorStatus' in lastCall).toBe(false);

    expect(router.navigate).toHaveBeenLastCalledWith(
      [],
      expect.objectContaining({
        queryParams: { creditRating: null, preferredVendorStatus: null, activeFlag: null, pageNumber: 1 },
        queryParamsHandling: 'merge',
      }),
    );
  });

  it('resets all filters, nulls URL params, and reloads the default view', () => {
    queryParamsSub.next({ creditRating: '4' });
    fixture.detectChanges();

    component['onResetFilters']();
    queryParamsSub.next({});
    fixture.detectChanges();

    expect(vendorStore.loadPage).toHaveBeenLastCalledWith({ pageNumber: 1, pageSize: 25 });
    expect(component['filterForm'].getRawValue()).toEqual({
      creditRating: '',
      preferredVendorStatus: false,
      activeFlag: false,
    });
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        queryParams: { creditRating: null, preferredVendorStatus: null, activeFlag: null, pageNumber: null },
        queryParamsHandling: 'merge',
      }),
    );
  });

  it('shows an error toast when the store reports an error', async () => {
    vi.spyOn(notificationService, 'error');
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(vendorStore), setError('load failed'));
    fixture.detectChanges();
    await fixture.whenStable();

    expect(notificationService.error).toHaveBeenCalledWith('Failed to load vendors. Please try again.');
  });

  it('projects credit rating label, status/risk keys, and currency-ready total for a high-risk vendor', () => {
    fixture.detectChanges();

    patchState(unprotected(vendorStore), setAllEntities([mockVendor, mockHighRiskVendor], { selectId }));
    fixture.detectChanges();

    const rows = component['rows']();
    expect(rows).toHaveLength(2);
    expect(rows[0]).toMatchObject({
      vendorId: 1576,
      name: 'Superior Bicycles',
      creditRatingLabel: 'Superior',
      statusKey: 'preferred',
      riskKey: 'low-risk',
      totalSpend: 5034266.74,
    });
    expect(rows[1]).toMatchObject({
      vendorId: 1678,
      creditRatingLabel: 'Average',
      statusKey: 'standard',
      riskKey: 'high-risk',
    });
  });

  it('shows the EmptyState (via DataTableComponent) when a filter matches no vendors', () => {
    fixture.detectChanges();

    const httpTesting = TestBed.inject(HttpTestingController);
    httpTesting.expectOne('https://api.test.com/v1/vendors?pageNumber=1&pageSize=25').flush({
      pageNumber: 1,
      pageSize: 25,
      totalPages: 0,
      totalRecords: 0,
      hasPreviousPage: false,
      hasNextPage: false,
      results: [],
    });
    fixture.detectChanges();

    const empty = fixture.nativeElement.querySelector('#aw-vendor-list-table-empty');
    expect(empty).toBeTruthy();
    httpTesting.verify();
  });

  it('View button navigates to the correct vendor detail URL', () => {
    fixture.detectChanges();

    patchState(unprotected(vendorStore), setAllEntities([mockVendor], { selectId }));
    fixture.detectChanges();

    component['onViewClick']({ vendorId: 1576 });

    expect(router.navigate).toHaveBeenCalledWith(['/purchasing/vendors', 1576]);
  });
});
