import { Route } from '@angular/router';

export const appRoutes: Route[] = [
  {
    path: '',
    loadComponent: () => import('./home/home').then((m) => m.HomeComponent),
  },
  {
    path: 'samples',
    loadComponent: () =>
      import('./samples/samples').then((m) => m.SamplesComponent),
  },
];
