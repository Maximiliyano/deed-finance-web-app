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

  toggle(origin?: MouseEvent): void {
    const flip = (): void => {
      this.isDark = !this.isDark;
      localStorage.setItem(this.storageKey, this.isDark ? 'dark' : 'light');
      this.apply();
    };

    const doc = document as Document & { startViewTransition?: (update: () => void) => { ready: Promise<void> } };
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    if (!doc.startViewTransition || reduceMotion) {
      flip();
      return;
    }

    const target = origin?.currentTarget instanceof HTMLElement ? origin.currentTarget.getBoundingClientRect() : null;
    const x = target ? target.left + target.width / 2 : window.innerWidth / 2;
    const y = target ? target.top + target.height / 2 : 0;
    const radius = Math.hypot(Math.max(x, window.innerWidth - x), Math.max(y, window.innerHeight - y));

    doc.startViewTransition(flip).ready.then(() =>
      document.documentElement.animate(
        { clipPath: [`circle(0 at ${x}px ${y}px)`, `circle(${radius}px at ${x}px ${y}px)`] },
        { duration: 650, easing: 'cubic-bezier(0.16, 1, 0.3, 1)', pseudoElement: '::view-transition-new(root)' }
      )
    );
  }

  private apply(): void {
    document.documentElement.setAttribute('data-theme', this.isDark ? 'dark' : 'light');
  }
}
