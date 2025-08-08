import { Route } from '@angular/router';
import { PlaceholderComponent } from '../placeholder/placeholder';

export const hrRoutes: Route[] = [
  { path: '', redirectTo: 'employees', pathMatch: 'full' },
  { path: 'employees', title: 'Employees', data: { breadcrumb: 'Employees' }, component: PlaceholderComponent },
  {
    path: 'departments',
    title: 'Departments',
    data: { breadcrumb: 'Departments' },
    component: PlaceholderComponent,
  },
];
