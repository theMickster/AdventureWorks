import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Provider, Type } from '@angular/core';
import { provideRouter } from '@angular/router';

/** Result of {@link renderEmployeeComponent}. */
export interface RenderEmployeeComponentResult<T> {
  fixture: ComponentFixture<T>;
  component: T;
}

/**
 * Local, lib-scoped equivalent of `@adventureworks-web/shared/util`'s test-only `renderComponent()`
 * helper. That helper is intentionally excluded from the public `shared/util` barrel (test-only code),
 * so it cannot be imported here across the library boundary without violating
 * `@nx/enforce-module-boundaries`. None of the employee components inject `AuthService`,
 * `LoadingService`, or `AppInsightsService`, so this helper only wires `provideRouter([])` plus
 * whatever the caller supplies — it does not need to replicate every default mock.
 */
export async function renderEmployeeComponent<T>(
  component: Type<T>,
  providers: Provider[] = [],
): Promise<RenderEmployeeComponentResult<T>> {
  await TestBed.configureTestingModule({
    imports: [component],
    providers: [provideRouter([]), ...providers],
  }).compileComponents();

  const fixture = TestBed.createComponent(component);

  return { fixture, component: fixture.componentInstance };
}
