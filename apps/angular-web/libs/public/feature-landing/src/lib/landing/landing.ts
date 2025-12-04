import { ChangeDetectionStrategy, Component, effect, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@adventureworks-web/shared/util';
import { PublicNavComponent } from '../public-nav/public-nav';
import { HeroSectionComponent } from '../hero-section/hero-section';
import { FeatureHighlightsComponent } from '../feature-highlights/feature-highlights';
import { PublicFooterComponent } from '../public-footer/public-footer';

@Component({
  selector: 'aw-landing',
  imports: [PublicNavComponent, HeroSectionComponent, FeatureHighlightsComponent, PublicFooterComponent],
  templateUrl: './landing.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  constructor() {
    // MSAL always returns to '/' after loginRedirect(); bounce an already-authenticated
    // session (fresh login or a direct visit while signed in) straight to the dashboard.
    effect(() => {
      if (this.authService.isAuthenticated()) {
        void this.router.navigateByUrl('/dashboard');
      }
    });
  }
}
