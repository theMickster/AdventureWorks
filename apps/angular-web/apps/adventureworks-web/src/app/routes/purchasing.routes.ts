import { Route } from '@angular/router';

export const purchasingRoutes: Route[] = [
  { path: '', redirectTo: 'vendors', pathMatch: 'full' },
  {
    path: 'vendors',
    title: 'Vendors',
    data: { breadcrumb: 'Vendors' },
    loadComponent: () =>
      import('@adventureworks-web/purchasing/feature-vendors').then((m) => m.VendorListComponent),
  },
];
