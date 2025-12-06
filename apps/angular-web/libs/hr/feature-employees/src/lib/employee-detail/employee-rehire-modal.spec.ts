import { of, throwError } from 'rxjs';
import { provideTranslateService } from '@ngx-translate/core';
import type { EmployeeRehire } from '@adventureworks-web/hr/data-access';
import type { Department } from '@adventureworks-web/shared/data-access';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import { NotificationService } from '@adventureworks-web/shared/util';
import { renderEmployeeComponent } from '../testing/render-employee-component';
import { EmployeeRehireModalComponent } from './employee-rehire-modal';

const mockDepartments: Department[] = [{ id: 1, name: 'Engineering', groupName: 'Research and Development', modifiedDate: '2020-01-01' }];

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

describe('EmployeeRehireModalComponent', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  async function setup(options: { daysSinceTermination?: number | null; rehireError?: boolean } = {}) {
    const mockHrApi = {
      getDepartments: vi.fn().mockReturnValue(of(mockDepartments)),
      rehireEmployee: vi
        .fn()
        .mockReturnValue(
          options.rehireError ? throwError(() => new Error('Network error')) : of({ businessEntityId: 2, message: 'ok' }),
        ),
      getEmployee: vi.fn().mockReturnValue(of({ id: 2 })),
    };
    const mockNotificationService = { error: vi.fn(), success: vi.fn(), info: vi.fn(), warning: vi.fn() };

    const { fixture, component } = await renderEmployeeComponent(EmployeeRehireModalComponent, [
      provideTranslateService(),
      { provide: HrApiService, useValue: mockHrApi },
      { provide: NotificationService, useValue: mockNotificationService },
    ]);

    fixture.componentRef.setInput('employeeId', 2);
    fixture.componentRef.setInput('daysSinceTermination', options.daysSinceTermination ?? 90);
    fixture.detectChanges();
    stubDialog(fixture);
    fixture.componentRef.setInput('open', true);
    fixture.detectChanges();

    return { fixture, component, mockHrApi, mockNotificationService };
  }

  function fillValidForm(component: EmployeeRehireModalComponent) {
    component['form'].patchValue({
      assignment: { rehireDate: daysFromToday(1), departmentId: 1, shiftId: 1, managerId: null },
      compensation: { payRate: 30, payFrequency: 2, restoreSeniority: true, notes: null },
    });
  }

  it('wires aria-invalid and aria-describedby on the rehire-date input when it has an error', async () => {
    const { fixture, component } = await setup({ daysSinceTermination: 90 });

    component['onNext']();
    fixture.detectChanges();

    const dateInput = fixture.nativeElement.querySelector('#aw-employee-rehire-date-input');
    expect(dateInput.getAttribute('aria-invalid')).toBe('true');
    const describedBy = dateInput.getAttribute('aria-describedby');
    expect(describedBy).toContain('aw-employee-rehire-date-error-required');
    expect(fixture.nativeElement.querySelector(`#${describedBy}`).textContent).toContain('Rehire date is required.');
  });

  it('shows the exact "N more days" banner text when blocked, matching the US-758 AC (terminated 30 days ago -> "60 more days")', async () => {
    const { fixture } = await setup({ daysSinceTermination: 40 });

    const banner = fixture.nativeElement.querySelector('#aw-employee-rehire-blocked-banner');
    expect(banner.textContent.trim()).toBe('50 more days until this employee is eligible for rehire.');
  });

  it('is not blocked and hides the banner at exactly the 90-day cooling-off boundary', async () => {
    const { fixture, component } = await setup({ daysSinceTermination: 90 });

    expect(component['isBlocked']()).toBe(false);
    expect(fixture.nativeElement.querySelector('#aw-employee-rehire-blocked-banner')).toBeFalsy();
  });

  it('is blocked the day before the 90-day cooling-off boundary', async () => {
    const { fixture, component } = await setup({ daysSinceTermination: 89 });

    expect(component['isBlocked']()).toBe(true);
    expect(fixture.nativeElement.querySelector('#aw-employee-rehire-blocked-banner')).toBeTruthy();
  });

  it('treats null daysSinceTermination as not blocked (daysRemaining = 0)', async () => {
    const { fixture, component } = await setup({ daysSinceTermination: null });

    expect(component['daysRemaining']()).toBe(0);
    expect(component['isBlocked']()).toBe(false);
    expect(fixture.nativeElement.querySelector('#aw-employee-rehire-blocked-banner')).toBeFalsy();
  });

  it('disables the Confirm button while blocked', async () => {
    const { fixture, component } = await setup({ daysSinceTermination: 40 });
    fillValidForm(component);
    component['onNext']();
    fixture.detectChanges();

    const confirmBtn = fixture.nativeElement.querySelector('#aw-employee-rehire-confirm-btn');
    expect(confirmBtn.disabled).toBe(true);
  });

  it('enables the Confirm button once eligible and the form is valid', async () => {
    const { fixture, component } = await setup({ daysSinceTermination: 90 });
    fillValidForm(component);
    component['onNext']();
    fixture.detectChanges();

    const confirmBtn = fixture.nativeElement.querySelector('#aw-employee-rehire-confirm-btn');
    expect(confirmBtn.disabled).toBe(false);
  });

  it('calls EmployeeStore.rehireEmployee with the correct payload including restoreSeniority', async () => {
    const { component, mockHrApi } = await setup({ daysSinceTermination: 90 });
    fillValidForm(component);
    component['onNext']();

    component['onConfirm']();

    const expected: EmployeeRehire = {
      employeeId: 2,
      rehireDate: daysFromToday(1),
      departmentId: 1,
      shiftId: 1,
      managerId: null,
      payRate: 30,
      payFrequency: 2,
      restoreSeniority: true,
      notes: null,
    };
    expect(mockHrApi.rehireEmployee).toHaveBeenCalledWith(2, expected);
  });

  it('sends managerId as null (not 0) when cleared to an empty string', async () => {
    const { component, mockHrApi } = await setup({ daysSinceTermination: 90 });
    fillValidForm(component);
    component['form'].controls.assignment.controls.managerId.setValue('' as unknown as number);
    component['onNext']();

    component['onConfirm']();

    const payload = mockHrApi.rehireEmployee.mock.calls[0][1] as EmployeeRehire;
    expect(payload.managerId).toBeNull();
  });

  it('closes the modal and emits rehired on success', async () => {
    const { fixture, component } = await setup({ daysSinceTermination: 90 });
    fillValidForm(component);
    component['onNext']();
    let emitted = false;
    component.rehired.subscribe(() => (emitted = true));

    component['onConfirm']();
    fixture.detectChanges();

    expect(component['open']()).toBe(false);
    expect(emitted).toBe(true);
  });

  it('stays open and shows an error toast on failure', async () => {
    const { fixture, component, mockNotificationService } = await setup({ daysSinceTermination: 90, rehireError: true });
    fillValidForm(component);
    component['onNext']();

    component['onConfirm']();
    fixture.detectChanges();

    expect(component['open']()).toBe(true);
    expect(mockNotificationService.error).toHaveBeenCalledWith('Failed to rehire employee. Please try again.');
  });

  it('recomputes the minDate validator against the current date on every open, not a date frozen at construction time', async () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-01-10T00:00:00Z'));
    const { fixture, component } = await setup({ daysSinceTermination: 90 });

    component['form'].controls.assignment.controls.rehireDate.setValue('2026-01-11');
    expect(component['rehireDateErrors']()).toBeNull();

    // Advance the clock 3 days and close/reopen the modal (the component instance is never
    // destroyed between opens) — a validator frozen at construction time would still treat
    // 2026-01-11 as "not in the past" here, even though it now is.
    vi.setSystemTime(new Date('2026-01-13T00:00:00Z'));
    fixture.componentRef.setInput('open', false);
    fixture.detectChanges();
    fixture.componentRef.setInput('open', true);
    fixture.detectChanges();

    component['form'].controls.assignment.controls.rehireDate.setValue('2026-01-11');
    component['form'].controls.assignment.controls.rehireDate.markAsTouched();
    fixture.detectChanges();

    expect(component['rehireDateErrors']()?.['minDate']).toBeTruthy();

    vi.useRealTimers();
  });
});
