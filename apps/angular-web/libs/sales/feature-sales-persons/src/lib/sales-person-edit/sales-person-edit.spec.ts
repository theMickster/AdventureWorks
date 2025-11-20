import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { of, Subject, throwError } from 'rxjs';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { SalesPerson, SalesPersonSalesConfigUpdate } from '@adventureworks-web/sales/data-access';
import { LookupApiService } from '@adventureworks-web/shared/data-access';
import type { SalesTerritory } from '@adventureworks-web/shared/data-access';
import { SalesPersonEditComponent } from './sales-person-edit';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockPerson: SalesPerson = {
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
  salesYtd: 3763178.18,
  territoryName: 'Central',
  modifiedDate: '2023-01-01T00:00:00',
};

const mockTerritories: SalesTerritory[] = [
  {
    id: 1,
    name: 'Northwest',
    group: 'North America',
    salesYtd: 0,
    salesLastYear: 0,
    costYtd: 0,
    costLastYear: 0,
    countryRegion: { code: 'US', name: 'United States' },
  },
  {
    id: 3,
    name: 'Central',
    group: 'North America',
    salesYtd: 0,
    salesLastYear: 0,
    costYtd: 0,
    costLastYear: 0,
    countryRegion: { code: 'US', name: 'United States' },
  },
];

const mockUpdatedPerson: SalesPerson = { ...mockPerson, salesQuota: 300000, bonus: 5000 };

function buildRoute(id = '275', queryParams: Record<string, string> = {}) {
  return {
    snapshot: {
      paramMap: { get: vi.fn().mockReturnValue(id) },
      queryParams,
    },
  };
}

describe('SalesPersonEditComponent', () => {
  let component: SalesPersonEditComponent;
  let fixture: ComponentFixture<SalesPersonEditComponent>;
  let salesApiService: SalesApiService;
  let lookupApiService: LookupApiService;
  let notificationService: NotificationService;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [SalesPersonEditComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    salesApiService = TestBed.inject(SalesApiService);
    lookupApiService = TestBed.inject(LookupApiService);
    notificationService = TestBed.inject(NotificationService);
    router = TestBed.inject(Router);

    vi.spyOn(salesApiService, 'getSalesPerson').mockReturnValue(of(mockPerson));
    vi.spyOn(lookupApiService, 'getTerritories').mockReturnValue(of(mockTerritories));
    vi.spyOn(salesApiService, 'updateSalesPersonSalesConfig').mockReturnValue(of(mockUpdatedPerson));
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(SalesPersonEditComponent);
    component = fixture.componentInstance;
  });

  it('renders skeleton while isLoading is true', () => {
    const subject = new Subject<SalesPerson>();
    vi.spyOn(salesApiService, 'getSalesPerson').mockReturnValue(subject.asObservable());
    fixture.detectChanges();

    expect(component['isLoading']()).toBe(true);
    expect(fixture.nativeElement.querySelector('#aw-sales-person-edit-loading')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('#aw-sales-person-edit-form-container')).toBeNull();
  });

  it('load error shows error toast and navigates to sales person detail', () => {
    vi.spyOn(salesApiService, 'getSalesPerson').mockReturnValue(
      throwError(() => new Error('load failed')),
    );
    vi.spyOn(notificationService, 'error');
    fixture.detectChanges();

    expect(notificationService.error).toHaveBeenCalledWith(
      'Failed to load sales person. Please try again.',
    );
    expect(router.navigate).toHaveBeenCalledWith(['/sales/persons', 275]);
  });

  it('pre-populates form from loaded sales person', () => {
    fixture.detectChanges();

    expect(component['form'].value.territoryId).toBe('3');
    expect(component['form'].value.salesQuota).toBe('250000');
    expect(component['form'].value.bonus).toBe('4100');
    expect(component['form'].value.commissionPct).toBe('1.2');
  });

  it('read-only section displays person name and email', () => {
    fixture.detectChanges();

    const nameEl = fixture.nativeElement.querySelector('#aw-sales-person-edit-fullname');
    const emailEl = fixture.nativeElement.querySelector('#aw-sales-person-edit-email');

    expect(nameEl?.textContent?.trim()).toBe('Michael Blythe');
    expect(emailEl?.textContent?.trim()).toBe('michael.blythe@adventure-works.com');
  });

  it('save button is disabled while isSaving is true', () => {
    fixture.detectChanges();
    component['isSaving'].set(true);
    fixture.detectChanges();

    const saveBtn = fixture.nativeElement.querySelector('#aw-sales-person-edit-save');
    expect(saveBtn?.disabled).toBe(true);
  });

  it('onSubmit calls updateSalesPersonSalesConfig with correct payload', () => {
    fixture.detectChanges();
    component['onSubmit']();

    expect(salesApiService.updateSalesPersonSalesConfig).toHaveBeenCalledWith(
      275,
      expect.objectContaining<SalesPersonSalesConfigUpdate>({
        id: 275,
        territoryId: 3,
        salesQuota: 250000,
        bonus: 4100,
        commissionPct: 0.012,
      }),
    );
  });

  it('on save success, navigates to /sales/persons/:id', () => {
    fixture.detectChanges();
    component['onSubmit']();

    expect(router.navigate).toHaveBeenCalledWith(['/sales/persons', 275]);
  });

  it('on save error, resets isSaving and shows error toast', () => {
    vi.spyOn(salesApiService, 'updateSalesPersonSalesConfig').mockReturnValue(
      throwError(() => new Error('fail')),
    );
    vi.spyOn(notificationService, 'error');
    fixture.detectChanges();
    component['onSubmit']();

    expect(component['isSaving']()).toBe(false);
    expect(notificationService.error).toHaveBeenCalledWith(
      'Failed to update sales configuration. Please try again.',
    );
  });

  it('onCancel navigates without calling the service', () => {
    fixture.detectChanges();
    component['onCancel']();

    expect(salesApiService.updateSalesPersonSalesConfig).not.toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/sales/persons', 275], { queryParams: {} });
  });

  it('double-submit guard: second onSubmit call when isSaving=true is a no-op', () => {
    fixture.detectChanges();
    component['isSaving'].set(true);
    component['onSubmit']();

    expect(salesApiService.updateSalesPersonSalesConfig).not.toHaveBeenCalled();
  });

  it("invalid id='0' navigates to /sales/persons", () => {
    route.snapshot.paramMap.get = vi.fn().mockReturnValue('0');
    fixture.detectChanges();
    expect(router.navigate).toHaveBeenCalledWith(['/sales/persons']);
  });

  it('null territoryId is preserved as null in the payload', () => {
    const personNoTerritory: SalesPerson = { ...mockPerson, territoryId: null };
    vi.spyOn(salesApiService, 'getSalesPerson').mockReturnValue(of(personNoTerritory));
    fixture.detectChanges();

    component['onSubmit']();

    expect(salesApiService.updateSalesPersonSalesConfig).toHaveBeenCalledWith(
      275,
      expect.objectContaining({ territoryId: null }),
    );
  });
});
