import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AuthService, ThemeService } from '@adventureworks-web/shared/util';

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
export class LoginFailedComponent {
  protected readonly authService = inject(AuthService);
  private readonly _themeService = inject(ThemeService);
}
