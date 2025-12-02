import { of, throwError } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { DepartmentHeadcount, Employee } from '@adventureworks-web/hr/data-access';
import type { Department } from '@adventureworks-web/shared/data-access';
import { NotificationService } from '@adventureworks-web/shared/util';
import { renderDepartmentComponent } from '../testing/render-department-component';
import { DepartmentDetailComponent } from './department-detail';

// Department 1 — Engineering, verified against the local AdventureWorks DB.
const mockDepartment: Department = {
  id: 1,
  name: 'Engineering',
  groupName: 'Research and Development',
  modifiedDate: '2008-04-30T00:00:00',
};

const mockHeadcount: DepartmentHeadcount = {
  departmentId: 1,
  departmentName: 'Engineering',
  activeEmployeeCount: 6,
};

// Active employees of department 1, verified against HumanResources.EmployeeDepartmentHistory.
const mockEmployees: Employee[] = [
  {
    id: 2,
    firstName: 'Terri',
    lastName: 'Duffy',
    middleName: null,
    title: null,
    suffix: null,
    jobTitle: 'Vice President of Engineering',
    maritalStatus: 'S',
    gender: 'F',
    salariedFlag: true,
    organizationLevel: 1,
    nationalIdNumber: '245797967',
    loginId: 'adventure-works\\terri0',
    birthDate: '1971-08-01',
    hireDate: '2008-01-31',
    currentFlag: true,
    vacationHours: 1,
    sickLeaveHours: 20,
    emailAddress: 'terri0@adventure-works.com',
    currentDepartment: 'Engineering',
    modifiedDate: '2008-04-30T00:00:00',
  },
  {
    id: 3,
    firstName: 'Roberto',
    lastName: 'Tamburello',
    middleName: null,
    title: null,
    suffix: null,
    jobTitle: 'Engineering Manager',
    maritalStatus: 'M',
    gender: 'M',
    salariedFlag: true,
    organizationLevel: 2,
    nationalIdNumber: '509647174',
    loginId: 'adventure-works\\roberto0',
    birthDate: '1974-11-12',
    hireDate: '2008-01-31',
    currentFlag: true,
    vacationHours: 2,
    sickLeaveHours: 21,
    emailAddress: 'roberto0@adventure-works.com',
    currentDepartment: 'Engineering',
    modifiedDate: '2008-04-30T00:00:00',
  },
];

function buildRoute(id = '1', queryParams: Record<string, string> = {}) {
  return { snapshot: { paramMap: { get: vi.fn().mockReturnValue(id) }, queryParams } };
}

describe('DepartmentDetailComponent', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  async function setup(
    options: {
      id?: string;
      queryParams?: Record<string, string>;
      department?: Department;
      headcount?: DepartmentHeadcount;
      employees?: Employee[];
      error?: boolean;
    } = {},
  ) {
    const route = buildRoute(options.id, options.queryParams);
    const mockHrApi = {
      getDepartment: vi.fn().mockReturnValue(
        options.error ? throwError(() => new Error('Network error')) : of(options.department ?? mockDepartment),
      ),
      getDepartmentHeadcount: vi.fn().mockReturnValue(of(options.headcount ?? mockHeadcount)),
      getDepartmentEmployees: vi.fn().mockReturnValue(of(options.employees ?? mockEmployees)),
    };
    const mockNotificationService = { error: vi.fn(), success: vi.fn() };
    // Spying on the prototype (rather than overriding the Router provider) keeps RouterLink
    // bindings in the template working against a real Router while still observing navigate().
    const navigateSpy = vi.spyOn(Router.prototype, 'navigate').mockResolvedValue(true);

    const { fixture, component } = await renderDepartmentComponent(DepartmentDetailComponent, [
      provideTranslateService(),
      { provide: HrApiService, useValue: mockHrApi },
      { provide: ActivatedRoute, useValue: route },
      { provide: NotificationService, useValue: mockNotificationService },
    ]);

    fixture.detectChanges();

    return { fixture, component, mockHrApi, mockNotificationService, router: { navigate: navigateSpy } };
  }

  it('loads department, headcount, and employees in parallel', async () => {
    const { component, mockHrApi } = await setup();

    expect(mockHrApi.getDepartment).toHaveBeenCalledWith(1);
    expect(mockHrApi.getDepartmentHeadcount).toHaveBeenCalledWith(1);
    expect(mockHrApi.getDepartmentEmployees).toHaveBeenCalledWith(1, { pageSize: 100 });
    expect(component['department']()).toEqual(mockDepartment);
    expect(component['headcount']()).toEqual(mockHeadcount);
    expect(component['isLoading']()).toBe(false);
  });

  it('roster rows combine firstName + lastName and expose jobTitle', async () => {
    const { component } = await setup();

    const rows = component['rosterRows']();
    expect(rows).toHaveLength(2);
    expect(rows[0]).toEqual({ id: 2, name: 'Terri Duffy', jobTitle: 'Vice President of Engineering' });
    expect(rows[1]).toEqual({ id: 3, name: 'Roberto Tamburello', jobTitle: 'Engineering Manager' });
  });

  it('roster rows link to /hr/employees/{id}', async () => {
    const { fixture } = await setup();
    fixture.detectChanges();

    const links = fixture.nativeElement.querySelectorAll('#aw-department-roster-list a');
    expect(links[0].getAttribute('href')).toBe('/hr/employees/2');
    expect(links[1].getAttribute('href')).toBe('/hr/employees/3');
  });

  it('shows empty state when the department has no active employees', async () => {
    const { fixture, component } = await setup({ employees: [] });
    fixture.detectChanges();

    expect(component['rosterRows']()).toHaveLength(0);
    const empty = fixture.nativeElement.querySelector('#aw-department-roster-empty');
    expect(empty).toBeTruthy();
  });

  it('shows error state and toast when the load fails', async () => {
    const { fixture, component, mockNotificationService } = await setup({ error: true });
    fixture.detectChanges();

    expect(component['hasError']()).toBe(true);
    expect(component['isLoading']()).toBe(false);
    expect(mockNotificationService.error).toHaveBeenCalledWith('Failed to load department. Please try again.');
    expect(fixture.nativeElement.querySelector('#aw-department-detail-error')).toBeTruthy();
  });

  it('redirects to list when id is NaN', async () => {
    const { mockHrApi, router } = await setup({ id: 'abc' });

    expect(router.navigate).toHaveBeenCalledWith(['/hr/departments']);
    expect(mockHrApi.getDepartment).not.toHaveBeenCalled();
  });

  it('redirects to list when id is zero', async () => {
    const { mockHrApi, router } = await setup({ id: '0' });

    expect(router.navigate).toHaveBeenCalledWith(['/hr/departments']);
    expect(mockHrApi.getDepartment).not.toHaveBeenCalled();
  });

  it('backQueryParams preserves search and page from query params', async () => {
    const { component } = await setup({ queryParams: { search: 'Eng', pageNumber: '2' } });

    expect(component['backQueryParams']()).toEqual({ search: 'Eng', pageNumber: '2' });
  });

  it('backQueryParams returns empty object with no query params', async () => {
    const { component } = await setup();

    expect(component['backQueryParams']()).toEqual({});
  });
});
