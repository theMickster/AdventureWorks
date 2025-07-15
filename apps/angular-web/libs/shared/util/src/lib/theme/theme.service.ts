import { Injectable, signal } from '@angular/core';

const STORAGE_KEY = 'aw-theme';
const LIGHT_THEME = 'alpine-circuit';
const DARK_THEME = 'alpine-circuit-dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly darkMode = signal(false);

  constructor() {
    const saved = localStorage.getItem(STORAGE_KEY);

    if (saved) {
      this.darkMode.set(saved === DARK_THEME);
    } else {
      this.darkMode.set(globalThis.matchMedia?.('(prefers-color-scheme: dark)').matches ?? false);
    }

    this.applyTheme();
  }

  toggle(): void {
    this.darkMode.update((v) => !v);
    this.applyTheme();
    localStorage.setItem(STORAGE_KEY, this.darkMode() ? DARK_THEME : LIGHT_THEME);
  }

  private applyTheme(): void {
    document.documentElement.dataset['theme'] = this.darkMode() ? DARK_THEME : LIGHT_THEME;
  }
}
