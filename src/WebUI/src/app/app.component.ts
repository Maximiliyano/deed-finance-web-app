import {Component, NgZone, OnDestroy, OnInit} from '@angular/core';
import {AuthService} from './modules/auth/services/auth-service';
import {NotificationService} from './shared/services/notification.service';
import {ThemeService} from './core/services/theme.service';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    standalone: false
})
export class AppComponent implements OnInit, OnDestroy {
    showScrollTop = false;
  private scrollListener: (() => void) | null = null;

    constructor(
        private readonly authService: AuthService,
        private readonly notificationService: NotificationService,
        private readonly _theme: ThemeService,
        private readonly zone: NgZone
    ) {
        authService.me().subscribe();
    }

    ngOnInit(): void {
        if (this.notificationService.isSupported()) {
            this.notificationService.requestPermission();
        }

      this.zone.runOutsideAngular(() => {
        this.scrollListener = () => {
          const show = window.scrollY > 400;
          if (show !== this.showScrollTop) {
            this.zone.run(() => this.showScrollTop = show);
          }
        };
        window.addEventListener('scroll', this.scrollListener, {passive: true});
      });
    }

  ngOnDestroy(): void {
    if (this.scrollListener) {
      window.removeEventListener('scroll', this.scrollListener);
    }
    }

    scrollToTop(): void {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
}
