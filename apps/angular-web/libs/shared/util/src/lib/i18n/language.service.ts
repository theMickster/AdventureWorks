import { inject, Injectable, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

const STORAGE_KEY = 'aw-lang';
const DEFAULT_LANG = 'en';

/** Manages the active language, persists preference to localStorage, and updates the HTML lang attribute. */
@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly translate = inject(TranslateService);

  /** The currently active language code (e.g., 'en', 'es'). */
  readonly currentLang = signal(DEFAULT_LANG);

  constructor() {
    const saved = localStorage.getItem(STORAGE_KEY);
    const lang = saved ?? DEFAULT_LANG;

    this.currentLang.set(lang);
    this.translate.use(lang);
    document.documentElement.lang = lang;
  }

  /** Switch to a new language. Persists the choice and updates the HTML lang attribute. */
  setLanguage(lang: string): void {
    this.currentLang.set(lang);
    this.translate.use(lang);
    localStorage.setItem(STORAGE_KEY, lang);
    document.documentElement.lang = lang;
  }
}
