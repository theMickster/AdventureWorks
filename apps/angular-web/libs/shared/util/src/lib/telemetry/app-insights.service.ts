import type { ApplicationInsights } from '@microsoft/applicationinsights-web';
import { ErrorHandler, inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ENVIRONMENT } from '../environment/environment.token';

type PendingCall =
  | { type: 'event'; name: string; properties?: Record<string, string> }
  | { type: 'exception'; error: Error };

/** Initializes Azure Application Insights with the Angular plugin for page view tracking and exception logging. */
@Injectable({ providedIn: 'root' })
export class AppInsightsService {
  private readonly env = inject(ENVIRONMENT);
  private readonly router = inject(Router);
  private sdk: ApplicationInsights | null = null;
  private readonly pending: PendingCall[] = [];

  /** Dynamically loads the App Insights SDK and flushes any queued events. Idempotent — safe to call more than once. */
  async initialize(): Promise<void> {
    if (this.sdk) {
      return;
    }

    const appInsights = this.env.appInsights;
    if (!appInsights?.connectionString || appInsights.connectionString.startsWith('__')) {
      return;
    }

    const [{ ApplicationInsights }, { AngularPlugin }] = await Promise.all([
      import('@microsoft/applicationinsights-web'),
      import('@microsoft/applicationinsights-angularplugin-js'),
    ]);

    const angularPlugin = new AngularPlugin();
    this.sdk = new ApplicationInsights({
      config: {
        connectionString: appInsights.connectionString,
        extensions: [angularPlugin],
        extensionConfig: { [angularPlugin.identifier]: { router: this.router } },
      },
    });

    this.sdk.loadAppInsights();
    this.sdk.addTelemetryInitializer((envelope) => {
      if (envelope.tags) {
        envelope.tags['ai.cloud.role'] = appInsights.cloudRoleName;
      }
    });

    for (const call of this.pending) {
      if (call.type === 'event') {
        this.sdk.trackEvent({ name: call.name }, call.properties);
      } else {
        this.sdk.trackException({ exception: call.error });
      }
    }
    this.pending.length = 0;
  }

  /** Track a custom event. Queued if called before initialize() completes. */
  trackEvent(name: string, properties?: Record<string, string>): void {
    if (this.sdk) {
      this.sdk.trackEvent({ name }, properties);
    } else {
      this.pending.push({ type: 'event', name, properties });
    }
  }

  /** Track an exception. Queued if called before initialize() completes. */
  trackException(error: Error): void {
    if (this.sdk) {
      this.sdk.trackException({ exception: error });
    } else {
      this.pending.push({ type: 'exception', error });
    }
  }
}

/** Angular ErrorHandler that routes unhandled exceptions to App Insights without a static SDK import. */
@Injectable()
export class AppInsightsErrorHandler implements ErrorHandler {
  private readonly appInsights = inject(AppInsightsService);

  handleError(error: unknown): void {
    this.appInsights.trackException(error instanceof Error ? error : new Error(String(error)));
    console.error(error);
  }
}
