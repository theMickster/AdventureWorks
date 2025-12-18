import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Provider, Type } from '@angular/core';
import { provideRouter } from '@angular/router';

/** Result of {@link renderOrgChartComponent}. */
export interface RenderOrgChartComponentResult<T> {
  fixture: ComponentFixture<T>;
  component: T;
}

/**
 * Local, lib-scoped equivalent of `@adventureworks-web/shared/util`'s test-only `renderComponent()`
 * helper (see `hr/feature-departments`'s `render-department-component.ts` for the original). That
 * helper is excluded from the public `shared/util` barrel, so it can't be imported across the
 * library boundary without violating `@nx/enforce-module-boundaries`. Neither org-chart component
 * injects `AuthService`, `LoadingService`, or `AppInsightsService`, so this only wires
 * `provideRouter([])` plus whatever the caller supplies.
 */
export async function renderOrgChartComponent<T>(
  component: Type<T>,
  providers: Provider[] = [],
): Promise<RenderOrgChartComponentResult<T>> {
  await TestBed.configureTestingModule({
    imports: [component],
    providers: [provideRouter([]), ...providers],
  }).compileComponents();

  const fixture = TestBed.createComponent(component);

  return { fixture, component: fixture.componentInstance };
}
