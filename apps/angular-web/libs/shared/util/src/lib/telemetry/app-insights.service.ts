import { ErrorHandler, inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import {
  AngularPlugin,
  ApplicationinsightsAngularpluginErrorService,
} from '@microsoft/applicationinsights-angularplugin-js';
import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import { ENVIRONMENT } from '../environment/environment.token';

/** Initializes Azure Application Insights with the Angular plugin for page view tracking and exception logging. */
@Injectable({ providedIn: 'root' })
export class AppInsightsService {
  private readonly appInsights: ApplicationInsights | null = null;

  constructor() {
    const env = inject(ENVIRONMENT);
    const router = inject(Router);

    if (!env.appInsights?.connectionString || env.appInsights.connectionString.startsWith('__')) {
      return;
    }

    const angularPlugin = new AngularPlugin();

    this.appInsights = new ApplicationInsights({
      config: {
        connectionString: env.appInsights.connectionString,
        extensions: [angularPlugin],
        extensionConfig: {
          [angularPlugin.identifier]: { router },
        },
      },
    });

    this.appInsights.loadAppInsights();

    this.appInsights.addTelemetryInitializer((envelope) => {
      if (envelope.tags) {
        envelope.tags['ai.cloud.role'] = env.appInsights!.cloudRoleName;
      }
    });
  }

  /** Track a custom event. */
  trackEvent(name: string, properties?: Record<string, string>): void {
    this.appInsights?.trackEvent({ name }, properties);
  }

  /** Track an exception. */
  trackException(error: Error): void {
    this.appInsights?.trackException({ exception: error });
  }
}

/** Angular ErrorHandler that reports unhandled exceptions to Application Insights. */
export const appInsightsErrorHandler: ErrorHandler = new ApplicationinsightsAngularpluginErrorService();
