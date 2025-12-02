import { of, Subject, throwError } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { Department } from '@adventureworks-web/shared/data-access';
import { ApiValidationError, NotificationService } from '@adventureworks-web/shared/util';
import type { ValidationError } from '@adventureworks-web/shared/util';
import { renderDepartmentComponent } from '../testing/render-department-component';
import { DepartmentEditComponent } from './department-edit';

// Department 1 — Engineering, verified against the local AdventureWorks DB.
const mockDepartment: Department = {
  id: 1,
  name: 'Engineering',
  groupName: 'Research and Development',
  modifiedDate: '2008-04-30T00:00:00',
};

const allDepartments: Department[] = [
  mockDepartment,
  { id: 3, name: 'Sales', groupName: 'Sales and Marketing', modifiedDate: '2008-04-30T00:00:00' },
];

function buildRoute(id = '1', queryParams: Record<string, string> = {}) {
  return { snapshot: { paramMap: { get: vi.fn().mockReturnValue(id) }, queryParams } };
}

describe('DepartmentEditComponent', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  async function setup(
    options: {
      id?: string;
      queryParams?: Record<string, string>;
      department?: Department;
      departments?: Department[];
    } = {},
  ) {
    const route = buildRoute(options.id, options.queryParams);
    const mockHrApi = {
      getDepartment: vi.fn().mockReturnValue(of(options.department ?? mockDepartment)),
      getDepartments: vi.fn().mockReturnValue(of(options.departments ?? allDepartments)),
      updateDepartment: vi.fn().mockReturnValue(of(mockDepartment)),
    };
    const mockNotificationService = { error: vi.fn(), success: vi.fn() };
    // Spying on the prototype (rather than overriding the Router provider) keeps RouterLink
    // bindings in the template working against a real Router while still observing navigate().
    const navigateSpy = vi.spyOn(Router.prototype, 'navigate').mockResolvedValue(true);

    const { fixture, component } = await renderDepartmentComponent(DepartmentEditComponent, [
      provideTranslateService(),
      { provide: HrApiService, useValue: mockHrApi },
      { provide: ActivatedRoute, useValue: route },
      { provide: NotificationService, useValue: mockNotificationService },
    ]);

    fixture.detectChanges();

    return { fixture, component, mockHrApi, mockNotificationService, router: { navigate: navigateSpy } };
  }

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

  it('loads department and group names in parallel, patches the form', async () => {
    const { component, mockHrApi } = await setup();

    expect(mockHrApi.getDepartment).toHaveBeenCalledWith(1);
    expect(mockHrApi.getDepartments).toHaveBeenCalled();
    expect(component['form'].value.name).toBe('Engineering');
    expect(component['form'].value.groupName).toBe('Research and Development');
    expect(component['groupNameOptions']()).toEqual([
      { value: 'Research and Development', label: 'Research and Development' },
      { value: 'Sales and Marketing', label: 'Sales and Marketing' },
    ]);
    expect(component['isLoading']()).toBe(false);
  });

  it('shows error toast when load fails', async () => {
    const mockHrApi = {
      getDepartment: vi.fn().mockReturnValue(throwError(() => new Error('Server error'))),
      getDepartments: vi.fn().mockReturnValue(of(allDepartments)),
      updateDepartment: vi.fn(),
    };
    const mockNotificationService = { error: vi.fn(), success: vi.fn() };

    const { fixture, component } = await renderDepartmentComponent(DepartmentEditComponent, [
      provideTranslateService(),
      { provide: HrApiService, useValue: mockHrApi },
      { provide: ActivatedRoute, useValue: buildRoute() },
      { provide: NotificationService, useValue: mockNotificationService },
    ]);
    fixture.detectChanges();

    expect(mockNotificationService.error).toHaveBeenCalledWith('Failed to load department. Please try again.');
    expect(component['isLoading']()).toBe(false);
  });

  it('blocks submission and shows required errors when name is cleared', async () => {
    const { component, mockHrApi } = await setup();

    component['form'].controls.name.setValue('');
    component['onSubmit']();

    expect(component['submitted']()).toBe(true);
    expect(component['nameErrors']()).toEqual({ required: 'Name is required.' });
    expect(mockHrApi.updateDepartment).not.toHaveBeenCalled();
  });

  it('submits a valid update and navigates to the detail page', async () => {
    const { component, mockHrApi, router, mockNotificationService } = await setup();

    component['form'].setValue({ name: 'Engineering Updated', groupName: 'Research and Development' });
    component['onSubmit']();

    expect(mockHrApi.updateDepartment).toHaveBeenCalledWith(1, {
      id: 1,
      name: 'Engineering Updated',
      groupName: 'Research and Development',
    });
    expect(mockNotificationService.success).toHaveBeenCalledWith('Department updated successfully.');
    expect(router.navigate).toHaveBeenCalledWith(['/hr/departments', 1]);
  });

  it('surfaces the Rule-06 duplicate-name error inline even though propertyName is empty', async () => {
    const duplicateError: ValidationError = {
      propertyName: '',
      errorCode: 'Rule-06',
      errorMessage: 'A department with this name already exists.',
      correlationId: 'corr-2',
    };
    const { component, mockHrApi } = await setup();
    mockHrApi.updateDepartment.mockReturnValue(throwError(() => new ApiValidationError([duplicateError], 'corr-2')));

    component['form'].setValue({ name: 'Sales', groupName: 'Research and Development' });
    component['onSubmit']();

    expect(component['nameErrors']()).toEqual({ server: 'A department with this name already exists.' });
    expect(component['isSaving']()).toBe(false);
  });

  it('falls back to a generic toast for a non-validation update error', async () => {
    const { component, mockHrApi, mockNotificationService } = await setup();
    mockHrApi.updateDepartment.mockReturnValue(throwError(() => new Error('Server error')));

    component['form'].setValue({ name: 'Engineering Updated', groupName: 'Research and Development' });
    component['onSubmit']();

    expect(mockNotificationService.error).toHaveBeenCalledWith('Failed to update department. Please try again.');
    expect(component['isSaving']()).toBe(false);
  });

  it('isLoading and isSaving remain independent signals', async () => {
    const saveSubject = new Subject<Department>();
    const { component, mockHrApi } = await setup();
    mockHrApi.updateDepartment.mockReturnValue(saveSubject.asObservable());

    expect(component['isLoading']()).toBe(false);

    component['form'].setValue({ name: 'Engineering Updated', groupName: 'Research and Development' });
    component['onSubmit']();

    expect(component['isSaving']()).toBe(true);
    expect(component['isLoading']()).toBe(false);

    saveSubject.next(mockDepartment);
    saveSubject.complete();

    expect(component['isLoading']()).toBe(false);
  });

  it('second submit while saving is blocked (double-submit guard)', async () => {
    const { component, mockHrApi } = await setup();

    component['form'].setValue({ name: 'Engineering Updated', groupName: 'Research and Development' });
    component['onSubmit']();
    component['onSubmit']();

    expect(mockHrApi.updateDepartment).toHaveBeenCalledTimes(1);
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
