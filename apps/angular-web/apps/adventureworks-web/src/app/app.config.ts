import {
  ApplicationConfig,
  ErrorHandler,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import {
  HTTP_INTERCEPTORS,
  provideHttpClient,
  withFetch,
  withInterceptors,
  withInterceptorsFromDi,
} from '@angular/common/http';
import { provideRouter, TitleStrategy, withComponentInputBinding } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import {
  MSAL_GUARD_CONFIG,
  MSAL_INSTANCE,
  MSAL_INTERCEPTOR_CONFIG,
  MsalBroadcastService,
  MsalGuard,
  MsalInterceptor,
  MsalService,
} from '@azure/msal-angular';
import {
  AppInsightsErrorHandler,
  AppInsightsService,
  AuthService,
  AwTitleStrategy,
  correlationIdInterceptor,
  ENVIRONMENT,
  errorInterceptor,
  loadingInterceptor,
  msalGuardConfigFactory,
  msalInstanceFactory,
  msalInterceptorConfigFactory,
} from '@adventureworks-web/shared/util';
import { environment } from '../environments/environment';
import { appRoutes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    { provide: ErrorHandler, useClass: AppInsightsErrorHandler },
    provideHttpClient(
      withFetch(),
      withInterceptors([correlationIdInterceptor, loadingInterceptor, errorInterceptor]),
      withInterceptorsFromDi(),
    ),
    { provide: ENVIRONMENT, useValue: environment },
    provideRouter(appRoutes, withComponentInputBinding()),
    { provide: TitleStrategy, useClass: AwTitleStrategy },
    provideTranslateService({
      fallbackLang: 'en',
      loader: provideTranslateHttpLoader({ prefix: './i18n/' }),
    }),
    { provide: MSAL_INSTANCE, useFactory: msalInstanceFactory, deps: [ENVIRONMENT] },
    { provide: MSAL_GUARD_CONFIG, useFactory: msalGuardConfigFactory, deps: [ENVIRONMENT] },
    { provide: MSAL_INTERCEPTOR_CONFIG, useFactory: msalInterceptorConfigFactory, deps: [ENVIRONMENT] },
    { provide: HTTP_INTERCEPTORS, useClass: MsalInterceptor, multi: true },
    MsalService,
    MsalGuard,
    MsalBroadcastService,
    provideAppInitializer(() => inject(AuthService).initialize()),
    provideAppInitializer(() => inject(AppInsightsService).initialize()),
  ],
};
