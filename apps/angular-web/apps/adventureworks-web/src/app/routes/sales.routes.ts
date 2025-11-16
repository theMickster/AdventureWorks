import { Route } from '@angular/router';

export const salesRoutes: Route[] = [
  { path: '', redirectTo: 'stores', pathMatch: 'full' },
  {
    path: 'stores',
    title: 'Stores',
    data: { breadcrumb: 'Stores' },
    loadComponent: () =>
      import('@adventureworks-web/sales/feature-stores').then((m) => m.StoreListComponent),
  },
  // stores/new MUST come before stores/:id — Angular matches top-to-bottom and "new" would otherwise be treated as an id param
  {
    path: 'stores/new',
    title: 'New Store',
    data: { breadcrumb: 'New Store' },
    loadComponent: () =>
      import('@adventureworks-web/sales/feature-stores').then((m) => m.StoreCreateComponent),
  },
  {
    path: 'stores/:id',
    title: 'Store Detail',
    data: { breadcrumb: 'Store Detail' },
    loadComponent: () =>
      import('@adventureworks-web/sales/feature-stores').then((m) => m.StoreDetailComponent),
  },
  {
    path: 'stores/:id/edit',
    title: 'Edit Store',
    data: { breadcrumb: 'Edit Store' },
    loadComponent: () =>
      import('@adventureworks-web/sales/feature-stores').then((m) => m.StoreEditComponent),
  },
  {
    path: 'persons',
    title: 'Sales Persons',
    data: { breadcrumb: 'Sales Persons' },
    loadComponent: () =>
      import('@adventureworks-web/sales/feature-sales-persons').then((m) => m.SalesPersonListComponent),
  },
];
