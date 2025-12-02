import { of, throwError } from 'rxjs';
import { provideTranslateService } from '@ngx-translate/core';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { Department } from '@adventureworks-web/shared/data-access';
import { NotificationService } from '@adventureworks-web/shared/util';
import { renderDepartmentComponent } from '../testing/render-department-component';
import { DepartmentListComponent } from './department-list';

// All 16 departments, verified against the local AdventureWorks DB (HumanResources.Department).
const allDepartments: Department[] = [
  { id: 1, name: 'Engineering', groupName: 'Research and Development', modifiedDate: '2008-04-30T00:00:00' },
  { id: 2, name: 'Tool Design', groupName: 'Research and Development', modifiedDate: '2008-04-30T00:00:00' },
  { id: 3, name: 'Sales', groupName: 'Sales and Marketing', modifiedDate: '2008-04-30T00:00:00' },
  { id: 4, name: 'Marketing', groupName: 'Sales and Marketing', modifiedDate: '2008-04-30T00:00:00' },
  { id: 5, name: 'Purchasing', groupName: 'Inventory Management', modifiedDate: '2008-04-30T00:00:00' },
  {
    id: 6,
    name: 'Research and Development',
    groupName: 'Research and Development',
    modifiedDate: '2008-04-30T00:00:00',
  },
  { id: 7, name: 'Production', groupName: 'Manufacturing', modifiedDate: '2008-04-30T00:00:00' },
  { id: 8, name: 'Production Control', groupName: 'Manufacturing', modifiedDate: '2008-04-30T00:00:00' },
  {
    id: 9,
    name: 'Human Resources',
    groupName: 'Executive General and Administration',
    modifiedDate: '2008-04-30T00:00:00',
  },
  { id: 10, name: 'Finance', groupName: 'Executive General and Administration', modifiedDate: '2008-04-30T00:00:00' },
  {
    id: 11,
    name: 'Information Services',
    groupName: 'Executive General and Administration',
    modifiedDate: '2008-04-30T00:00:00',
  },
  { id: 12, name: 'Document Control', groupName: 'Quality Assurance', modifiedDate: '2008-04-30T00:00:00' },
  { id: 13, name: 'Quality Assurance', groupName: 'Quality Assurance', modifiedDate: '2008-04-30T00:00:00' },
  {
    id: 14,
    name: 'Facilities and Maintenance',
    groupName: 'Executive General and Administration',
    modifiedDate: '2008-04-30T00:00:00',
  },
  { id: 15, name: 'Shipping and Receiving', groupName: 'Inventory Management', modifiedDate: '2008-04-30T00:00:00' },
  {
    id: 16,
    name: 'Executive',
    groupName: 'Executive General and Administration',
    modifiedDate: '2008-04-30T00:00:00',
  },
];

describe('DepartmentListComponent', () => {
  async function setup(options: { departments?: Department[]; error?: boolean } = {}) {
    const mockHrApi = {
      getDepartments: vi.fn().mockReturnValue(
        options.error ? throwError(() => new Error('Network error')) : of(options.departments ?? allDepartments),
      ),
    };
    const mockNotificationService = { error: vi.fn(), success: vi.fn() };

    const { fixture, component } = await renderDepartmentComponent(DepartmentListComponent, [
      provideTranslateService(),
      { provide: HrApiService, useValue: mockHrApi },
      { provide: NotificationService, useValue: mockNotificationService },
    ]);
    fixture.detectChanges();

    return { fixture, component, mockHrApi, mockNotificationService };
  }

  it('loads all 16 departments on init', async () => {
    const { component } = await setup();
    expect(component['departments']()).toHaveLength(16);
    expect(component['rows']()).toHaveLength(16);
  });

  it('derives 6 distinct group name options, sorted alphabetically', async () => {
    const { component } = await setup();
    expect(component['groupNameOptions']()).toEqual([
      'Executive General and Administration',
      'Inventory Management',
      'Manufacturing',
      'Quality Assurance',
      'Research and Development',
      'Sales and Marketing',
    ]);
  });

  it('filter narrows rows to the selected group and updates the count', async () => {
    const { component } = await setup();

    component['onGroupNameFilterChange']('Manufacturing');

    expect(component['filteredDepartments']()).toHaveLength(2);
    expect(component['rows']().map((r) => r['name'])).toEqual(['Production', 'Production Control']);
  });

  it('clearing the filter (empty string) restores the full list', async () => {
    const { component } = await setup();

    component['onGroupNameFilterChange']('Quality Assurance');
    expect(component['filteredDepartments']()).toHaveLength(2);

    component['onGroupNameFilterChange']('');
    expect(component['filteredDepartments']()).toHaveLength(16);
  });

  it('shows error notification when the load fails', async () => {
    const { component, mockNotificationService } = await setup({ error: true });

    expect(component['hasError']()).toBe(true);
    expect(component['isLoading']()).toBe(false);
    expect(mockNotificationService.error).toHaveBeenCalledWith('Failed to load departments. Please try again.');
  });

  it('rows map id, name, groupName, and formatted modifiedDate', async () => {
    const { component } = await setup({ departments: [allDepartments[0]] });

    const row = component['rows']()[0];
    expect(row['id']).toBe(1);
    expect(row['name']).toBe('Engineering');
    expect(row['groupName']).toBe('Research and Development');
    expect(typeof row['modifiedDate']).toBe('string');
  });
});
