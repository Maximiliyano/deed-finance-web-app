import { DestroyRef, Injectable, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { UserSettingsService } from '../../modules/home/services/user-settings.service';
import { AuthService } from '../../modules/auth/services/auth-service';
import { ThemeSetting } from '../../modules/home/models/user-settings.model';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'deed-theme';
  private readonly userSettings = inject(UserSettingsService);
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly user = toSignal(this.auth.user$, { initialValue: null });

  isDark: boolean;

  constructor() {
    this.isDark = this.readCached() ?? window.matchMedia('(prefers-color-scheme: dark)').matches;
    this.apply();

    this.userSettings.settings$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(settings => {
      if (!settings?.theme || settings.theme === 'System') return;

      const dark = settings.theme === 'Dark';
      if (dark === this.isDark) return;

      this.isDark = dark;
      this.apply();
    });
  }

  toggle(origin?: MouseEvent): void {
    const flip = (): void => {
      this.isDark = !this.isDark;
      this.apply();
    };

    const doc = document as Document & { startViewTransition?: (update: () => void) => { ready: Promise<void> } };
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    if (!doc.startViewTransition || reduceMotion) {
      flip();
    } else {
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

    this.persist();
  }

  private persist(): void {
    const theme: ThemeSetting = this.isDark ? 'Dark' : 'Light';

    if (!this.user()) return;

    this.userSettings
      .patch({ theme })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ error: () => undefined });
  }

  private readCached(): boolean | null {
    try {
      const saved = localStorage.getItem(this.storageKey);
      return saved ? saved === 'dark' : null;
    } catch {
      return null;
    }
  }

  private apply(): void {
    document.documentElement.setAttribute('data-theme', this.isDark ? 'dark' : 'light');

    try {
      localStorage.setItem(this.storageKey, this.isDark ? 'dark' : 'light');
    } catch {
      return;
    }
  }
}
