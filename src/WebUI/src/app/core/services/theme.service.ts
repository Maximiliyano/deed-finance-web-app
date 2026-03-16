import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'deed-theme';
  isDark: boolean;

  constructor() {
    const saved = localStorage.getItem(this.storageKey);
    if (saved) {
      this.isDark = saved === 'dark';
    } else {
      this.isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      localStorage.setItem(this.storageKey, this.isDark ? 'dark' : 'light');
    }
    this.apply();
  }

  toggle(): void {
    this.isDark = !this.isDark;
    localStorage.setItem(this.storageKey, this.isDark ? 'dark' : 'light');
    this.apply();
  }

  private apply(): void {
    document.documentElement.setAttribute('data-theme', this.isDark ? 'dark' : 'light');
  }
}
