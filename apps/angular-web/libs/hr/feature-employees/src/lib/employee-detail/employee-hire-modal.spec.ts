import { of, throwError } from 'rxjs';
import { TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import type { EmployeeHire } from '@adventureworks-web/hr/data-access';
import type { Department } from '@adventureworks-web/shared/data-access';
import { EmployeeStore, HrApiService } from '@adventureworks-web/hr/data-access';
import { NotificationService } from '@adventureworks-web/shared/util';
import { renderEmployeeComponent } from '../testing/render-employee-component';
import { EmployeeHireModalComponent } from './employee-hire-modal';

const mockDepartments: Department[] = [
  { id: 1, name: 'Engineering', groupName: 'Research and Development', modifiedDate: '2020-01-01' },
  { id: 2, name: 'Sales', groupName: 'Sales and Marketing', modifiedDate: '2020-01-01' },
];

/** ISO date string N days from today, for boundary-accurate hire-date fixtures. */
function daysFromToday(days: number): string {
  const date = new Date();
  date.setDate(date.getDate() + days);
  return date.toISOString().slice(0, 10);
}

/** jsdom doesn't implement `<dialog>.showModal()`/`close()` — stub them before toggling `open`. */
function stubDialog(fixture: { nativeElement: HTMLElement }): void {
  const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
  dialog.showModal = vi.fn(() => {
    Object.defineProperty(dialog, 'open', { value: true, writable: true, configurable: true });
  });
  dialog.close = vi.fn(() => {
    Object.defineProperty(dialog, 'open', { value: false, writable: true, configurable: true });
  });
}

describe('EmployeeHireModalComponent', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  async function setup(options: { hireError?: boolean } = {}) {
    const mockHrApi = {
      getDepartments: vi.fn().mockReturnValue(of(mockDepartments)),
      hireEmployee: vi
        .fn()
        .mockReturnValue(
          options.hireError
            ? throwError(() => new Error('Network error'))
            : of({ businessEntityId: 2, message: 'ok' }),
        ),
      getEmployee: vi.fn().mockReturnValue(of({ id: 2 })),
    };
    const mockNotificationService = { error: vi.fn(), success: vi.fn(), info: vi.fn(), warning: vi.fn() };

    const { fixture, component } = await renderEmployeeComponent(EmployeeHireModalComponent, [
      provideTranslateService(),
      { provide: HrApiService, useValue: mockHrApi },
      { provide: NotificationService, useValue: mockNotificationService },
    ]);

    fixture.componentRef.setInput('employeeId', 2);
    fixture.detectChanges();
    stubDialog(fixture);
    fixture.componentRef.setInput('open', true);
    fixture.detectChanges();

    const employeeStore = TestBed.inject(EmployeeStore);

    return { fixture, component, mockHrApi, mockNotificationService, employeeStore };
  }

  function fillValidForm(component: EmployeeHireModalComponent) {
    component['form'].patchValue({
      assignment: { departmentId: 1, shiftId: 1, hireDate: daysFromToday(5), managerId: null },
      compensation: { initialPayRate: 25, payFrequency: 1, initialVacationHours: 40, initialSickLeaveHours: 24, notes: null },
    });
  }

  it('loads departments on init', async () => {
    const { component, mockHrApi } = await setup();

    expect(mockHrApi.getDepartments).toHaveBeenCalled();
    expect(component['departments']()).toEqual(mockDepartments);
  });

  it('blocks Next on step 1 until the assignment group is valid', async () => {
    const { component } = await setup();

    component['onNext']();

    expect(component['currentStep']()).toBe(1);
    expect(component['form'].controls.assignment.touched).toBe(true);
  });

  it('advances to step 2 once the assignment group is valid', async () => {
    const { component } = await setup();
    fillValidForm(component);

    component['onNext']();

    expect(component['currentStep']()).toBe(2);
  });

  it('wires aria-invalid and aria-describedby on the hire-date input when it has an error', async () => {
    const { fixture, component } = await setup();

    component['onNext']();
    fixture.detectChanges();

    const dateInput = fixture.nativeElement.querySelector('#aw-employee-hire-date-input');
    expect(dateInput.getAttribute('aria-invalid')).toBe('true');
    const describedBy = dateInput.getAttribute('aria-describedby');
    expect(describedBy).toContain('aw-employee-hire-date-error-required');
    expect(fixture.nativeElement.querySelector(`#${describedBy}`).textContent).toContain('Hire date is required.');
  });

  it('rejects a hire date more than 30 days in the future', async () => {
    const { component } = await setup();
    component['form'].controls.assignment.patchValue({ departmentId: 1, shiftId: 1, hireDate: daysFromToday(31) });

    component['onNext']();

    expect(component['currentStep']()).toBe(1);
    expect(component['hireDateErrors']()?.['maxFutureDate']).toBeTruthy();
  });

  it('calls EmployeeStore.hireEmployee with the correct EmployeeHire payload on confirm', async () => {
    const { component, mockHrApi } = await setup();
    fillValidForm(component);
    component['onNext']();
    component['onNext']();

    component['onConfirm']();

    const expectedHireDate = daysFromToday(5);
    const expected: EmployeeHire = {
      employeeId: 2,
      hireDate: expectedHireDate,
      departmentId: 1,
      shiftId: 1,
      managerId: null,
      initialPayRate: 25,
      payFrequency: 1,
      initialVacationHours: 40,
      initialSickLeaveHours: 24,
      notes: null,
    };
    expect(mockHrApi.hireEmployee).toHaveBeenCalledWith(2, expected);
  });

  it('closes the modal and emits hired on success', async () => {
    const { fixture, component } = await setup();
    fillValidForm(component);
    component['onNext']();
    component['onNext']();
    let emitted = false;
    component.hired.subscribe(() => (emitted = true));

    component['onConfirm']();
    fixture.detectChanges();

    expect(component['open']()).toBe(false);
    expect(emitted).toBe(true);
  });

  it('stays open and shows an error toast on failure', async () => {
    const { fixture, component, mockNotificationService } = await setup({ hireError: true });
    fillValidForm(component);
    component['onNext']();
    component['onNext']();

    component['onConfirm']();
    fixture.detectChanges();

    expect(component['open']()).toBe(true);
    expect(component['isSubmitting']()).toBe(false);
    expect(mockNotificationService.error).toHaveBeenCalledWith('Failed to hire employee. Please try again.');
  });

  it('sends optional numeric fields as undefined/null (not 0) when cleared to an empty string', async () => {
    const { component, mockHrApi } = await setup();
    fillValidForm(component);
    component['form'].controls.assignment.controls.managerId.setValue('' as unknown as number);
    component['onNext']();
    component['form'].controls.compensation.controls.initialVacationHours.setValue('' as unknown as number);
    component['form'].controls.compensation.controls.initialSickLeaveHours.setValue('' as unknown as number);
    component['onNext']();

    component['onConfirm']();

    const payload = mockHrApi.hireEmployee.mock.calls[0][1] as EmployeeHire;
    expect(payload.initialVacationHours).toBeUndefined();
    expect(payload.initialSickLeaveHours).toBeUndefined();
    expect(payload.managerId).toBeNull();
  });
});
