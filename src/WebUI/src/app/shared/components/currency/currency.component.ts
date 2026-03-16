import { Component, OnDestroy, OnInit } from "@angular/core";
import { CurrencyType } from "../../../core/types/currency-type";
import { getCurrencies } from "./functions/get-currencies.component";
import { Subject, takeUntil } from "rxjs";
import { AuthService } from "../../../modules/auth/services/auth-service";
import { stringToCurrencyEnum } from "./functions/string-to-currency-enum";
import { PopupMessageService } from "../../services/popup-message.service";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { UserSettingsService } from "../../../modules/home/services/user-settings.service";

@Component({
    selector: 'app-currency',
    templateUrl: 'currency.component.html',
    standalone: true,
    imports: [CommonModule, FormsModule]
})
export class CurrencyComponent implements OnInit, OnDestroy {
    currencies: { key: string, value: CurrencyType }[] = [];
    userCurrency: CurrencyType;

    private unsubscribe$ = new Subject<void>();

    constructor(
        private readonly authService: AuthService,
        private readonly popupMessageService: PopupMessageService,
        private readonly userSettingsService: UserSettingsService
    ) {}

    ngOnInit(): void {
        this.currencies = getCurrencies();
        this.authService
            .me()
            .pipe(takeUntil(this.unsubscribe$))
            .subscribe({
                next: (user) => this.userCurrency = stringToCurrencyEnum(user?.currency ?? '') ?? CurrencyType.UAH
            });
    }

    ngOnDestroy(): void {
        this.unsubscribe$.next();
        this.unsubscribe$.complete();
    }

    onCurrencyChange(event: Event) {
        const newCurrency = (event.target as HTMLSelectElement).value;

        if (newCurrency) {
            this.userCurrency = newCurrency as unknown as CurrencyType;

            this.userSettingsService.get()
                .pipe(takeUntil(this.unsubscribe$))
                .subscribe({
                    next: (settings) => {
                        const payload = { salary: settings?.salary ?? 0, currency: Number(newCurrency) };
                        this.userSettingsService.upsert(payload as any)
                            .pipe(takeUntil(this.unsubscribe$))
                            .subscribe({
                                next: () => this.popupMessageService.success(`Default currency updated to <b>${CurrencyType[Number(newCurrency)]}</b>`),
                            });
                    }
                });
        }
    }
}
