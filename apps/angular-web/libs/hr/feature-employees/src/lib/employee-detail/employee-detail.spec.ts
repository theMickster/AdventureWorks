import { of, throwError } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { Employee, EmployeeLifecycleStatus } from '@adventureworks-web/hr/data-access';
import { NotificationService } from '@adventureworks-web/shared/util';
import { renderEmployeeComponent } from '../testing/render-employee-component';
import { EmployeeDetailComponent } from './employee-detail';

const mockEmployee: Employee = {
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
};

const mockActiveLifecycle: EmployeeLifecycleStatus = {
  employeeId: 2,
  fullName: 'Terri Duffy',
  employmentStatus: 'Active',
  hireDate: '2008-01-31',
  terminationDate: null,
  daysEmployed: 6000,
  currentDepartment: 'Engineering',
  currentShift: 'Day',
  departmentStartDate: '2008-01-31',
  currentPayRate: 50,
  payRateEffectiveDate: '2008-01-31',
  vacationHoursBalance: 1,
  sickLeaveHoursBalance: 20,
  eligibleForRehire: true,
  rehireCount: 0,
};

/** ISO date string N days before now, for boundary-accurate termination-date fixtures. */
function daysAgo(days: number): string {
  const date = new Date();
  date.setDate(date.getDate() - days);
  return date.toISOString().slice(0, 10);
}

const mockTerminatedLifecycle: EmployeeLifecycleStatus = {
  ...mockActiveLifecycle,
  employmentStatus: 'Terminated',
  terminationDate: daysAgo(30),
  eligibleForRehire: true,
};

function buildRoute(id = '2', queryParams: Record<string, string> = {}) {
  return { snapshot: { paramMap: { get: vi.fn().mockReturnValue(id) }, queryParams } };
}

describe('EmployeeDetailComponent', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  async function setup(
    options: {
      id?: string;
      queryParams?: Record<string, string>;
      employee?: Employee;
      lifecycle?: EmployeeLifecycleStatus;
      error?: boolean;
    } = {},
  ) {
    const route = buildRoute(options.id, options.queryParams);
    const mockHrApi = {
      getEmployee: vi.fn().mockReturnValue(
        options.error ? throwError(() => new Error('Network error')) : of(options.employee ?? mockEmployee),
      ),
      getLifecycleStatus: vi.fn().mockReturnValue(of(options.lifecycle ?? mockActiveLifecycle)),
    };
    const mockNotificationService = { error: vi.fn(), success: vi.fn(), info: vi.fn() };
    const navigateSpy = vi.spyOn(Router.prototype, 'navigate').mockResolvedValue(true);

    const { fixture, component } = await renderEmployeeComponent(EmployeeDetailComponent, [
      provideTranslateService(),
      { provide: HrApiService, useValue: mockHrApi },
      { provide: ActivatedRoute, useValue: route },
      { provide: NotificationService, useValue: mockNotificationService },
    ]);

    fixture.detectChanges();

    return { fixture, component, mockHrApi, mockNotificationService, router: { navigate: navigateSpy } };
  }

  it('loads employee and lifecycle status in parallel', async () => {
    const { component, mockHrApi } = await setup();

    expect(mockHrApi.getEmployee).toHaveBeenCalledWith(2);
    expect(mockHrApi.getLifecycleStatus).toHaveBeenCalledWith(2);
    expect(component['employee']()).toEqual(mockEmployee);
    expect(component['lifecycle']()).toEqual(mockActiveLifecycle);
    expect(component['isLoading']()).toBe(false);
  });

  it('shows error state and toast when the load fails', async () => {
    const { fixture, component, mockNotificationService } = await setup({ error: true });
    fixture.detectChanges();

    expect(component['hasError']()).toBe(true);
    expect(component['isLoading']()).toBe(false);
    expect(mockNotificationService.error).toHaveBeenCalledWith('Failed to load employee. Please try again.');
    expect(fixture.nativeElement.querySelector('#aw-employee-detail-error')).toBeTruthy();
  });

  it('redirects to list when id is NaN', async () => {
    const { mockHrApi, router } = await setup({ id: 'abc' });

    expect(router.navigate).toHaveBeenCalledWith(['/hr/employees']);
    expect(mockHrApi.getEmployee).not.toHaveBeenCalled();
  });

  it('redirects to list when id is zero', async () => {
    const { mockHrApi, router } = await setup({ id: '0' });

    expect(router.navigate).toHaveBeenCalledWith(['/hr/employees']);
    expect(mockHrApi.getEmployee).not.toHaveBeenCalled();
  });

  it('backQueryParams preserves name, status, and page from query params', async () => {
    const { component } = await setup({ queryParams: { name: 'Eng', status: 'active', pageNumber: '2' } });

    expect(component['backQueryParams']()).toEqual({ name: 'Eng', status: 'active', pageNumber: '2' });
  });

  describe('Active employee', () => {
    it('shows the Terminate button and hides Hire/Rehire', async () => {
      const { fixture } = await setup({ lifecycle: mockActiveLifecycle });

      expect(fixture.nativeElement.querySelector('#aw-employee-detail-terminate-btn')).toBeTruthy();
      expect(fixture.nativeElement.querySelector('#aw-employee-detail-rehire-btn')).toBeFalsy();
    });

    it('does not show termination date or rehire eligibility', async () => {
      const { fixture } = await setup({ lifecycle: mockActiveLifecycle });

      expect(fixture.nativeElement.querySelector('#aw-employee-detail-eligibility')).toBeFalsy();
    });

    it('Terminate button shows an informational notification', async () => {
      const { fixture, component, mockNotificationService } = await setup({ lifecycle: mockActiveLifecycle });
      fixture.detectChanges();

      component['onTerminateClick']();

      expect(mockNotificationService.info).toHaveBeenCalledWith('Terminate employee is not yet available.');
    });
  });

  describe('Terminated employee', () => {
    it('shows the Rehire button, days-since-termination, and eligibility text; hides Terminate', async () => {
      const { fixture, component } = await setup({ lifecycle: mockTerminatedLifecycle });

      expect(fixture.nativeElement.querySelector('#aw-employee-detail-rehire-btn')).toBeTruthy();
      expect(fixture.nativeElement.querySelector('#aw-employee-detail-terminate-btn')).toBeFalsy();
      expect(component['lifecycle']()?.terminationDate).toBe(mockTerminatedLifecycle.terminationDate);
      const eligibility = fixture.nativeElement.querySelector('#aw-employee-detail-eligibility');
      expect(eligibility.textContent.trim()).toBe('Yes');
    });

    it('Rehire button shows an informational notification', async () => {
      const { fixture, component, mockNotificationService } = await setup({ lifecycle: mockTerminatedLifecycle });
      fixture.detectChanges();

      component['onRehireClick']();

      expect(mockNotificationService.info).toHaveBeenCalledWith('Rehire employee is not yet available.');
    });

    it('is eligible for rehire within the 90-day window regardless of the API eligibleForRehire flag', async () => {
      // API's eligibleForRehire only means "was terminated at least once" — the 90-day
      // window is computed client-side from terminationDate, not read from this flag.
      const { fixture } = await setup({
        lifecycle: { ...mockActiveLifecycle, employmentStatus: 'Terminated', terminationDate: daysAgo(89), eligibleForRehire: false },
      });

      const eligibility = fixture.nativeElement.querySelector('#aw-employee-detail-eligibility');
      expect(eligibility.textContent.trim()).toBe('Yes');
    });

    it('is eligible for rehire at exactly the 90-day boundary', async () => {
      const { fixture } = await setup({
        lifecycle: { ...mockActiveLifecycle, employmentStatus: 'Terminated', terminationDate: daysAgo(90), eligibleForRehire: true },
      });

      const eligibility = fixture.nativeElement.querySelector('#aw-employee-detail-eligibility');
      expect(eligibility.textContent.trim()).toBe('Yes');
    });

    it('is not eligible for rehire beyond the 90-day window even when the API eligibleForRehire flag is true', async () => {
      const { fixture } = await setup({
        lifecycle: { ...mockActiveLifecycle, employmentStatus: 'Terminated', terminationDate: daysAgo(91), eligibleForRehire: true },
      });

      const eligibility = fixture.nativeElement.querySelector('#aw-employee-detail-eligibility');
      expect(eligibility.textContent.trim()).toBe('No');
    });

    it('renders days-since-termination as a number', async () => {
      const { fixture } = await setup({
        lifecycle: { ...mockActiveLifecycle, employmentStatus: 'Terminated', terminationDate: daysAgo(45) },
      });

      expect(fixture.nativeElement.textContent).toContain('45');
    });
  });
});
