import { NgOptimizedImage } from '@angular/common';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LanguageService, ThemeService } from '@adventureworks-web/shared/util';
import { filter, map, startWith } from 'rxjs';

interface Breadcrumb {
  label: string;
  url: string;
}

@Component({
  selector: 'aw-app-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NgOptimizedImage],
  templateUrl: './app-layout.html',
  styleUrl: './app-layout.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppLayoutComponent {
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly themeService = inject(ThemeService);
  private readonly languageService = inject(LanguageService);
  protected readonly currentYear = new Date().getFullYear();

  protected readonly breadcrumbs = signal<Breadcrumb[]>([]);
  protected readonly userMenuOpen = signal(false);

  private readonly salesNav = viewChild<ElementRef<HTMLElement>>('salesNav');
  private readonly hrNav = viewChild<ElementRef<HTMLElement>>('hrNav');
  private readonly userMenuTrigger = viewChild<ElementRef<HTMLButtonElement>>('userMenuTrigger');

  constructor() {
    afterNextRender(() => {
      this.router.events
        .pipe(
          filter((e): e is NavigationEnd => e instanceof NavigationEnd),
          map(() => this.buildBreadcrumbs()),
          startWith(this.buildBreadcrumbs()),
          takeUntilDestroyed(this.destroyRef),
        )
        .subscribe((crumbs) => {
          this.breadcrumbs.set(crumbs);

          this.salesNav()
            ?.nativeElement.querySelector('details')
            ?.toggleAttribute('open', this.router.url.startsWith('/sales'));
          this.hrNav()
            ?.nativeElement.querySelector('details')
            ?.toggleAttribute('open', this.router.url.startsWith('/hr'));
        });
    });
  }

  protected closeUserMenu(): void {
    this.userMenuOpen.set(false);
    this.userMenuTrigger()?.nativeElement.focus();
  }

  private buildBreadcrumbs(): Breadcrumb[] {
    const crumbs: Breadcrumb[] = [];
    let route: ActivatedRoute | null = this.activatedRoute.root;
    let url = '';

    while (route) {
      const children: ActivatedRoute[] = route.children;
      route = null;

      for (const child of children) {
        const segments = child.snapshot.url.map((seg) => seg.path);
        if (segments.length) {
          url += '/' + segments.join('/');
        }

        const label = child.snapshot.data['breadcrumb'] as string | undefined;
        if (label) {
          crumbs.push({ label, url: url || '/' });
        }

        route = child;
      }
    }

    return crumbs;
  }
}
