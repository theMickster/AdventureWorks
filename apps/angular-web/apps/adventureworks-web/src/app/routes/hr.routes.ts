import { Route } from '@angular/router';

export const hrRoutes: Route[] = [
  { path: '', redirectTo: 'employees', pathMatch: 'full' },
  {
    path: 'employees',
    title: 'Employees',
    data: { breadcrumb: 'Employees' },
    loadComponent: () =>
      import('@adventureworks-web/hr/feature-employees').then((m) => m.EmployeeListComponent),
  },
  // employees/new MUST come before employees/:id — Angular matches top-to-bottom and "new" would otherwise be treated as an id param
  {
    path: 'employees/new',
    title: 'New Employee',
    data: { breadcrumb: 'New Employee' },
    loadComponent: () =>
      import('@adventureworks-web/hr/feature-employees').then((m) => m.EmployeeCreateComponent),
  },
  {
    path: 'employees/:id',
    title: 'Employee Detail',
    data: { breadcrumb: 'Employee Detail' },
    loadComponent: () =>
      import('@adventureworks-web/hr/feature-employees').then((m) => m.EmployeeDetailComponent),
  },
  {
    path: 'org-chart',
    title: 'Organization Chart',
    data: { breadcrumb: 'Org Chart' },
    loadComponent: () => import('@adventureworks-web/hr/feature-org-chart').then((m) => m.OrgChartComponent),
  },
  {
    path: 'departments',
    title: 'Departments',
    data: { breadcrumb: 'Departments' },
    loadComponent: () =>
      import('@adventureworks-web/hr/feature-departments').then((m) => m.DepartmentListComponent),
  },
  // departments/new MUST come before departments/:id — Angular matches top-to-bottom and "new" would otherwise be treated as an id param
  {
    path: 'departments/new',
    title: 'New Department',
    data: { breadcrumb: 'New Department' },
    loadComponent: () =>
      import('@adventureworks-web/hr/feature-departments').then((m) => m.DepartmentCreateComponent),
  },
  {
    path: 'departments/:id',
    title: 'Department Detail',
    data: { breadcrumb: 'Department Detail' },
    loadComponent: () =>
      import('@adventureworks-web/hr/feature-departments').then((m) => m.DepartmentDetailComponent),
  },
  {
    path: 'departments/:id/edit',
    title: 'Edit Department',
    data: { breadcrumb: 'Edit Department' },
    loadComponent: () =>
      import('@adventureworks-web/hr/feature-departments').then((m) => m.DepartmentEditComponent),
  },
];
