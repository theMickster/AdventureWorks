import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'aw-hero-section',
  imports: [],
  templateUrl: './hero-section.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeroSectionComponent {}
