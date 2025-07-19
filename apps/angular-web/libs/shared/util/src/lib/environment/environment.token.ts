import { InjectionToken } from '@angular/core';
import { Environment } from './environment.model';

/** Injection token for the typed environment configuration. Provided in app.config.ts. */
export const ENVIRONMENT = new InjectionToken<Environment>('ENVIRONMENT');
