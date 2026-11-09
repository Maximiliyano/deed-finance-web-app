import { ChangeDetectionStrategy, Component, NgZone, OnDestroy, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {AuthService} from './modules/auth/services/auth-service';


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

    ngOnInit(): void {
        this.zone.runOutsideAngular(() => window.addEventListener('scroll', this.onScroll, { passive: true }));

        this.authService.me().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
    }

    ngOnDestroy(): void {
        window.removeEventListener('scroll', this.onScroll);
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
