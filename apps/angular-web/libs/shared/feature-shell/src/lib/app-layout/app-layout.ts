import { NgOptimizedImage } from '@angular/common';
import { afterNextRender, ChangeDetectionStrategy, Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter, map, startWith } from 'rxjs';

@Component({
  selector: 'aw-app-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NgOptimizedImage],
  templateUrl: './app-layout.html',
  styleUrl: './app-layout.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppLayoutComponent {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    afterNextRender(() => {
      this.router.events
        .pipe(
          filter((e): e is NavigationEnd => e instanceof NavigationEnd),
          map((e) => e.urlAfterRedirects),
          startWith(this.router.url),
          takeUntilDestroyed(this.destroyRef),
        )
        .subscribe((url) => {
          document
            .getElementById('aw-nav-sales')
            ?.querySelector('details')
            ?.toggleAttribute('open', url.startsWith('/sales'));
          document
            .getElementById('aw-nav-hr')
            ?.querySelector('details')
            ?.toggleAttribute('open', url.startsWith('/hr'));
        });
    });
  }
}
