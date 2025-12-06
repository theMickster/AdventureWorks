import { of, throwError } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { Employee, EmployeeLifecycleStatus, EmployeeUpdate } from '@adventureworks-web/hr/data-access';
import { ApiValidationError, NotificationService } from '@adventureworks-web/shared/util';
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
      updateEmployee: vi.fn(),
      getDepartments: vi.fn().mockReturnValue(of([])),
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

    it('Terminate button opens the terminate modal', async () => {
      const { fixture, component } = await setup({ lifecycle: mockActiveLifecycle });
      fixture.detectChanges();

      component['onTerminateClick']();

      expect(component['terminateModalOpen']()).toBe(true);
    });
  });

  describe('Terminated employee', () => {
    it('shows the Rehire button, days-since-termination, and eligibility text; hides Terminate', async () => {
      const { fixture, component } = await setup({ lifecycle: mockTerminatedLifecycle });

      expect(fixture.nativeElement.querySelector('#aw-employee-detail-rehire-btn')).toBeTruthy();
      expect(fixture.nativeElement.querySelector('#aw-employee-detail-terminate-btn')).toBeFalsy();
      expect(component['lifecycle']()?.terminationDate).toBe(mockTerminatedLifecycle.terminationDate);
      // mockTerminatedLifecycle is terminated 30 days ago — still within the 90-day cooling-off period.
      const eligibility = fixture.nativeElement.querySelector('#aw-employee-detail-eligibility');
      expect(eligibility.textContent.trim()).toBe('No');
    });

    it('Rehire button opens the rehire modal', async () => {
      const { fixture, component } = await setup({ lifecycle: mockTerminatedLifecycle });
      fixture.detectChanges();

      component['onRehireClick']();

      expect(component['rehireModalOpen']()).toBe(true);
    });

    it('is not yet eligible for rehire while within the 90-day cooling-off period, regardless of the API eligibleForRehire flag', async () => {
      // API's eligibleForRehire only means "was terminated at least once" — the 90-day
      // cooling-off period is computed client-side from terminationDate, not read from this flag.
      const { fixture } = await setup({
        lifecycle: { ...mockActiveLifecycle, employmentStatus: 'Terminated', terminationDate: daysAgo(89), eligibleForRehire: false },
      });

      const eligibility = fixture.nativeElement.querySelector('#aw-employee-detail-eligibility');
      expect(eligibility.textContent.trim()).toBe('No');
    });

    it('is eligible for rehire at exactly the 90-day cooling-off boundary', async () => {
      const { fixture } = await setup({
        lifecycle: { ...mockActiveLifecycle, employmentStatus: 'Terminated', terminationDate: daysAgo(90), eligibleForRehire: true },
      });

      const eligibility = fixture.nativeElement.querySelector('#aw-employee-detail-eligibility');
      expect(eligibility.textContent.trim()).toBe('Yes');
    });

    it('is eligible for rehire once the 90-day cooling-off period has fully elapsed', async () => {
      const { fixture } = await setup({
        lifecycle: { ...mockActiveLifecycle, employmentStatus: 'Terminated', terminationDate: daysAgo(91), eligibleForRehire: true },
      });

      const eligibility = fixture.nativeElement.querySelector('#aw-employee-detail-eligibility');
      expect(eligibility.textContent.trim()).toBe('Yes');
    });

    it('renders days-since-termination as a number', async () => {
      const { fixture } = await setup({
        lifecycle: { ...mockActiveLifecycle, employmentStatus: 'Terminated', terminationDate: daysAgo(45) },
      });

      expect(fixture.nativeElement.textContent).toContain('45');
    });
  });

  describe('Editing personal info', () => {
    it('clicking Edit populates the form from employee() and shows the form instead of the read-only view', async () => {
      const { fixture, component } = await setup();

      component['onEditClick']();
      fixture.detectChanges();

      expect(component['isEditing']()).toBe(true);
      expect(component['personalInfoForm'].value).toEqual({
        firstName: mockEmployee.firstName,
        middleName: '',
        lastName: mockEmployee.lastName,
        jobTitle: mockEmployee.jobTitle,
        maritalStatus: mockEmployee.maritalStatus,
        gender: mockEmployee.gender,
      });
      expect(fixture.nativeElement.querySelector('#aw-employee-detail-personal-form')).toBeTruthy();
      expect(fixture.nativeElement.querySelector('#aw-employee-detail-edit-btn')).toBeFalsy();
    });

    it('changing fields including Job Title and clicking Save calls updateEmployee with the exact payload, updates employee(), exits edit mode, and shows a success toast', async () => {
      const updatedEmployee: Employee = {
        ...mockEmployee,
        firstName: 'Jane',
        jobTitle: 'Principal Engineer',
        maritalStatus: 'M',
      };
      const { fixture, component, mockHrApi, mockNotificationService } = await setup();
      mockHrApi.updateEmployee.mockReturnValue(of(updatedEmployee));

      component['onEditClick']();
      component['personalInfoForm'].patchValue({
        firstName: 'Jane',
        jobTitle: 'Principal Engineer',
        maritalStatus: 'M',
      });
      component['onSaveClick']();
      fixture.detectChanges();

      const expectedPayload: EmployeeUpdate = {
        id: mockEmployee.id,
        firstName: 'Jane',
        middleName: null,
        lastName: mockEmployee.lastName,
        title: mockEmployee.title,
        suffix: mockEmployee.suffix,
        jobTitle: 'Principal Engineer',
        maritalStatus: 'M',
        gender: mockEmployee.gender,
      };
      expect(mockHrApi.updateEmployee).toHaveBeenCalledWith(mockEmployee.id, expectedPayload);
      expect(component['employee']()).toEqual(updatedEmployee);
      expect(component['isEditing']()).toBe(false);
      expect(component['isSaving']()).toBe(false);
      expect(mockNotificationService.success).toHaveBeenCalledWith('Employee updated successfully.');
    });

    it('preserves an existing title/suffix unchanged on save, since they are not editable fields', async () => {
      const employeeWithHonorific: Employee = { ...mockEmployee, title: 'Dr.', suffix: 'III' };
      const { fixture, component, mockHrApi } = await setup({ employee: employeeWithHonorific });
      mockHrApi.updateEmployee.mockReturnValue(of(employeeWithHonorific));

      component['onEditClick']();
      component['onSaveClick']();
      fixture.detectChanges();

      expect(mockHrApi.updateEmployee).toHaveBeenCalledWith(
        employeeWithHonorific.id,
        expect.objectContaining({ title: 'Dr.', suffix: 'III' }),
      );
    });

    it('clicking Cancel after changing fields without saving returns to the read-only view showing the original values', async () => {
      const { fixture, component } = await setup();

      component['onEditClick']();
      component['personalInfoForm'].patchValue({ firstName: 'Changed But Not Saved' });
      component['onCancelClick']();
      fixture.detectChanges();

      expect(component['isEditing']()).toBe(false);
      expect(component['employee']()).toEqual(mockEmployee);
      expect(fixture.nativeElement.textContent).toContain(`${mockEmployee.firstName} ${mockEmployee.lastName}`);
      expect(fixture.nativeElement.querySelector('#aw-employee-detail-personal-form')).toBeFalsy();
    });

    it('sets a field-level error and keeps isEditing true on a 400 ApiValidationError', async () => {
      // propertyName is the raw FluentValidation C# property name (PascalCase) — only the JSON key
      // "propertyName" itself is camelCase, per ExceptionHandlerMiddleware's CamelCaseOptions.
      const { fixture, component, mockHrApi, mockNotificationService } = await setup();
      mockHrApi.updateEmployee.mockReturnValue(
        throwError(
          () =>
            new ApiValidationError(
              [{ propertyName: 'JobTitle', errorCode: 'Rule-12', errorMessage: 'Job title is required', correlationId: 'abc' }],
              'abc',
            ),
        ),
      );

      component['onEditClick']();
      fixture.detectChanges(); // mounts the edit form so its reactive form directives attach before Save runs
      component['onSaveClick']();
      fixture.detectChanges();

      expect(component['isEditing']()).toBe(true);
      expect(component['personalInfoForm'].controls.jobTitle.errors?.['server']).toBe('Job title is required');
      expect(component['personalInfoForm'].controls.jobTitle.touched).toBe(true);
      expect(mockNotificationService.error).not.toHaveBeenCalled();
    });

    it('shows the generic error toast when the ApiValidationError propertyName has no form-control mapping', async () => {
      const { fixture, component, mockHrApi, mockNotificationService } = await setup();
      mockHrApi.updateEmployee.mockReturnValue(
        throwError(
          () =>
            new ApiValidationError(
              [{ propertyName: 'Id', errorCode: 'Rule-02', errorMessage: 'Employee ID must exist prior to update', correlationId: 'abc' }],
              'abc',
            ),
        ),
      );

      component['onEditClick']();
      fixture.detectChanges();
      component['onSaveClick']();
      fixture.detectChanges();

      expect(component['isEditing']()).toBe(true);
      expect(mockNotificationService.error).toHaveBeenCalledWith('Failed to update employee. Please try again.');
    });

    it('keeps isEditing true and shows a hardcoded error toast on a network/500 error', async () => {
      const { fixture, component, mockHrApi, mockNotificationService } = await setup();
      mockHrApi.updateEmployee.mockReturnValue(throwError(() => new Error('Internal server error')));

      component['onEditClick']();
      fixture.detectChanges();
      component['onSaveClick']();
      fixture.detectChanges();

      expect(component['isEditing']()).toBe(true);
      expect(component['isSaving']()).toBe(false);
      expect(mockNotificationService.error).toHaveBeenCalledWith('Failed to update employee. Please try again.');
    });

    it('does not call updateEmployee when the form is invalid', async () => {
      const { component, mockHrApi } = await setup();

      component['onEditClick']();
      component['personalInfoForm'].patchValue({ firstName: '' });
      component['onSaveClick']();

      expect(mockHrApi.updateEmployee).not.toHaveBeenCalled();
      expect(component['isEditing']()).toBe(true);
    });
  });

  const mockOnLeaveLifecycle: EmployeeLifecycleStatus = {
    ...mockActiveLifecycle,
    employmentStatus: 'OnLeave',
  };

  describe('OnLeave employee', () => {
    it('shows the Hire button and hides Terminate/Rehire', async () => {
      const { fixture } = await setup({ lifecycle: mockOnLeaveLifecycle });

      expect(fixture.nativeElement.querySelector('#aw-employee-detail-hire-btn')).toBeTruthy();
      expect(fixture.nativeElement.querySelector('#aw-employee-detail-terminate-btn')).toBeFalsy();
      expect(fixture.nativeElement.querySelector('#aw-employee-detail-rehire-btn')).toBeFalsy();
    });

    it('renders the OnLeave badge as badge-warning', async () => {
      const { fixture } = await setup({ lifecycle: mockOnLeaveLifecycle });

      const badge = fixture.nativeElement.querySelector('#aw-employee-detail-status-badge span');
      expect(badge.className).toContain('badge-warning');
    });

    it('Hire button opens the hire modal', async () => {
      const { fixture, component } = await setup({ lifecycle: mockOnLeaveLifecycle });
      fixture.detectChanges();

      component['onHireClick']();

      expect(component['hireModalOpen']()).toBe(true);
    });
  });

  describe('Lifecycle action confirmation handlers', () => {
    it('onHireConfirmed re-fetches lifecycle status and shows a success toast', async () => {
      const { component, mockHrApi, mockNotificationService } = await setup({ lifecycle: mockOnLeaveLifecycle });
      mockHrApi.getLifecycleStatus.mockClear();
      mockHrApi.getLifecycleStatus.mockReturnValue(of(mockActiveLifecycle));

      component['onHireConfirmed']();

      expect(mockHrApi.getLifecycleStatus).toHaveBeenCalledWith(mockEmployee.id);
      expect(component['lifecycle']()).toEqual(mockActiveLifecycle);
      expect(mockNotificationService.success).toHaveBeenCalledWith('Employee hired successfully.');
    });

    it('onTerminateConfirmed re-fetches lifecycle status and shows a success toast', async () => {
      const { component, mockHrApi, mockNotificationService } = await setup({ lifecycle: mockActiveLifecycle });
      mockHrApi.getLifecycleStatus.mockClear();
      mockHrApi.getLifecycleStatus.mockReturnValue(of(mockTerminatedLifecycle));

      component['onTerminateConfirmed']();

      expect(mockHrApi.getLifecycleStatus).toHaveBeenCalledWith(mockEmployee.id);
      expect(component['lifecycle']()).toEqual(mockTerminatedLifecycle);
      expect(mockNotificationService.success).toHaveBeenCalledWith('Employee terminated successfully.');
    });

    it('onRehireConfirmed re-fetches lifecycle status and shows a success toast', async () => {
      const { component, mockHrApi, mockNotificationService } = await setup({ lifecycle: mockTerminatedLifecycle });
      mockHrApi.getLifecycleStatus.mockClear();
      mockHrApi.getLifecycleStatus.mockReturnValue(of(mockActiveLifecycle));

      component['onRehireConfirmed']();

      expect(mockHrApi.getLifecycleStatus).toHaveBeenCalledWith(mockEmployee.id);
      expect(component['lifecycle']()).toEqual(mockActiveLifecycle);
      expect(mockNotificationService.success).toHaveBeenCalledWith('Employee rehired successfully.');
    });
  });
});
