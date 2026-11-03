import {ChangeDetectionStrategy, Component, HostListener, OnDestroy, OnInit, signal} from '@angular/core';
import {AuthService} from './modules/auth/services/auth-service';
import {NotificationService} from './shared/services/notification.service';
import {Subject, takeUntil} from 'rxjs';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    standalone: false,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements OnInit, OnDestroy {
    showScrollTop = signal(false);

    private unsubscribe$ = new Subject<void>();

    constructor(
        private readonly authService: AuthService,
        private readonly notificationService: NotificationService
    ) {}

    ngOnInit(): void {
        this.authService.me().pipe(takeUntil(this.unsubscribe$)).subscribe();

        if (this.notificationService.isSupported()) {
            this.notificationService.requestPermission();
        }
    }

    ngOnDestroy(): void {
        this.unsubscribe$.next();
        this.unsubscribe$.complete();
    }

    @HostListener('window:scroll')
    onScroll() {
      const show = window.scrollY > 300;
      if (show !== this.showScrollTop()) this.showScrollTop.set(show);
    }

    scrollToTop(): void {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
}
