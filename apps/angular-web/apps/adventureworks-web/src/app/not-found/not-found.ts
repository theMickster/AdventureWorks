import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'aw-not-found',
  imports: [RouterLink],
  template: `
    <div id="aw-not-found" class="flex flex-col items-center justify-center gap-6 py-20 text-center">
      <i class="fa-solid fa-triangle-exclamation text-6xl text-warning" aria-hidden="true"></i>
      <h1 class="text-3xl font-bold text-base-content">Page Not Found</h1>
      <p class="text-base-content/60">The page you're looking for doesn't exist or has been moved.</p>
      <a routerLink="/dashboard" class="btn btn-primary">
        <i class="fa-solid fa-house" aria-hidden="true"></i>
        Back to Dashboard
      </a>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotFoundComponent {}
