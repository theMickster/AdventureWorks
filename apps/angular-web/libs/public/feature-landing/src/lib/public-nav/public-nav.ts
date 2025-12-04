import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NgOptimizedImage } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService, ThemeService } from '@adventureworks-web/shared/util';

@Component({
  selector: 'aw-public-nav',
  imports: [RouterLink, RouterLinkActive, NgOptimizedImage],
  templateUrl: './public-nav.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicNavComponent {
  protected readonly authService = inject(AuthService);
  protected readonly themeService = inject(ThemeService);
}
