import { Injectable, inject } from '@angular/core';
import { EMPTY, catchError, forkJoin, switchMap } from 'rxjs';
import { PopupMessageService } from '../../../shared/services/popup-message.service';
import { ExchangeService } from '../../../shared/services/exchange.service';
import { convertCurrency } from '../../../shared/utils/currency-conversion.util';
import { BudgetEstimation } from '../../home/models/budget-estimation';
import { BudgetEstimationService } from '../../home/services/budget-estimation.service';

@Injectable({ providedIn: 'root' })
export class CapitalCurrencyService {
  private readonly estimationService = inject(BudgetEstimationService);
  private readonly exchangeService = inject(ExchangeService);
  private readonly popup = inject(PopupMessageService);

  offerBudgetRecalculation(capitalId: number, newCurrency: string): void {
    this.estimationService.findByCapital(capitalId)
      .pipe(catchError(() => EMPTY))
      .subscribe(estimations => {
        const stale = estimations.filter(item => item.budgetCurrency !== newCurrency);
        if (stale.length === 0) return;

        const from = [...new Set(stale.map(item => item.budgetCurrency))].join(', ');
        const noun = stale.length === 1 ? 'budget item is' : 'budget items are';

        this.popup.prompt(
          `Capital currency is now ${newCurrency}, but ${stale.length} ${noun} still in ${from}.`,
          "Recalculate at today's rate",
          () => this.recalculate(stale, newCurrency));
      });
  }

  private recalculate(estimations: BudgetEstimation[], currency: string): void {
    this.exchangeService.getLatest()
      .pipe(
        switchMap(exchanges => forkJoin(estimations.map(item =>
          this.estimationService.update(item.id, {
            description: item.description,
            budgetAmount: Math.round(convertCurrency(item.budgetAmount, item.budgetCurrency, currency, exchanges) * 100) / 100,
            budgetCurrency: currency,
            capitalId: item.capitalId,
            isCompleted: item.isCompleted
          }))))
      )
      .subscribe({
        next: () => this.popup.success(`Budget recalculated in ${currency}`),
        error: () => this.popup.error('Failed to recalculate budget')
      });
  }
}
