import { Route } from '@angular/router';
import { PlaceholderComponent } from '../placeholder/placeholder';

export const salesRoutes: Route[] = [
  { path: '', redirectTo: 'stores', pathMatch: 'full' },
  {
    path: 'stores',
    title: 'Stores',
    data: { breadcrumb: 'Stores' },
    loadComponent: () =>
      import('@adventureworks-web/sales/feature-stores').then((m) => m.StoreListComponent),
  },
  {
    path: 'persons',
    title: 'Sales Persons',
    data: { breadcrumb: 'Sales Persons' },
    component: PlaceholderComponent,
  },
];
