import { Route } from '@angular/router';

export const manufacturingRoutes: Route[] = [
  { path: '', redirectTo: 'work-orders', pathMatch: 'full' },
  {
    path: 'work-orders',
    title: 'Work Orders',
    data: { breadcrumb: 'Work Orders' },
    loadComponent: () =>
      import('@adventureworks-web/manufacturing/feature-work-orders').then((m) => m.WorkOrderListComponent),
  },
  {
    path: 'work-orders/:id',
    title: 'Work Order Detail',
    data: { breadcrumb: 'Work Order Detail' },
    loadComponent: () =>
      import('@adventureworks-web/manufacturing/feature-work-orders').then((m) => m.WorkOrderDetailComponent),
  },
];
