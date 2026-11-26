import {
  ChangeDetectionStrategy,
  Component,
  NgZone,
  OnDestroy,
  OnInit,
  signal,
  inject,
  DestroyRef
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from './modules/auth/services/auth-service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements OnInit, OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly zone = inject(NgZone);
  private readonly destroyRef = inject(DestroyRef);

  showScrollTop = signal(false);

  private phaseTimer: number | undefined;

  ngOnInit(): void {
    this.zone.runOutsideAngular(() => {
      window.addEventListener('scroll', this.onScroll, { passive: true });
      this.applyPhase();
      this.phaseTimer = window.setInterval(() => this.applyPhase(), 5 * 60 * 1000);
    });

    this.authService.me().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
  }

  ngOnDestroy(): void {
    window.removeEventListener('scroll', this.onScroll);
    window.clearInterval(this.phaseTimer);
  }

  private applyPhase(): void {
    const hour = new Date().getHours();
    const phase =
      hour >= 5 && hour < 9 ? 'dawn' : hour >= 9 && hour < 17 ? 'day' : hour >= 17 && hour < 21 ? 'dusk' : 'night';
    document.documentElement.dataset['phase'] = phase;
  }

  private readonly onScroll = (): void => {
    const show = window.scrollY > 300;
    if (show !== this.showScrollTop()) {
      this.zone.run(() => this.showScrollTop.set(show));
    }
  };

  scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
