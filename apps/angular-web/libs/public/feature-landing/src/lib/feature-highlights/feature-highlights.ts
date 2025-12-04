import { ChangeDetectionStrategy, Component } from '@angular/core';

interface FeatureHighlight {
  icon: string;
  heading: string;
  blurb: string;
}

const PLACEHOLDER_FEATURES: FeatureHighlight[] = [
  { icon: 'fa-chart-line', heading: 'Feature headline', blurb: 'Feature description placeholder — content TBD.' },
  { icon: 'fa-users', heading: 'Feature headline', blurb: 'Feature description placeholder — content TBD.' },
  { icon: 'fa-boxes-stacked', heading: 'Feature headline', blurb: 'Feature description placeholder — content TBD.' },
];

@Component({
  selector: 'aw-feature-highlights',
  imports: [],
  templateUrl: './feature-highlights.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FeatureHighlightsComponent {
  protected readonly features = PLACEHOLDER_FEATURES;
}
