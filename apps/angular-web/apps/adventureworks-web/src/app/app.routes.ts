import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, Route } from '@angular/router';

// Temporary placeholder component for routes until Story #591 adds proper pages
@Component({
  selector: 'aw-placeholder',
  template: `<h1 class="text-2xl font-bold text-base-content">{{ title }}</h1>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class PlaceholderComponent {
  title: string;

  constructor() {
    const route = inject(ActivatedRoute);
    const segments = route.snapshot.url.map((s) => s.path);
    this.title = segments.length ? segments.join(' / ') : 'Dashboard';
  }
}

export const appRoutes: Route[] = [
  {
    path: '',
    loadComponent: () => import('@adventureworks-web/shared/feature-shell').then((m) => m.AppLayoutComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', data: { breadcrumb: 'Dashboard' }, component: PlaceholderComponent },
      {
        path: 'sales',
        data: { breadcrumb: 'Sales' },
        children: [
          { path: '', redirectTo: 'stores', pathMatch: 'full' },
          { path: 'stores', data: { breadcrumb: 'Stores' }, component: PlaceholderComponent },
          { path: 'persons', data: { breadcrumb: 'Sales Persons' }, component: PlaceholderComponent },
        ],
      },
      {
        path: 'hr',
        data: { breadcrumb: 'Human Resources' },
        children: [
          { path: '', redirectTo: 'employees', pathMatch: 'full' },
          { path: 'employees', data: { breadcrumb: 'Employees' }, component: PlaceholderComponent },
          { path: 'departments', data: { breadcrumb: 'Departments' }, component: PlaceholderComponent },
        ],
      },
      {
        path: 'samples',
        data: { breadcrumb: 'Samples' },
        loadComponent: () => import('./samples/samples').then((m) => m.SamplesComponent),
      },
    ],
  },
];
