import { of, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { Department } from '@adventureworks-web/shared/data-access';
import { ApiValidationError, NotificationService } from '@adventureworks-web/shared/util';
import type { ValidationError } from '@adventureworks-web/shared/util';
import { renderDepartmentComponent } from '../testing/render-department-component';
import { DepartmentCreateComponent } from './department-create';

const existingDepartments: Department[] = [
  { id: 1, name: 'Engineering', groupName: 'Research and Development', modifiedDate: '2008-04-30T00:00:00' },
  { id: 3, name: 'Sales', groupName: 'Sales and Marketing', modifiedDate: '2008-04-30T00:00:00' },
];

describe('DepartmentCreateComponent', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  async function setup(options: { departments?: Department[] } = {}) {
    const mockHrApi = {
      getDepartments: vi.fn().mockReturnValue(of(options.departments ?? existingDepartments)),
      createDepartment: vi.fn().mockReturnValue(of({ id: 17, name: 'Robotics', groupName: 'Research and Development', modifiedDate: '2026-01-01T00:00:00' })),
    };
    const mockNotificationService = { error: vi.fn(), success: vi.fn() };
    // Spying on the prototype (rather than overriding the Router provider) keeps RouterLink
    // bindings in the template working against a real Router while still observing navigate().
    const navigateSpy = vi.spyOn(Router.prototype, 'navigate').mockResolvedValue(true);

    const { fixture, component } = await renderDepartmentComponent(DepartmentCreateComponent, [
      provideTranslateService(),
      { provide: HrApiService, useValue: mockHrApi },
      { provide: NotificationService, useValue: mockNotificationService },
    ]);

    fixture.detectChanges();

    return { fixture, component, mockHrApi, mockNotificationService, router: { navigate: navigateSpy } };
  }

  it('derives group name options from the loaded department list', async () => {
    const { component } = await setup();

    expect(component['groupNameOptions']()).toEqual([
      { value: 'Research and Development', label: 'Research and Development' },
      { value: 'Sales and Marketing', label: 'Sales and Marketing' },
    ]);
  });

  it('blocks submission and shows required errors when name and groupName are empty', async () => {
    const { component, mockHrApi } = await setup();

    component['onSubmit']();

    expect(component['submitted']()).toBe(true);
    expect(component['nameErrors']()).toEqual({ required: 'Name is required.' });
    expect(component['groupNameErrors']()).toEqual({ required: 'Group name is required.' });
    expect(mockHrApi.createDepartment).not.toHaveBeenCalled();
  });

  it('submits a valid form and navigates to the new department detail page', async () => {
    const { component, mockHrApi, router, mockNotificationService } = await setup();

    component['form'].setValue({ name: 'Robotics', groupName: 'Research and Development' });
    component['onSubmit']();

    expect(mockHrApi.createDepartment).toHaveBeenCalledWith({ name: 'Robotics', groupName: 'Research and Development' });
    expect(mockNotificationService.success).toHaveBeenCalledWith('Department created successfully.');
    expect(router.navigate).toHaveBeenCalledWith(['/hr/departments', 17]);
  });

  it('surfaces the Rule-05 duplicate-name error inline on the name field', async () => {
    const duplicateError: ValidationError = {
      propertyName: 'Name',
      errorCode: 'Rule-05',
      errorMessage: 'A department with this name already exists.',
      correlationId: 'corr-1',
    };
    const { component, mockHrApi } = await setup();
    mockHrApi.createDepartment.mockReturnValue(throwError(() => new ApiValidationError([duplicateError], 'corr-1')));

    component['form'].setValue({ name: 'Engineering', groupName: 'Research and Development' });
    component['onSubmit']();

    expect(component['nameErrors']()).toEqual({ server: 'A department with this name already exists.' });
    expect(component['isSaving']()).toBe(false);
  });

  it('falls back to a generic toast for a non-validation error', async () => {
    const { component, mockHrApi, mockNotificationService } = await setup();
    mockHrApi.createDepartment.mockReturnValue(throwError(() => new Error('Server error')));

    component['form'].setValue({ name: 'Robotics', groupName: 'Research and Development' });
    component['onSubmit']();

    expect(mockNotificationService.error).toHaveBeenCalledWith('Failed to create department. Please try again.');
    expect(component['isSaving']()).toBe(false);
  });

  it('second submit while saving is blocked (double-submit guard)', async () => {
    const { component, mockHrApi } = await setup();

    component['form'].setValue({ name: 'Robotics', groupName: 'Research and Development' });
    component['onSubmit']();
    component['onSubmit']();

    expect(mockHrApi.createDepartment).toHaveBeenCalledTimes(1);
  });

  it('trims whitespace from the name before submitting', async () => {
    const { component, mockHrApi } = await setup();

    component['form'].setValue({ name: '  Robotics  ', groupName: 'Research and Development' });
    component['onSubmit']();

    expect(mockHrApi.createDepartment).toHaveBeenCalledWith(
      expect.objectContaining({ name: 'Robotics' }),
    );
  });
});
