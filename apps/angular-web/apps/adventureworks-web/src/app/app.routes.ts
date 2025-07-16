import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, Route, RouterLink } from '@angular/router';

@Component({
  selector: 'aw-placeholder',
  template: `<h1 class="text-2xl font-bold text-base-content">{{ title }}</h1>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class PlaceholderComponent {
  readonly title: string;

  constructor() {
    const route = inject(ActivatedRoute);
    this.title = (route.snapshot.data['breadcrumb'] as string) || 'Page';
  }
}

@Component({
  selector: 'aw-not-found',
  imports: [RouterLink],
  template: `
    <div id="aw-not-found" class="flex flex-col items-center justify-center gap-6 py-20 text-center">
      <i class="fa-solid fa-triangle-exclamation text-6xl text-warning" aria-hidden="true"></i>
      <h1 class="text-3xl font-bold text-base-content">Page Not Found</h1>
      <p class="text-base-content/60">The page you're looking for doesn't exist or has been moved.</p>
      <a routerLink="/dashboard" class="btn btn-primary">
        <i class="fa-solid fa-house" aria-hidden="true"></i>
        Back to Dashboard
      </a>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class NotFoundComponent {}

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
      { path: '**', data: { breadcrumb: 'Not Found' }, component: NotFoundComponent },
    ],
  },
];
