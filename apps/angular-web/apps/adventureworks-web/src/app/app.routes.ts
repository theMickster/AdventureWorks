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
      { path: 'dashboard', component: PlaceholderComponent },
      { path: 'sales/stores', component: PlaceholderComponent },
      { path: 'sales/persons', component: PlaceholderComponent },
      { path: 'hr/employees', component: PlaceholderComponent },
      { path: 'hr/departments', component: PlaceholderComponent },
      {
        path: 'samples',
        loadComponent: () => import('./samples/samples').then((m) => m.SamplesComponent),
      },
    ],
  },
];
