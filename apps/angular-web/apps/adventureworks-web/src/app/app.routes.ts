import { Route } from '@angular/router';
import { MsalGuard, MsalRedirectComponent } from '@azure/msal-angular';
import { LoginFailedComponent } from './login-failed/login-failed';
import { NotFoundComponent } from './not-found/not-found';
import { PlaceholderComponent } from './placeholder/placeholder';

export const appRoutes: Route[] = [
  { path: 'auth', component: MsalRedirectComponent },
  { path: 'login-failed', title: 'Sign In Failed', component: LoginFailedComponent },
  {
    path: '',
    loadComponent: () => import('@adventureworks-web/shared/app-layout').then((m) => m.AppLayoutComponent),
    canActivate: [MsalGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', title: 'Dashboard', data: { breadcrumb: 'Dashboard' }, component: PlaceholderComponent },
      {
        path: 'sales',
        data: { breadcrumb: 'Sales' },
        loadChildren: () => import('./routes/sales.routes').then((m) => m.salesRoutes),
      },
      {
        path: 'hr',
        data: { breadcrumb: 'Human Resources' },
        loadChildren: () => import('./routes/hr.routes').then((m) => m.hrRoutes),
      },
      {
        path: 'samples',
        title: 'Samples',
        data: { breadcrumb: 'Samples' },
        loadComponent: () => import('./samples/samples').then((m) => m.SamplesComponent),
      },
      { path: '**', title: 'Page Not Found', data: { breadcrumb: 'Not Found' }, component: NotFoundComponent },
    ],
  },
];
