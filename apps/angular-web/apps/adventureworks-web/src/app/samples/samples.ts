import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'aw-samples',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './samples.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SamplesComponent {
  darkMode = signal(false);

  toggleTheme(): void {
    this.darkMode.update((v) => !v);
    const theme = this.darkMode() ? 'alpine-circuit-dark' : 'alpine-circuit';
    document.documentElement.dataset['theme'] = theme;
  }
}
