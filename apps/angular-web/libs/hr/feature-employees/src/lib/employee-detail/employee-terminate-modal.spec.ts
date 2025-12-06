import { of, throwError } from 'rxjs';
import { provideTranslateService } from '@ngx-translate/core';
import type { EmployeeTerminate } from '@adventureworks-web/hr/data-access';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import { NotificationService } from '@adventureworks-web/shared/util';
import { renderEmployeeComponent } from '../testing/render-employee-component';
import { EmployeeTerminateModalComponent } from './employee-terminate-modal';

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

describe('EmployeeTerminateModalComponent', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  async function setup(options: { vacationHoursBalance?: number; terminateError?: boolean } = {}) {
    const mockHrApi = {
      terminateEmployee: vi
        .fn()
        .mockReturnValue(options.terminateError ? throwError(() => new Error('Network error')) : of({ message: 'ok' })),
      getEmployee: vi.fn().mockReturnValue(of({ id: 2 })),
    };
    const mockNotificationService = { error: vi.fn(), success: vi.fn(), info: vi.fn(), warning: vi.fn() };

    const { fixture, component } = await renderEmployeeComponent(EmployeeTerminateModalComponent, [
      provideTranslateService(),
      { provide: HrApiService, useValue: mockHrApi },
      { provide: NotificationService, useValue: mockNotificationService },
    ]);

    fixture.componentRef.setInput('employeeId', 2);
    fixture.componentRef.setInput('vacationHoursBalance', options.vacationHoursBalance ?? 0);
    fixture.detectChanges();
    stubDialog(fixture);
    fixture.componentRef.setInput('open', true);
    fixture.detectChanges();

    return { fixture, component, mockHrApi, mockNotificationService };
  }

  function fillValidDetails(component: EmployeeTerminateModalComponent) {
    component['form'].controls.details.patchValue({
      terminationDate: daysFromToday(5),
      terminationType: 'Voluntary',
      reason: 'Employee resigned.',
    });
  }

  it('wires aria-invalid and aria-describedby on the termination-date input when it has an error', async () => {
    const { fixture, component } = await setup();

    component['onNext']();
    fixture.detectChanges();

    const dateInput = fixture.nativeElement.querySelector('#aw-employee-terminate-date-input');
    expect(dateInput.getAttribute('aria-invalid')).toBe('true');
    const describedBy = dateInput.getAttribute('aria-describedby');
    expect(describedBy).toContain('aw-employee-terminate-date-error-required');
    expect(fixture.nativeElement.querySelector(`#${describedBy}`).textContent).toContain('Termination date is required.');
  });

  it('hides the PTO payout toggle when vacationHoursBalance is 0', async () => {
    const { fixture, component } = await setup({ vacationHoursBalance: 0 });
    fillValidDetails(component);
    component['onNext']();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#aw-employee-terminate-payout-pto-toggle')).toBeFalsy();
  });

  it('shows the PTO payout toggle when vacationHoursBalance is greater than 0', async () => {
    const { fixture, component } = await setup({ vacationHoursBalance: 10 });
    fillValidDetails(component);
    component['onNext']();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#aw-employee-terminate-payout-pto-toggle')).toBeTruthy();
  });

  it('exposes hasVacationBalance as false when vacationHoursBalance is 0', async () => {
    const { component } = await setup({ vacationHoursBalance: 0 });

    expect(component['hasVacationBalance']()).toBe(false);
  });

  it('exposes hasVacationBalance as true when vacationHoursBalance is greater than 0', async () => {
    const { component } = await setup({ vacationHoursBalance: 10 });

    expect(component['hasVacationBalance']()).toBe(true);
  });

  it('blocks Next when reason is empty', async () => {
    const { component } = await setup();
    component['form'].controls.details.patchValue({ terminationDate: daysFromToday(5), terminationType: 'Voluntary' });

    component['onNext']();

    expect(component['currentStep']()).toBe(1);
    expect(component['reasonErrors']()?.['required']).toBeTruthy();
  });

  it('calls EmployeeStore.terminateEmployee with the correct payload including payoutPto', async () => {
    const { component, mockHrApi } = await setup({ vacationHoursBalance: 10 });
    fillValidDetails(component);
    component['onNext']();
    component['form'].controls.options.patchValue({ eligibleForRehire: false, payoutPto: true, notes: 'note' });
    component['onNext']();

    component['onConfirm']();

    const expected: EmployeeTerminate = {
      employeeId: 2,
      terminationDate: daysFromToday(5),
      terminationType: 'Voluntary',
      reason: 'Employee resigned.',
      eligibleForRehire: false,
      payoutPto: true,
      notes: 'note',
    };
    expect(mockHrApi.terminateEmployee).toHaveBeenCalledWith(2, expected);
  });

  it('forces payoutPto to false when there is no vacation balance, even if the toggle was set', async () => {
    const { component, mockHrApi } = await setup({ vacationHoursBalance: 0 });
    fillValidDetails(component);
    component['onNext']();
    component['onNext']();

    component['onConfirm']();

    expect(mockHrApi.terminateEmployee).toHaveBeenCalledWith(2, expect.objectContaining({ payoutPto: false }));
  });

  it('closes the modal and emits terminated on success', async () => {
    const { fixture, component } = await setup();
    fillValidDetails(component);
    component['onNext']();
    component['onNext']();
    let emitted = false;
    component.terminated.subscribe(() => (emitted = true));

    component['onConfirm']();
    fixture.detectChanges();

    expect(component['open']()).toBe(false);
    expect(emitted).toBe(true);
  });

  it('stays open and shows an error toast on failure', async () => {
    const { fixture, component, mockNotificationService } = await setup({ terminateError: true });
    fillValidDetails(component);
    component['onNext']();
    component['onNext']();

    component['onConfirm']();
    fixture.detectChanges();

    expect(component['open']()).toBe(true);
    expect(mockNotificationService.error).toHaveBeenCalledWith('Failed to terminate employee. Please try again.');
  });
});
