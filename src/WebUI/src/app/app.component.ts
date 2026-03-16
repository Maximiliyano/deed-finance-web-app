import { Component, HostListener, OnInit } from '@angular/core';
import { AuthService } from './modules/auth/services/auth-service';
import { NotificationService } from './shared/services/notification.service';
import { ThemeService } from './core/services/theme.service';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    standalone: false
})
export class AppComponent implements OnInit {
    showScrollTop = false;

    constructor(
        private readonly authService: AuthService,
        private readonly notificationService: NotificationService,
        private readonly _theme: ThemeService
    ) {
        authService.me().subscribe();
    }

    ngOnInit(): void {
        if (this.notificationService.isSupported()) {
            this.notificationService.requestPermission();
        }
    }

    @HostListener('window:scroll')
    onScroll(): void {
        this.showScrollTop = window.scrollY > 400;
    }

    scrollToTop(): void {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
}
