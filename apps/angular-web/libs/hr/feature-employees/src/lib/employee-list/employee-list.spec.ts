import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { BehaviorSubject } from 'rxjs';
import { patchState } from '@ngrx/signals';
import { setAllEntities } from '@ngrx/signals/entities';
import { unprotected } from '@ngrx/signals/testing';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { setError } from '@adventureworks-web/shared/data-access';
import { EmployeeStore } from '@adventureworks-web/hr/data-access';
import type { Employee } from '@adventureworks-web/hr/data-access';
import { EmployeeListComponent } from './employee-list';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

let queryParamsSub: BehaviorSubject<Record<string, string>>;

const selectId = (e: Employee) => e.id;

function makeEmployee(overrides: Partial<Employee> = {}): Employee {
  return {
    id: 1,
    firstName: 'John',
    lastName: 'Doe',
    middleName: null,
    title: null,
    suffix: null,
    jobTitle: 'Software Engineer',
    maritalStatus: 'S',
    gender: 'M',
    salariedFlag: true,
    organizationLevel: 2,
    nationalIdNumber: '123456789',
    loginId: 'adventure-works\\john',
    birthDate: '1990-01-01',
    hireDate: '2020-01-01',
    currentFlag: true,
    vacationHours: 40,
    sickLeaveHours: 24,
    emailAddress: 'john.doe@example.com',
    currentDepartment: 'Engineering',
    modifiedDate: '2026-01-01T00:00:00',
    ...overrides,
  };
}

function buildRoute(queryParams: Record<string, string> = {}) {
  queryParamsSub = new BehaviorSubject<Record<string, string>>(queryParams);
  return {
    queryParams: queryParamsSub,
  };
}

function tenEmployees(): Employee[] {
  return Array.from({ length: 10 }, (_, i) =>
    makeEmployee({ id: i + 1, firstName: `First${i}`, lastName: `Last${i}` }),
  );
}

describe('EmployeeListComponent', () => {
  let component: EmployeeListComponent;
  let fixture: ComponentFixture<EmployeeListComponent>;
  let employeeStore: InstanceType<typeof EmployeeStore>;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [EmployeeListComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    employeeStore = TestBed.inject(EmployeeStore);
    router = TestBed.inject(Router);

    vi.spyOn(employeeStore, 'loadPage');
    vi.spyOn(employeeStore, 'search');
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(EmployeeListComponent);
    component = fixture.componentInstance;
  });

  it('renders without errors', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('loads page 1 via loadPage when no filters are active', () => {
    fixture.detectChanges();

    expect(employeeStore.loadPage).toHaveBeenCalledWith({ pageNumber: 1, pageSize: 10 });
    expect(employeeStore.search).not.toHaveBeenCalled();
  });

  it('shows 10 rows with the correct columns and badge colors', () => {
    fixture.detectChanges();
    patchState(unprotected(employeeStore), setAllEntities(tenEmployees(), { selectId }));
    fixture.detectChanges();

    expect(component['rows']()).toHaveLength(10);
    const row = component['rows']()[0];
    expect(row['name']).toBe('First0 Last0');
    expect(row['jobTitle']).toBe('Software Engineer');
    expect(row['department']).toBe('Engineering');
    expect(row['status']).toBe('Active');
    expect(row['statusKey']).toBe('active');
    expect(row['hireDate']).toBe('2020-01-01');
  });

  it('renders a dash for department when currentDepartment is null', () => {
    fixture.detectChanges();
    patchState(
      unprotected(employeeStore),
      setAllEntities([makeEmployee({ currentDepartment: null })], { selectId }),
    );
    fixture.detectChanges();

    expect(component['rows']()[0]['department']).toBe('—');
  });

  it('maps a terminated employee to the terminated status key', () => {
    fixture.detectChanges();
    patchState(
      unprotected(employeeStore),
      setAllEntities([makeEmployee({ currentFlag: false })], { selectId }),
    );
    fixture.detectChanges();

    expect(component['rows']()[0]['status']).toBe('Terminated');
    expect(component['rows']()[0]['statusKey']).toBe('terminated');
  });

  it('searching by name resets pagination to page 1 and calls search', () => {
    fixture.detectChanges();
    component['filterForm'].patchValue({ name: 'Jane' });

    component['onApplyFilters']();
    queryParamsSub.next({ name: 'Jane', pageNumber: '1' });
    fixture.detectChanges();

    expect(employeeStore.search).toHaveBeenLastCalledWith({
      params: { pageNumber: 1, pageSize: 10 },
      body: { firstName: 'Jane' },
    });
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        queryParams: expect.objectContaining({ name: 'Jane', pageNumber: 1 }),
        queryParamsHandling: 'merge',
      }),
    );
  });

  it('splits a two-word name search into firstName and lastName', () => {
    queryParamsSub.next({ name: 'Jane Smith' });
    fixture.detectChanges();

    expect(employeeStore.search).toHaveBeenLastCalledWith({
      params: { pageNumber: 1, pageSize: 10 },
      body: { firstName: 'Jane', lastName: 'Smith' },
    });
  });

  it('status filter maps to currentFlag and calls search', () => {
    queryParamsSub.next({ status: 'terminated' });
    fixture.detectChanges();

    expect(employeeStore.search).toHaveBeenLastCalledWith({
      params: { pageNumber: 1, pageSize: 10 },
      body: { currentFlag: false },
    });
  });

  it('changing the search term resets pageNumber via onApplyFilters', () => {
    fixture.detectChanges();
    component['filterForm'].patchValue({ name: 'Smith' });

    component['onApplyFilters']();

    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        queryParams: expect.objectContaining({ pageNumber: 1 }),
      }),
    );
  });

  it('resetting filters clears the form and nulls URL params', () => {
    fixture.detectChanges();
    component['filterForm'].patchValue({ name: 'Smith', status: 'active' });

    component['onResetFilters']();

    expect(component['filterForm'].value).toEqual({ name: '', status: '' });
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        queryParams: { name: null, status: null, pageNumber: null },
        queryParamsHandling: 'merge',
      }),
    );
  });

  it('treats a whitespace-only name as empty and does not call search', () => {
    queryParamsSub.next({ name: '   ' });
    fixture.detectChanges();

    expect(employeeStore.loadPage).toHaveBeenCalledWith({ pageNumber: 1, pageSize: 10 });
    expect(employeeStore.search).not.toHaveBeenCalled();
  });

  it('clamps a junk pageNumber from the URL to 1', () => {
    queryParamsSub.next({ pageNumber: 'abc' });
    fixture.detectChanges();

    expect(employeeStore.loadPage).toHaveBeenCalledWith({ pageNumber: 1, pageSize: 10 });
  });

  it('renders a "New Employee" link routing to /hr/employees/new', () => {
    fixture.detectChanges();
    const compiled: HTMLElement = fixture.nativeElement;
    const newEmployeeLink = compiled.querySelector('#aw-employee-list-new-btn') as HTMLAnchorElement | null;

    expect(newEmployeeLink).toBeTruthy();
    expect(newEmployeeLink?.getAttribute('href')).toBe('/hr/employees/new');
  });

  it('row click navigates to the employee detail route', () => {
    fixture.detectChanges();

    component['onRowClick']({ id: 42 });

    expect(router.navigate).toHaveBeenCalledWith(['/hr/employees', 42]);
  });

  it('shows an error notification when the store reports an error', () => {
    const notificationService = TestBed.inject(NotificationService);
    vi.spyOn(notificationService, 'error');

    fixture.detectChanges();
    patchState(unprotected(employeeStore), setError('Failed to load employees'));
    fixture.detectChanges();

    expect(notificationService.error).toHaveBeenCalledWith('Failed to load employees. Please try again.');
  });
});
