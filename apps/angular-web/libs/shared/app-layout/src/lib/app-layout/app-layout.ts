import { NgOptimizedImage } from '@angular/common';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  computed,
  Component,
  DestroyRef,
  ElementRef,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ConfirmDialogComponent, ToastContainerComponent } from '@adventureworks-web/shared/ui';
import type { AwRouteData } from '@adventureworks-web/shared/util';
import {
  AppInsightsService,
  AuthService,
  LanguageService,
  LoadingService,
  SignalrService,
  SignalRConnectionStatus,
  ThemeService,
} from '@adventureworks-web/shared/util';
import { filter, map, startWith } from 'rxjs';

interface Breadcrumb {
  label: string;
  url: string;
}

@Component({
  selector: 'aw-app-layout',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    NgOptimizedImage,
    ToastContainerComponent,
    ConfirmDialogComponent,
  ],
  templateUrl: './app-layout.html',
  styleUrl: './app-layout.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppLayoutComponent {
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly authService = inject(AuthService);
  protected readonly themeService = inject(ThemeService);
  protected readonly loadingService = inject(LoadingService);
  protected readonly signalrService = inject(SignalrService);
  private readonly languageService = inject(LanguageService);
  private readonly appInsightsService = inject(AppInsightsService);
  protected readonly currentYear = new Date().getFullYear();

  protected readonly breadcrumbs = signal<Breadcrumb[]>([]);
  protected readonly userMenuOpen = signal(false);
  protected readonly signalrStatusLabel = computed(() => this.getSignalrStatusLabel(this.signalrService.connectionStatus()));

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

  protected logout(): void {
    this.userMenuOpen.set(false);
    this.authService.logout();
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

        const label = (child.snapshot.data as AwRouteData).breadcrumb;
        if (label) {
          crumbs.push({ label, url: url || '/' });
        }

        route = child;
      }
    }

    return crumbs;
  }

  private getSignalrStatusLabel(status: SignalRConnectionStatus): string {
    switch (status) {
      case 'connected':
        return 'Connected';
      case 'reconnecting':
      case 'connecting':
        return 'Reconnecting';
      case 'disconnected':
      default:
        return 'Disconnected';
    }
  }

}
