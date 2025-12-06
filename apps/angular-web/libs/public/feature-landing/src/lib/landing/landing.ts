import { ChangeDetectionStrategy, Component } from '@angular/core';
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
export class LandingComponent {}
