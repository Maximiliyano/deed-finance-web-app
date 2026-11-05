import { Component, OnDestroy, OnInit } from "@angular/core";
import { CURRENCIES, Currency, CURRENCY_DEFAULT } from "../../../core/types/currency-type";
import { Subject, takeUntil } from "rxjs";
import { AuthService } from "../../../modules/auth/services/auth-service";
import { PopupMessageService } from "../../services/popup-message.service";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { UserSettingsService } from "../../../modules/home/services/user-settings.service";
import { UserSettings } from "../../../modules/home/models/user-settings.model";

@Component({
    selector: 'app-currency',
    templateUrl: 'currency.component.html',
    standalone: true,
    imports: [CommonModule, FormsModule]
})
export class CurrencyComponent implements OnInit, OnDestroy {
    currencies: string[] = CURRENCIES.map(x => x);
    userCurrency: string;

    private unsubscribe$ = new Subject<void>();

    constructor(
        private readonly authService: AuthService,
        private readonly popupMessageService: PopupMessageService,
        private readonly userSettingsService: UserSettingsService
    ) {}

    ngOnInit(): void {
        this.authService
            .me()
            .pipe(takeUntil(this.unsubscribe$))
            .subscribe({
                next: (user) => this.userCurrency = user?.currency ?? CURRENCY_DEFAULT
            });
    }

    ngOnDestroy(): void {
        this.unsubscribe$.next();
        this.unsubscribe$.complete();
    }

    onCurrencyChange(event: Event) {
        const newCurrency = (event.target as HTMLSelectElement).value;

        if (newCurrency) {
            this.userCurrency = newCurrency as Currency;

            this.userSettingsService.load()
                .pipe(takeUntil(this.unsubscribe$))
                .subscribe({
                    next: (settings) => {
                        const payload: UserSettings = { salary: settings?.salary ?? 0, currency: newCurrency };
                        this.userSettingsService.upsert(payload)
                            .pipe(takeUntil(this.unsubscribe$))
                            .subscribe({
                                next: () => this.popupMessageService.success(`Default currency updated to <b>${newCurrency}</b>`),
                            });
                    }
                });
        }
    }
}
