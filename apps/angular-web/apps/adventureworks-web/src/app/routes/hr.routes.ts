import { Route } from '@angular/router';
import { PlaceholderComponent } from '../placeholder/placeholder';

export const hrRoutes: Route[] = [
  { path: '', redirectTo: 'employees', pathMatch: 'full' },
  { path: 'employees', title: 'Employees', data: { breadcrumb: 'Employees' }, component: PlaceholderComponent },
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
