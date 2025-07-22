import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, Route, RouterLink } from '@angular/router';
import { MsalGuard, MsalRedirectComponent } from '@azure/msal-angular';
import { AuthService, ThemeService } from '@adventureworks-web/shared/util';

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

@Component({
  selector: 'aw-login-failed',
  template: `
    <div
      id="aw-login-failed"
      class="flex min-h-screen flex-col items-center justify-center gap-6 bg-base-100 text-center"
    >
      <h2 class="text-lg font-bold text-base-content">AdventureWorks</h2>
      <i class="fa-solid fa-circle-xmark text-6xl text-error" aria-hidden="true"></i>
      <h1 class="text-3xl font-bold text-base-content">Sign In Failed</h1>
      <p class="text-base-content/60">We were unable to sign you in. Please try again.</p>
      <button id="aw-login-failed-retry" class="btn btn-primary" (click)="authService.login()">
        <i class="fa-solid fa-right-to-bracket" aria-hidden="true"></i>
        Try Again
      </button>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class LoginFailedComponent {
  protected readonly authService = inject(AuthService);
  private readonly _themeService = inject(ThemeService);
}

export const appRoutes: Route[] = [
  { path: 'auth', component: MsalRedirectComponent },
  { path: 'login-failed', component: LoginFailedComponent },
  {
    path: '',
    loadComponent: () => import('@adventureworks-web/shared/app-layout').then((m) => m.AppLayoutComponent),
    canActivate: [MsalGuard],
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
