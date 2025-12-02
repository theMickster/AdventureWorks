import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Provider, Type } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideMockEnvironment } from './provide-mock-environment';
import { provideMockAuthService } from './provide-mock-auth-service';
import { provideMockNotificationService } from './provide-mock-notification-service';
import { provideMockLoadingService } from './provide-mock-loading-service';
import { provideMockAppInsightsService } from './provide-mock-app-insights-service';

/** Options for {@link renderComponent}. */
export interface RenderComponentOptions {
  imports?: unknown[];
  providers?: Provider[];
}

/** Result of {@link renderComponent}. */
export interface RenderComponentResult<T> {
  fixture: ComponentFixture<T>;
  component: T;
  element: HTMLElement;
}

/**
 * Thin `TestBed` wrapper for standalone components. Provides `provideRouter([])` plus the 5 default
 * mock services; caller-supplied `providers` are spread last so they shadow the defaults (Angular DI
 * resolves the last provider for a token). Does not call `detectChanges()` — left to the test author.
 */
export async function renderComponent<T>(
  component: Type<T>,
  options: RenderComponentOptions = {},
): Promise<RenderComponentResult<T>> {
  await TestBed.configureTestingModule({
    imports: [component, ...(options.imports ?? [])],
    providers: [
      provideRouter([]),
      provideMockEnvironment(),
      provideMockAuthService(),
      provideMockNotificationService(),
      provideMockLoadingService(),
      provideMockAppInsightsService(),
      ...(options.providers ?? []),
    ],
  }).compileComponents();

  const fixture = TestBed.createComponent(component);
  await fixture.whenStable();

  return {
    fixture,
    component: fixture.componentInstance,
    element: fixture.nativeElement as HTMLElement,
  };
}
