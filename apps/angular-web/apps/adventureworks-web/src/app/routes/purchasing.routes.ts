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
  {
    path: 'vendors/:id',
    title: 'Vendor Detail',
    data: { breadcrumb: 'Vendor Detail' },
    loadComponent: () =>
      import('@adventureworks-web/purchasing/feature-vendors').then((m) => m.VendorDetailComponent),
  },
  {
    path: 'purchase-orders/:id',
    title: 'Purchase Order Detail',
    data: { breadcrumb: 'Purchase Order Detail' },
    loadComponent: () =>
      import('@adventureworks-web/purchasing/feature-vendors').then((m) => m.PurchaseOrderDetailPlaceholderComponent),
  },
];
